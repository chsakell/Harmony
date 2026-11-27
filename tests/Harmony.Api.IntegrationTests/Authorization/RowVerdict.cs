using System.Globalization;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>The three rules the matrix checks.</summary>
    public enum Rule
    {
        /// <summary>No Authorization header: 401 unless the endpoint is public (FR-020, FR-021).</summary>
        Anonymous,

        /// <summary>A logged-in user with no membership: 403 or 404 on resource endpoints (FR-030).</summary>
        NonMember,

        /// <summary>A logged-in user without the required permission: 403 on administration endpoints (FR-110 to FR-112).</summary>
        Admin,
    }

    /// <summary>The three callers of the admin rule.</summary>
    public enum AdminCaller
    {
        /// <summary>A user whose only role is Basic, which holds no permission (FR-110).</summary>
        PlainUser,

        /// <summary>A user holding every permission except the line's required one (FR-111).</summary>
        AllButRequired,

        /// <summary>A user holding only the line's required permission (FR-112).</summary>
        OnlyRequired,
    }

    /// <summary>A row outcome (spec, Definitions, <i>Row outcome</i>).</summary>
    public enum Outcome
    {
        Refused,
        LetIn,
        Inconclusive,
    }

    /// <param name="Status">The HTTP status code.</param>
    /// <param name="ReachedEndpoint">Routing selected the endpoint under test (FR-015).</param>
    /// <param name="MatchedEndpoint">What routing selected instead, or the endpoint itself; null when nothing was selected.</param>
    /// <param name="Body">The response body, shown for inconclusive rows.</param>
    public sealed record RowResponse(int Status, bool ReachedEndpoint, string? MatchedEndpoint, string Body);

    public sealed record Verdict(bool Passed, string Message);

    /// <summary>
    /// Turns a row's response and its line's mark into pass or fail (FR-015, FR-020, FR-021, FR-030, FR-033, FR-034,
    /// FR-040, FR-041). An inconclusive row never passes, whatever the mark.
    /// </summary>
    public static class RowVerdict
    {
        private const int BodyLimit = 2000;

        private static readonly int[] NonMemberInconclusive = [400, 401, 405, 415];

        private static readonly int[] OwnerInconclusive = [401, 403, 404];

        public static Outcome Classify(Rule rule, RowResponse response)
        {
            if (!response.ReachedEndpoint || response.Status == 405)
            {
                return Outcome.Inconclusive;
            }

            return rule switch
            {
                Rule.Anonymous => response.Status == 401 ? Outcome.Refused : Outcome.LetIn,
                Rule.NonMember when response.Status is 403 or 404 => Outcome.Refused,
                Rule.NonMember when NonMemberInconclusive.Contains(response.Status) => Outcome.Inconclusive,
                Rule.NonMember => Outcome.LetIn,
                Rule.Admin when response.Status == 401 => Outcome.Inconclusive,
                Rule.Admin when response.Status == 403 => Outcome.Refused,
                Rule.Admin => Outcome.LetIn,
                _ => throw new ArgumentOutOfRangeException(nameof(rule), rule, null),
            };
        }

        /// <summary>
        /// The admin rule (FR-110 to FR-112). A 401 means the login was not accepted, so the row proves nothing about the
        /// permission and is inconclusive, like a 405 or a request that did not reach the endpoint. The plain user and the
        /// user with every other permission pass only on 403; the user with only <paramref name="permission"/> passes on
        /// any other status.
        /// </summary>
        public static Verdict JudgeAdmin(AdminCaller caller, RowResponse response, string row, string permission)
        {
            var outcome = Classify(Rule.Admin, response);
            if (outcome == Outcome.Inconclusive)
            {
                return Fail($"{row} is inconclusive: {Describe(response)}. "
                    + "A 401 means the harness login was not accepted; fix the request or the harness user.");
            }

            var status = response.Status.ToString(CultureInfo.InvariantCulture);
            return (caller, outcome) switch
            {
                (AdminCaller.OnlyRequired, Outcome.Refused) => Fail($"{row} was refused with 403 to a user holding {permission}, "
                    + "the line's required permission; the endpoint needs another permission, or its line names the wrong one (FR-112)."),
                (AdminCaller.OnlyRequired, _) => Pass($"{row} lets in a user holding {permission} with {status}."),
                (_, Outcome.Refused) => Pass($"{row} is refused with 403 without {permission}."),
                _ => Fail($"{row} was let in with {status} without {permission}; expected 403. "
                    + $"Add [Authorize(Policy = {permission})] to the action (FR-110, FR-111)."),
            };
        }

        /// <param name="owner">The owner's request on its own fixture (FR-033); required for the non-member rule.</param>
        public static Verdict Judge(Rule rule, Mark mark, RowResponse response, string row, RowResponse? owner = null)
        {
            if (rule == Rule.Admin)
            {
                throw new ArgumentException("The admin rule is judged by JudgeAdmin.", nameof(rule));
            }

            if (rule == Rule.NonMember && mark == Mark.NotApplicable)
            {
                throw new ArgumentException("Only resource endpoints have non-member rows.", nameof(mark));
            }

            if (Classify(rule, response) == Outcome.Inconclusive)
            {
                return Fail($"{row} is inconclusive: {Describe(response)}. "
                    + "Fix the request in the harness or with a binding or body= on the line; an inconclusive row cannot be marked as a known gap.");
            }

            if (rule == Rule.NonMember)
            {
                ArgumentNullException.ThrowIfNull(owner);
                if (!owner.ReachedEndpoint || OwnerInconclusive.Contains(owner.Status))
                {
                    return Fail($"{row} is inconclusive: the owner's request on its own fixture got {Describe(owner)}, "
                        + "so the request does not prove anything about membership (FR-033). Fix it with a binding or body= on the line.");
                }
            }

            var outcome = Classify(rule, response);
            var status = response.Status.ToString(CultureInfo.InvariantCulture);
            var expected = rule == Rule.Anonymous ? "401" : "403 or 404";
            return mark switch
            {
                Mark.Refused when outcome == Outcome.Refused => Pass($"{row} is refused with {status}."),
                Mark.Refused => Fail($"{row} was let in with {status}; expected {expected}. "
                    + $"If this is a gap that is tracked, mark the line '{MatrixFile.GapMark(rule)}' and raise the baseline in review."),
                Mark.KnownGap when outcome == Outcome.LetIn => Pass($"{row} is let in with {status}, as its known-gap mark says."),
                Mark.KnownGap => Fail($"{row} is now refused with {status}: remove the known-gap mark '{MatrixFile.GapMark(rule)}' "
                    + "from its line (set it to 'refused') and lower the baseline by one (FR-041)."),
                Mark.NotApplicable when outcome == Outcome.LetIn => Pass($"{row} is public and answered {status}."),
                Mark.NotApplicable => Fail($"{row} is public but answered 401 (FR-021). Add [AllowAnonymous], or change its scope in review."),
                _ => throw new ArgumentOutOfRangeException(nameof(mark), mark, null),
            };
        }

        private static string Describe(RowResponse response)
        {
            var reached = response.ReachedEndpoint
                ? string.Empty
                : $"routing did not select the endpoint under test (it selected {response.MatchedEndpoint ?? "no endpoint"}), ";
            var body = response.Body.Length > BodyLimit ? response.Body[..BodyLimit] + "..." : response.Body;
            return $"{reached}status {response.Status.ToString(CultureInfo.InvariantCulture)}, body: {body}";
        }

        private static Verdict Pass(string message) => new(true, message);

        private static Verdict Fail(string message) => new(false, message);
    }
}
