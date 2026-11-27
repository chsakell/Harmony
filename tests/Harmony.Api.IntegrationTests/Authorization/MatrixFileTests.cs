using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The matrix file parser on in-memory text (plan, <i>Matrix file contract</i>). Pure code: no host.
    /// </summary>
    public class MatrixFileTests
    {
        private const string Header = "| Route | Method | Scope | Anonymous | Non-member | Bindings |";

        private static readonly string[] ValidLines =
        [
            "| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 | id=board |",
            "| api/CheckLists/{id:guid}/title | PUT | resource | gap R-07 | refused | id=checklist; ListId=checklist |",
            "| api/identity/role/{id} | DELETE | administration | gap R-07 | - |  |",
            "| api/identity/token | POST | public | - | - |  |",
        ];

        [Fact]
        [Trait("Requirement", "FR-042")]
        public void Parse_ValidFile_ReturnsLinesAndBaselines()
        {
            var document = MatrixFile.Parse(Document(ValidLines, anonymousGaps: 3, nonMemberGaps: 1));

            document.Baselines.ShouldBe(new Baselines(3, 1));
            document.Lines.Count.ShouldBe(4);
            var load = document.Lines[0];
            load.Route.ShouldBe("api/Board/{id:guid}/load");
            load.Method.ShouldBe("POST");
            load.Scope.ShouldBe(Scope.Resource);
            load.Anonymous.ShouldBe(Mark.KnownGap);
            load.NonMember.ShouldBe(Mark.KnownGap);
            load.Bindings.ShouldBe([new Binding("id", "board")]);
            load.FixedBody.ShouldBeNull();
            document.Lines[1].NonMember.ShouldBe(Mark.Refused);
            document.Lines[1].Bindings.ShouldBe([new Binding("id", "checklist"), new Binding("ListId", "checklist")]);
            document.Lines[2].NonMember.ShouldBe(Mark.NotApplicable);
            document.Lines[3].Anonymous.ShouldBe(Mark.NotApplicable);
        }

        [Fact]
        [Trait("Requirement", "FR-012")]
        public void Parse_FixedBodyAsLastBinding_KeepsJsonWithSemicolons()
        {
            var line = "| api/Board | POST | resource | gap R-07 | gap R-08 | WorkspaceId=workspace; body={\"Key\":\"MTRX\",\"Title\":\"a;b\"} |";

            var document = MatrixFile.Parse(Document([line], anonymousGaps: 1, nonMemberGaps: 1));

            document.Lines[0].Bindings.ShouldBe([new Binding("WorkspaceId", "workspace")]);
            document.Lines[0].FixedBody.ShouldBe("{\"Key\":\"MTRX\",\"Title\":\"a;b\"}");
        }

        [Fact]
        [Trait("Requirement", "FR-012")]
        public void Parse_LineMovedToEnd_FailsOnSortOrder()
        {
            string[] moved = [ValidLines[0], ValidLines[2], ValidLines[3], ValidLines[1]];

            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(Document(moved, 3, 1)));

            exception.Message.ShouldContain("out of order");
            exception.Message.ShouldContain(ValidLines[1]);
        }

        [Fact]
        [Trait("Requirement", "FR-012")]
        public void Parse_DuplicateLine_Fails()
        {
            string[] duplicated = [ValidLines[0], ValidLines[0], ValidLines[1], ValidLines[2], ValidLines[3]];

            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(Document(duplicated, 4, 2)));

            exception.Message.ShouldContain("duplicate");
            exception.Message.ShouldContain(ValidLines[0]);
        }

        [Fact]
        [Trait("Requirement", "FR-012")]
        public void Parse_UnknownScope_FailsNamingLine()
        {
            var line = "| api/Board/{id:guid}/load | POST | board | gap R-07 | gap R-08 | id=board |";

            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(Document([line], 1, 1)));

            exception.Message.ShouldContain("unknown scope 'board'");
            exception.Message.ShouldContain(line);
            exception.Message.ShouldContain($"line {HeaderLineNumber + 2}");
        }

        [Theory]
        [Trait("Requirement", "FR-040")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-08 | gap R-08 | id=board |")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-07 | id=board |")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-24 | gap R-08 | id=board |")]
        public void Parse_GapMarkNamesWrongItem_Fails(string line)
        {
            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(Document([line], 1, 1)));

            exception.Message.ShouldContain("must name");
            exception.Message.ShouldContain(line);
        }

        [Theory]
        [Trait("Requirement", "FR-012")]
        [InlineData("| api/identity/account/UpdateProfile | PUT | self | refused | refused |  |", "Non-member")]
        [InlineData("| api/identity/account/UpdateProfile | PUT | self | refused | gap R-08 |  |", "Non-member")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-07 | - | id=board |", "Non-member")]
        [InlineData("| api/identity/token | POST | public | refused | - |  |", "Anonymous")]
        [InlineData("| api/identity/user | GET | administration | - | - |  |", "Anonymous")]
        public void Parse_NonMemberMarkOnSelfScope_Fails(string line, string column)
        {
            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(Document([line], 0, 0)));

            exception.Message.ShouldContain($"{column} mark");
            exception.Message.ShouldContain(line);
        }

        [Theory]
        [Trait("Requirement", "FR-012")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 |", "6 cells")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | maybe | gap R-08 | id=board |", "unknown mark 'maybe'")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 | id=team |", "unknown kind 'team'")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 | id |", "name=kind")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 | body={}; id=board |", "body=")]
        [InlineData("| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 | body={ |", "JSON")]
        [InlineData("| api/Board/{id:guid}/load | post | resource | gap R-07 | gap R-08 | id=board |", "method")]
        public void Parse_MalformedLine_FailsNamingLine(string line, string expected)
        {
            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(Document([line], 1, 1)));

            exception.Message.ShouldContain(expected);
            exception.Message.ShouldContain(line);
        }

        [Fact]
        [Trait("Requirement", "FR-012")]
        public void Parse_WrongHeader_Fails()
        {
            var text = Document(ValidLines, 3, 1).Replace(Header, "| Route | Method | Scope | Anonymous | Bindings |", StringComparison.Ordinal);

            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(text));

            exception.Message.ShouldContain("header");
        }

        [Fact]
        [Trait("Requirement", "FR-042")]
        public void Parse_MissingBaseline_Fails()
        {
            var text = Document(ValidLines, 3, 1).Replace("- Non-member known gaps: 1", string.Empty, StringComparison.Ordinal);

            var exception = Should.Throw<MatrixFileException>(() => MatrixFile.Parse(text));

            exception.Message.ShouldContain("Non-member known gaps");
        }

        private const int HeaderLineNumber = 9;

        /// <summary>A file in the contract's shape; the header is on line <see cref="HeaderLineNumber"/>.</summary>
        private static string Document(IEnumerable<string> lines, int anonymousGaps, int nonMemberGaps) =>
            string.Join('\n',
            [
                "# Authorization matrix",
                "",
                "## Baselines",
                "",
                $"- Anonymous known gaps: {anonymousGaps}",
                $"- Non-member known gaps: {nonMemberGaps}",
                "",
                "## Endpoints",
                Header,
                "|---|---|---|---|---|---|",
                .. lines,
                "",
            ]);
    }
}
