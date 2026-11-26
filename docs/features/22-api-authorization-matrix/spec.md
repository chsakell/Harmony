---
id: 22
title: Api test host and authorization matrix
artifact: spec
status: draft            # draft | approved | superseded
owner: chsakell
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:spec-approved label is added
created: 2026-11-26
intent: ./intent.md
---

# Spec: Api test host and authorization matrix

## Summary

Harmony gets its first tests that send real HTTP requests through the core API. A new test project starts the Api in memory against a throwaway PostgreSQL database, with test logins, and with the broker, GitHub and every other outside call replaced by stand-ins. On that host an **authorization matrix** finds every endpoint by itself and checks two rules:

- an anonymous caller gets 401 unless the endpoint is deliberately public;
- a logged-in user who is not a member gets 403 or 404 on another user's workspace, board and everything inside it.

Today most rows fail. Each failing row is recorded as a known gap in one reviewed, checked-in **matrix file**. It is pinned like a `_KnownIssue` test and is never skipped. R-07 and R-08 remove those marks, and the list can only shrink. A new endpoint fails the run until someone states its scope. See [intent.md](./intent.md).

## Definitions

| Term | Meaning |
|---|---|
| **Test command** | `dotnet test --solution Harmony.slnx` (AGENTS.md). It needs no Docker. |
| **Api test project** | `tests/Harmony.Api.IntegrationTests`, named by the [testing strategy ADR](../../adr/2026-11-26-testing-strategy.md), rule 6. |
| **Api test command** | `dotnet test --project tests/Harmony.Api.IntegrationTests -p:HarmonyIntegrationTests=true`, which runs only the Api test project. It needs Docker and uses the same opt-in as the [R-03 integration tests](../19-integration-test-harness/spec.md). Requirement IDs in this project refer to this spec. |
| **Harmony Api** | The core API service, `src/Services/Harmony.Api`. |
| **Test host** | The Harmony Api's own startup (`Program`), run in memory by the Api test project. Only the overrides listed in NFR-003 differ. |
| **Harness** | The test code in the Api test project that: <br>- starts the disposable instance and the test host; <br>- creates users and fixtures; <br>- issues tokens; <br>- builds and sends matrix requests. |
| **Shared tag guard** | `RequirementTagGuard` in `tests/Shared/`, compiled into every test project ([traceability ADR](../../adr/2026-11-26-test-traceability-tags.md)). |
| **Disposable instance** | A database container that the run starts, uses and removes. It has a random host port and no named volume (as in R-03). |
| **Docker available** | A Docker daemon that runs Linux containers can be reached from the test process. |
| **Endpoint** | A controller action that the test host's routing serves. It is identified by its HTTP method and route template, exactly as routing reports them, for example `DELETE api/identity/role/{id}`. There were 106 on 2026-11-26. |
| **Management controllers** | The controllers in `src/Services/Harmony.Api/Controllers/Management/`. |
| **Other endpoints** | Routes in the test host that are not controller actions: <br>- the two health checks; <br>- the `/Error` page; <br>- the `index.html` fallback; <br>- the gRPC endpoints, meaning every method of the six gRPC services `CardService`, `UserCardService`, `BoardService`, `WorkspaceService`, `UserService` and `UserNotificationService`, plus the catch-all endpoints gRPC maps for unknown methods. <br>None of them is a row of the matrix (FR-013). Swagger and the `/Files` static files are served by middleware, not by routing, so they are neither endpoints nor other endpoints. |
| **Reaches the endpoint** | Routing selected the endpoint under test for the request, whether or not the action then ran. A 401 from the authentication handler after routing selected the endpoint counts as reaching it. The harness reads the selected endpoint through a test-only diagnostic. |
| **Matrix file** | A checked-in file in the Api test project with one line per endpoint. Each line states the endpoint's scope, any binding overrides (FR-031) and its known-gap marks (FR-040). It is the published answer to "which endpoints need a login". |
| **Scope** | What an endpoint acts on, stated on its line: <br>**public**: deliberately reachable without a login (the six endpoints in Design, *Public endpoints*); <br>**self**: needs a login and acts only on the caller's own data (profile, password, notification settings, the caller's own list of boards or workspaces, search); <br>**resource**: names a workspace or a board-scoped resource; <br>**administration**: role and user administration; <br>**user**: names another user's account or settings. <br>A **resource endpoint** is an endpoint whose scope is resource. |
| **Board-scoped resource** | A board, or something that belongs to exactly one board: list, card, sprint, checklist, checklist item, comment, label, link, attachment or issue type. |
| **Resource kinds** | Workspace plus the board-scoped resources. |
| **Owner** and **non-member** | Two users the harness creates once per run. Each has only the role that a newly registered user gets. Every fixture makes the owner a member of its workspace and the Admin of its board. The non-member is given no workspace or board membership in any fixture. |
| **Fixture** | A set of resources created directly in the test database for one request, not through the endpoints under test. It has a workspace, a private board with one of each resource kind, and a second card as a link target. Every fixture has new IDs. |
| **Name map** | The default rule for which resource kind an ID parameter or body property names. It is matched without regard to case: <br>`BoardId` → board; <br>`ListId`, `BoardListId` → list; <br>`CardId`, `SourceCardId` → card; `TargetCardId` → the second card; <br>`WorkspaceId` → workspace; <br>`SprintId`, `MoveToSprintId` → sprint; <br>`CheckListId` → checklist; `ListItemId` → checklist item; <br>`CommentId` → comment; `LabelId` → label; `LinkId` → link; `AttachmentId` → attachment; `IssueTypeId` → issue type; <br>`UserId` → the caller's own user ID. <br>A route `{id}`, a body `Id`, and any name the map gets wrong (for example `ListId` in a checklist command) are decided by an override on the endpoint's line. |
| **Row** | One check of one rule on one endpoint. The **anonymous rule** has one row per endpoint. The **non-member rule** has one row per resource endpoint. |
| **Row outcome** | <br>**Refused:** 401 for the anonymous rule; 403 or 404 for the non-member rule. <br>**Let in:** any other status from the endpoint under test, including 2xx with `Succeeded=false` and 5xx. A 5xx means the request got past authorization. <br>**Inconclusive:** the request did not reach the endpoint under test, or the non-member rule got 400, 401, 405 or 415. The cause is a broken request or token, not a gap. It is fixed in the harness or with a line override, never marked as a known gap. |
| **Known gap** | A mark on a line saying that a rule is let in today and which roadmap item closes it: R-07 for the anonymous rule, R-08 for the non-member rule. Together the marks are the **expected-failure list**. |
| **Request values** | How the harness fills a request: <br>- **Route values:** an ID gets a fixture ID (non-member rule) or a new random ID (anonymous rule); any other value gets a fixed placeholder of its type (`1` for numbers, a fixed text for strings) that meets the route's constraints. <br>- **Query parameters:** ID parameters are filled the same way, and the others are left out, so their defaults apply. <br>- **Body:** sent as JSON only for actions that have a body parameter. Content types other than JSON are out of scope, because no action takes one on 2026-11-26. |

## User stories

### US1 - No endpoint is open to anonymous callers by accident (P1)

As a maintainer or reviewer, I want every endpoint checked for a 401 when no login is sent, with today's gaps pinned in a reviewed list, so that R-07 can prove its fix and no new endpoint can ship open without someone deciding it.

- **Why this priority:** it needs only the test host and no test data, and it covers role and user administration, the worst gap. On its own it is the safety net R-07 needs.
- **Independent test:** run the Api test command filtered to FR-010 to FR-022. Every anonymous row passes, the known-gap rows among them included. The run reports about 94 anonymous known gaps (Design, *Expected sizes*), and an endpoint added without a line makes it fail.

### US2 - A non-member cannot act on another user's work (P1)

As a self-hosting team, I want every endpoint that names a workspace or board-scoped resource checked with a logged-in non-member, so that R-08 can prove that board membership is enforced everywhere.

- **Why this priority:** it is R-08's failing test. It builds on US1's host and matrix file.
- **Independent test:** run the Api test command filtered to FR-030 to FR-034. Every row passes, and the non-member known gaps are listed.

### US3 - The matrix runs on every pull request and is documented (P1)

As a reviewer, I want the matrix in the required `Integration tests` CI check, and the Api test command in AGENTS.md, so that an unprotected endpoint shows up as a failing check.

- **Why this priority:** success measure 3 ("fails on the pull request that adds it") holds only in CI, because the test command does not run the matrix. It needs a human edit to the workflow (Flagged concern 2).
- **Independent test:** this feature's pull request shows the Api tests running in the `Integration tests` check, and AGENTS.md documents the command.

## Functional requirements

| ID | Requirement | Story |
|---|---|---|
| FR-001 | When the Api test command runs with Docker available, the Api test project shall start one disposable PostgreSQL instance, start the test host against a new database on it, and remove the instance when the run ends, whether tests passed or failed. The host's own startup applies the migrations and runs the seeders. | US1 |
| FR-002 | When the test command runs, the test suite shall run none of the Api tests and shall pass without Docker. The solution build shall still compile the Api test project. | US1 |
| FR-003 | If Docker is not available when the Api test command runs, then the run shall exit with a non-zero code and report the selected tests as failed, with a message that contains `Docker`. | US1 |
| FR-004 | If the Api test command selects no tests, then the run shall exit with a non-zero code. | US1 |
| FR-005 | If a test method in the Api test project has no valid `Requirement` or `Bug` tag, then the shared tag guard shall fail and name the method. | US1 |
| FR-006 | While the Api tests run, the test host shall send nothing outside the test process: no broker message, gRPC call, HTTP request to GitHub or any other host, search-index update (Algolia or any other), email, log shipment or telemetry export. It shall write files only under a temporary folder that the run removes. Messages the Api publishes shall be recorded by a stand-in that tests can read. | US1 |
| FR-007 | When a test asks for a login for a user it created, the harness shall issue a token that the test host accepts as that user. The token carries the claim types of the tokens issued by `POST api/identity/token` (name identifier, email, name, surname, mobile phone and role) and is signed with a key generated for the run. The committed `AppConfiguration:Secret` shall not be used. | US2 |
| FR-010 | When the Api tests run, the matrix shall discover its endpoints from the test host's routing, not from a hand-kept list. | US1 |
| FR-011 | If a discovered endpoint has no line in the matrix file, then the matrix shall fail, naming the endpoint and saying that its scope must be stated. | US1 |
| FR-012 | If the matrix file has a line that matches no discovered endpoint, a duplicate line, a line out of sort order or an unknown scope, then the matrix shall fail and name the line. | US1 |
| FR-013 | If the test host serves an other endpoint that is not one of `/healthz/ready`, `/healthz/live`, `/Error`, the `index.html` fallback and the gRPC endpoints (see Definitions, *Other endpoints*), then the matrix shall fail and name it. A gRPC service other than the six named there also fails. | US1 |
| FR-014 | The Api test project shall report each row as its own test result whose display name contains the rule, the HTTP method and the route template. | US1 |
| FR-015 | If a matrix request does not reach the endpoint under test (for example it falls through to the `index.html` fallback, or gets 405), then the row shall fail as inconclusive. An inconclusive row never passes and cannot be marked as a known gap. | US1 |
| FR-020 | When a request with no `Authorization` header is sent to an endpoint whose scope is not public, the Harmony Api shall respond 401. The harness fills the request by the request-values rule, with new random IDs, and sends `{}` as the body of actions that have a body parameter. | US1 |
| FR-021 | When a request with no `Authorization` header is sent to an endpoint whose scope is public, the Harmony Api shall respond with a status other than 401. | US1 |
| FR-022 | If an endpoint allows anonymous access in code (`[AllowAnonymous]`) and its scope is not public, then the matrix shall fail and name it. | US1 |
| FR-030 | While the non-member has no membership in a fixture's workspace or board, when the non-member sends a request to a resource endpoint with that fixture's IDs, the Harmony Api shall respond 403 or 404. | US2 |
| FR-031 | When the harness builds a non-member request, it shall put the fixture's IDs in every route value, query value and body property that the name map or the endpoint's overrides assign a resource kind. This includes properties of nested objects, and a list of IDs gets one element. Body properties named `UserId` get the caller's own ID. Other body properties get their type's default value, and strings get a fixed placeholder. An endpoint's line may give a fixed body, or a different user for `UserId`, when the default cannot be bound or makes the owner request inconclusive; the harness still replaces the IDs in it. If an ID parameter or body property (a name ending in `Id`) maps to no resource kind, then the matrix shall fail and name it. | US2 |
| FR-032 | Each non-member request, and its owner request (FR-033), shall run against a new fixture of its own, so that one request's changes (for example a deleted board) cannot change another row's outcome. | US2 |
| FR-033 | When the owner sends the same request to the same endpoint against a new fixture, the Harmony Api shall respond with a status other than 401, 403 or 404. If it does not, then the row shall fail as inconclusive. | US2 |
| FR-034 | If the non-member request gets 400, 401, 405 or 415, then the row shall fail as inconclusive and show the response body. | US2 |
| FR-040 | Where an endpoint's line marks a rule as a known gap, that row shall pass only while the request is let in. It shall be reported under a test method whose name ends with `_KnownIssue`, and the mark shall name R-07 (anonymous rule) or R-08 (non-member rule). Known-gap rows carry both their rule's requirement tag (FR-020 or FR-030) and FR-040, so a filter on the rule selects all of its rows. | US1, US2 |
| FR-041 | If a row marked as a known gap is refused, then the row shall fail and say that the mark must be removed. | US1, US2 |
| FR-042 | The number of known-gap marks for each rule shall equal a baseline recorded in the Api test project. If the two differ, then the matrix shall fail and give both numbers. The baselines on merge are the counts this feature records (Design, *Expected sizes*). | US1, US2 |
| FR-050 | When the CI workflow runs on its existing triggers, the `Integration tests` job shall also run the Api test command and fail if any Api test fails, if none ran, or if Docker is unavailable. It shall publish the Api test summary to the run and upload the Api test results and coverage report as an artifact kept for 14 days, whatever the outcome. | US3 |

### Acceptance scenarios

All scenarios run on the test host with the Api test command unless they say otherwise.

- **FR-001**
  - Given Docker available, when the Api test command runs, then all Api tests run and pass. Within 60 seconds of the run ending, `docker ps -a` lists no instance the run started.
  - Given the test host has started, then `GET /healthz/ready` returns 200, the database's applied migrations equal the PostgreSQL migrations of `HarmonyContext`, and the roles created by the roles seeder exist.
- **FR-002**
  - Given Docker stopped, when the test command runs, then it exits with code 0 and its summary lists no test from the Api test project.
  - Given `dotnet build Harmony.slnx`, then the output includes the Api test project.
- **FR-003** - Given Docker stopped, when the Api test command runs, then the exit code is not 0 and the output contains `Docker`. No test is reported as passed or skipped.
- **FR-004** - Given a filter that matches no test, when the Api test command runs, then the exit code is not 0.
- **FR-005** - The shared guard runs in the Api test project. An untagged method added locally makes it fail and name the method. This is proven during verification, not committed.
- **FR-006**
  - Given the anonymous row for `POST api/test` (a debug endpoint mapped in every environment, which publishes an email notification), when it runs, then the recording stand-in holds that message and no broker connection is attempted.
  - Given the test host, then a test confirms that:
    - the broker publisher and the SourceControl gRPC client are the stand-ins;
    - the search service is the database search, and no Algolia client is registered;
    - the Elasticsearch URI and the OTLP endpoint are empty.
  - Given a test that uploads a valid file through `POST api/files` as the owner, then the file is written under the run's temporary folder, and nothing is written under `src/`.
- **FR-007**
  - Given a user created by the test and a token issued for them, when `GET api/board/userboards` (which has `[Authorize]`) is called with the token, then the response is not 401.
  - Given a token for the same user signed with a different, freshly generated key, then the same call does not get a 2xx response. Today it gets 500 (`OnAuthenticationFailed`), and R-11 owns that status.
- **FR-010** - Given the test host on 2026-11-26, then the matrix discovers 106 endpoints.
- **FR-011** - Given a controller action added locally without a line, then the matrix fails and names it. This is proven during verification, not committed.
- **FR-012** - Given a line removed from the middle of the file and added again at the end, then the matrix fails on sort order. Given a line for an endpoint that does not exist, then the matrix fails and names that line.
- **FR-013** - Given a minimal API endpoint (`MapGet`) added locally, then the matrix fails and names it. This is proven during verification, not committed.
- **FR-014** - Given a run, then the test results include one result per row, for example `Anonymous DELETE api/identity/role/{id}`.
- **FR-015** - Given a request whose route value breaks a `:guid` constraint and so falls through to the fallback, then the row is reported as inconclusive and not as let in. This is proven during verification by a deliberately broken route value.
- **FR-020**
  - Given the six endpoints with `[Authorize]` today, then an anonymous request gets 401. They are `PUT api/identity/account/UpdateProfile`, `PUT api/identity/account/ChangePassword`, `POST api/identity/account/profile-picture/{userId}` (all through the class attribute), `POST api/files`, `POST api/files/profile-picture` and `GET api/board/userboards`.
  - Given `DELETE api/identity/role/{id}`, then today the request is let in, so the row is a known gap for R-07 (FR-040).
- **FR-021** - Given each of the six public endpoints, then an anonymous request does not get 401.
- **FR-022** - Given `[AllowAnonymous]` added locally to `GET api/identity/user`, then the matrix fails and names it. This is proven during verification, not committed.
- **FR-030**
  - Given a fixture owned by the owner, when the non-member sends `POST api/board/{id:guid}/load` for the fixture's board, then the rule expects 403 or 404. Today the response is 200 with `Succeeded=false`, so the row is a known gap for R-08.
  - Given `POST api/board/{id:guid}/members` with the non-member's own user ID in the body, then the rule expects 403 or 404. Today the non-member can add themselves to the board, so the row is a known gap.
- **FR-031**
  - Given `PUT api/cards/{id:guid}/title`, then the route `{id}` and the body `CardId` both carry the fixture's card ID, and the body `BoardId` carries the fixture's board ID.
  - Given an endpoint with a body property `FooId` that neither the name map nor an override covers, then the matrix fails and names `FooId`.
- **FR-032** - Given the non-member row and the owner request for `DELETE api/checklists/{id:guid}`, then each deletes a checklist from its own fixture, and every other row's fixture still has its checklist.
- **FR-033** - Given each resource endpoint, then the owner's request is not answered 401, 403 or 404.
- **FR-034** - Given a body that cannot be bound, for example a wrong JSON type, then the row is inconclusive and shows the 400 response. This is proven during verification, not committed.
- **FR-040** - Given every row on the expected-failure list, then it passes today, and its result appears under a method ending with `_KnownIssue`.
- **FR-041** - Given a known-gap row whose endpoint gets `[Authorize]` locally, then the row fails and says to remove the mark. This is proven during verification, not committed.
- **FR-042** - Given one known-gap mark removed without lowering the baseline, then the matrix fails and gives both numbers.
- **FR-050** - Given this feature's pull request, then the `Integration tests` check runs the Api tests next to the data-layer tests, passes, and the run summary and artifact include the Api test results.

## Non-functional requirements

| ID | Category | Requirement (measurable) |
|---|---|---|
| NFR-001 | Performance | The Api test command finishes its test run in at most 5 minutes on `ubuntu-latest`, counted from the start of the run to the summary, image pull and container start included. The whole `Integration tests` job, with both integration projects, finishes in at most 15 minutes. Its timeout stays at 20 minutes. |
| NFR-002 | Isolation | The run never uses a fixed host port, a named volume, or a container or volume name that starts with `harmony-`. A developer's Aspire containers and data are never read, changed or stopped. This is verified once, as in R-03 NFR-002. |
| NFR-003 | Fidelity | The test host runs the Harmony Api's own startup. It differs only in: <br>**configuration:** `ConnectionStrings:HarmonyConnection`, `DatabaseProvider=PostgreSQL`, `AppConfiguration:Secret` (the key generated for the run), an empty `ElasticConfiguration:Uri`, `CacheProvider=InMemory`, no `OTEL_EXPORTER_OTLP_ENDPOINT`, and the environment name `Testing`, so the non-Development pipeline applies; <br>**services:** `INotificationsPublisher` and the SourceControl gRPC client are replaced by stand-ins; <br>**files:** the folder for uploaded files is the run's temporary folder. <br>Any other difference is a deviation recorded in `plan.md`. |
| NFR-004 | Fidelity | The PostgreSQL image tag is `postgres:17-alpine`, the tag `docker-compose.postgres.yml` uses on 2026-11-26. A test fails if the two differ. |
| NFR-005 | Reliability | The `Integration tests` job passes 3 runs in a row on this feature's pull request without code changes. Failed tests are never rerun automatically. No row's outcome depends on another row or on test order. |
| NFR-006 | Build | `dotnet build Harmony.slnx` reports 0 errors and no more than 585 code warnings. The Api test project builds with warnings as errors (from `tests/Directory.Build.props`). `dotnet format --verify-no-changes` passes for changed files. |
| NFR-007 | Licences | The one new package, `Microsoft.AspNetCore.Mvc.Testing`, is MIT-licensed. The PostgreSQL image is under the PostgreSQL Licence, which is OSI-approved. No other new package is added. |
| NFR-008 | No behaviour change | Production code changes only where the plan shows the test host cannot start without them, and each such change is listed and justified in `plan.md`. After them, the test command and the R-03 integration test command still pass. `dotnet run --project src/Aspire/Harmony.AppHost` still reaches `https://localhost:7108/healthz/ready` 200. |
| NFR-009 | Coverage | Every Api test run writes a Cobertura report under `TestResults/Harmony.Api.IntegrationTests/`, through the shared test settings. No threshold is set. |
| NFR-010 | Documentation | <br>**AGENTS.md** (Commands) documents the Api test command, that it needs Docker, and how to filter it by requirement. <br>**`docs/architecture/overview.md`** (Known issues): no longer says that Api host and authorization tests are missing; links the matrix file; states the size of each known-gap list. <br>**`docs/roadmap.md`**: R-07 and R-08 each gain a note with their list size and the matrix file path, and R-04's row is updated. |

## Design

### Components affected

See [overview](../../architecture/overview.md).

- **New: `tests/Harmony.Api.IntegrationTests`.**
  - References `Harmony.Api`.
  - Uses xUnit v3 on Microsoft.Testing.Platform, Shouldly, NSubstitute, coverlet.MTP, `Testcontainers.PostgreSql` (already in the central versions) and `Microsoft.AspNetCore.Mvc.Testing` (new, Flagged concern 1).
  - Opts in exactly like `Harmony.Infrastructure.IntegrationTests`: the project is a test application only when `HarmonyIntegrationTests=true`, so the test command never runs it (FR-002).
- **Harness:**
  - One PostgreSQL instance and one test host per run, shared through an assembly-level fixture.
  - The host's startup migrates and seeds its database. The seeders' admin account and roles exist in the test database, and no row relies on them.
  - Owners, non-members and fixtures are created directly through the host's `HarmonyContext` and Identity stores (FR-032).
  - Tokens are issued by the harness (FR-007).
  - The plan decides whether parts of the R-03 harness (Docker guard, image-tag check) are shared or copied. `tests/Shared/` may hold only code that depends on xUnit alone (traceability ADR).
- **Matrix:**
  - **Discovery:** endpoints are read from the host's routing data (FR-010). Other endpoints are checked against the fixed set in FR-013.
  - **Routing check:** the matrix confirms through test-only request diagnostics that each request reached the endpoint under test (FR-015).
  - **Results:** each row is one test result (FR-014). Known-gap rows run under `_KnownIssue` methods (FR-040).
- **Matrix file.**
  - **Contract:** one line per endpoint, sorted by route template and then HTTP method, so that each change is a one-line diff. Each line holds the method, the template, the scope, any binding overrides, and the known-gap marks with their roadmap item.
  - **Format:** chosen in the plan (carried forward).
  - **Review:** CODEOWNERS already assigns every path to @chsakell, so any change to a scope or a mark needs that review.
  - **Baselines:** the per-rule baselines (FR-042) live next to the file.
- **Production code:** no behaviour change (NFR-008). The likely testability points are named in Flagged concern 3.
- **CI (`.github/workflows/ci.yml`):** the `Integration tests` job gets a second test step for the Api test command, plus its summary and upload. The agent drafts the step and a human applies it (Flagged concern 2).
- **Docs:** AGENTS.md, the overview and the roadmap (NFR-010).

### Public endpoints

Agreed here as the starting list. R-07 may change it, and any change is a reviewed edit to the matrix file.

| Endpoint | Purpose | Marked `[AllowAnonymous]` today |
|---|---|---|
| `POST api/identity/token` | Log in | no (its controller has no `[Authorize]`) |
| `POST api/identity/token/refresh` | Refresh a token | no |
| `POST api/identity/user` | Register | yes |
| `GET api/identity/user/confirm-email` | Confirm an email address | yes |
| `POST api/identity/user/forgot-password` | Start a password reset | yes |
| `POST api/identity/user/reset-password` | Finish a password reset | yes |

When R-07 adds an authenticated-by-default policy, the two token endpoints need `[AllowAnonymous]`, or FR-021 fails. That is intended.

### Expected sizes

Read from the code on 2026-11-26 (`src/Services/Harmony.Api/Controllers/**`). The implementation records the exact baselines (FR-042) and updates this table if a number differs.

| Scope | Endpoints | Examples |
|---|---|---|
| public | 6 | see above |
| self | 10 | `api/identity/account/*`, `POST api/files/profile-picture`, `GET api/board/userboards`, `GET api/workspace`, `GET api/search`, `GET api/search/advanced`, `POST api/usernotifications`, `POST api/test` |
| administration | 8 | all 5 of `api/identity/role/*`, `GET api/identity/user`, `GET`/`PUT api/identity/user/roles/{id}` |
| user | 2 | `GET api/identity/user/{id}`, `GET api/usernotifications/{userId}` |
| resource | 80 | 78 of the 82 Management controller actions (all except `GET api/board/userboards`, `GET api/workspace` and the two `api/usernotifications` actions), `POST api/files`, `POST api/search/advanced` |
| **Total** | **106** | |

- **Anonymous known gaps:** 94. That is 106 endpoints, minus the 6 with `[Authorize]` (FR-020 scenario) and the 6 public ones.
- **Non-member known gaps:** expected 80. Every resource endpoint returns HTTP 200 in today's code, so none answers 403 or 404 (read from the code, not run). A row that turns out inconclusive is fixed in the harness or with a line override, never marked, so the recorded number can only be 80 or lower.

### API contract

No endpoint changes. The matrix observes only status codes and, for inconclusive rows, response bodies.

- **Refused:** 401 for the anonymous rule; 403 or 404 for the non-member rule (decided by chsakell, 2026-11-26).
- **A 2xx response with `Succeeded=false` counts as let in.** R-08 must therefore answer membership refusals with 403 or 404, not with today's 200 envelope (Flagged concern 4).

### Data model

No change. The Api tests use the existing PostgreSQL migrations through the host's startup.

### Events and integrations

The Api tests send no events and make no outside calls (FR-006).

- The broker publisher is replaced by a recording stand-in.
- The SourceControl gRPC client is replaced by a stand-in that answers in-process.
- Search already uses the database engine.
- Email is sent only by the Notifications service, so the publisher stand-in covers it.

### UX notes

Not applicable.

## Standards and policies applied

- **No `dotnet-instructions` standard is installed** (AGENTS.md). The spec follows the ADRs and the R-03 harness conventions.
- **[Testing strategy ADR](../../adr/2026-11-26-testing-strategy.md):**
  - xUnit v3 on Microsoft.Testing.Platform, Shouldly, NSubstitute and coverlet.MTP (rules 1 to 5).
  - The project name `Harmony.Api.IntegrationTests` (rule 6).
  - Known gaps are pinned under `_KnownIssue` and never skipped (rule 7, FR-040).
  - `Microsoft.AspNetCore.Mvc.Testing` goes in a separate `build:` pull request (rule 8, Flagged concern 1).
- **[Traceability ADR](../../adr/2026-11-26-test-traceability-tags.md):** every test is tagged, and the shared guard runs in the new project (FR-005).
- **[SQL Server and PostgreSQL ADR](../../adr/2026-11-25-sql-server-and-postgresql.md):** the host runs on one supported provider, configured by the host's own `AddPostgreDatabase` (NFR-003). Provider parity remains R-03's job.
- **R-03 spec #19:** the same opt-in, disposable-instance, Docker-guard and image-tag rules (FR-001 to FR-004, NFR-002, NFR-004).
- **AGENTS.md:** warning baseline, format rule, never skip a failing test, fix the code and not the test (NFR-006, FR-040).
- **SDLC guard:** agents cannot edit `.github/workflows/**` (Flagged concern 2).

## Flagged concerns

| # | Concern | Owner | Resolution |
|---|---|---|---|
| 1 | `Microsoft.AspNetCore.Mvc.Testing` is a new central package. Testing ADR rule 8 puts it in a separate `build:` pull request, which must merge before implementation. | chsakell | Open. The plan makes it task 0. |
| 2 | The SDLC guard blocks agent edits to `.github/workflows/ci.yml`, so FR-050 needs a human edit. | chsakell | Open. The agent drafts the step in the scratchpad, and chsakell applies it. `Integration tests` is already a required check. |
| 3 | Hosting the Api in a test may need small production changes: <br>(a) `Program` must be visible to `WebApplicationFactory`, which .NET 10 may generate as public by itself; <br>(b) the `/Files` static-file provider resolves from the process's current directory and throws at startup if the folder is missing, and uploads are written under it. | chsakell (tech lead) | Open. Preferred: no `src/` change, settled by a trial in the plan. Any change is justified there (NFR-008). |
| 4 | Refused means 403 or 404 only, so R-08 must also return real status codes for membership refusals. This overlaps R-24 (validation and error contract). | chsakell | Decided by chsakell, 2026-11-26. Add a note to R-08 and R-24 in this PR. |
| 5 | The 8 administration and 2 user endpoints (role administration, listing all users, changing roles, reading another user's profile or notification settings) are checked only by the anonymous rule. No row checks that a logged-in non-admin is refused there. | chsakell | Open. Proposed: R-07 adds an admin rule and its rows to this matrix. The scope column already marks them. |
| 6 | The matrix needs Docker, so it runs in the `Integration tests` CI job and not in the everyday test command. A contributor without Docker sees a new unprotected endpoint only on the pull request. That still meets success measure 3. | chsakell | Decided by chsakell, 2026-11-26 (PostgreSQL container). |
| 7 | Success measure 1 ("appears without anyone adding it by hand"): discovery is automatic and no endpoint can be missed (FR-010, FR-011). Each new endpoint still needs one line in the matrix file stating its scope, as the intent asks ("must state whether it is public, or the build fails"). | chsakell (intent owner) | Open. Confirm this reading at spec review. |
| 8 | Swagger, the `/Files` static files and the six gRPC services stay anonymous and outside the matrix (R-07, R-13, R-09). The FR-013 check stops new non-controller endpoints from slipping past. | chsakell | Proposed: accept. They are recorded under Out of scope. |
| 9 | "The list can only shrink" is enforced by FR-041 (a closed gap must be unmarked) and FR-042 (a strict count). A test cannot see git history, though, so a pull request that adds a new endpoint with a known-gap mark and also raises the baseline would pass. Only CODEOWNERS review stops it. | chsakell | Open. Proposed: accept review as the control, and make the baseline change stand out with a comment in the file. Alternative: a CI step that fails when a mark is added relative to the target branch. |

## Open questions

| Question (from intent or new) | Answer, or "carried forward" | Owner |
|---|---|---|
| Which database does the test host use? | A disposable PostgreSQL container (`postgres:17-alpine`). An in-memory store cannot run the host's startup migrations, and it rejects the bulk updates and transactions some handlers use. The Api tests therefore join the opt-in integration run. | chsakell, 2026-11-26 |
| What shape does the expected-failure list take, and how is it reviewed? | Known-gap marks on the lines of one checked-in matrix file, with a strict count baseline per rule (FR-040 to FR-042), reviewed through CODEOWNERS. Carried forward: the file format, chosen in the plan. | chsakell, in the plan |
| For the non-member rule, which endpoints count as naming a resource, and how does the matrix find the ID? | Endpoints with scope resource, 80 on 2026-11-26. IDs come from a new fixture per request and are placed by the name map plus per-line overrides. An unmapped ID fails the run (FR-031). | - |
| Should the matrix also cover the six gRPC services? | No: R-09, as the intent proposed. FR-013 pins them so that a new one is noticed. | - |
| Is the list of deliberately public endpoints agreed here or in R-07? | Assumed: agreed here as the six in Design, *Public endpoints*. R-07 may change the list with a reviewed edit. | chsakell |
| Should the Api tests cover the database seeders (carried from R-03)? | Carried forward. Proposed: R-04 proves only that they complete at host startup on PostgreSQL (FR-001). Tests of their behaviour belong with R-11, which changes the seeded admin account. | chsakell |

## Out of scope

- Fixing any gap the matrix shows (R-07, R-08).
- Board role rules (Admin manages members, Observers read only) and an admin-permission rule (R-08, R-07; Flagged concern 5).
- The six gRPC services (R-09), Swagger, and the `/Files` static files (R-07, R-13).
- The Automations, SourceControl, SignalR and gateway services (R-10).
- Expired, malformed or wrongly signed tokens beyond FR-007's one check, and login hardening (R-11).
- Functional checks beyond "let in" or "refused", including what a let-in request returns.
- Running the Api tests on SQL Server, and seeder behaviour.
- A page in the `harmony-docs` GitBook. The matrix file is the published answer for now. A user-facing page follows once R-07 has made the answer short.

## Rollout

1. A **`build:` pull request** adds `Microsoft.AspNetCore.Mvc.Testing` to `src/Directory.Packages.props` (Flagged concern 1). It merges first.
2. **This feature's pull request** adds the Api test project, the matrix file with today's known gaps, the docs, and the drafted CI step, which chsakell applies to `ci.yml`.
3. **R-07 and R-08** remove the marks and lower the baselines as they close each gap.

There is no runtime effect and nothing to migrate. To roll back, revert the pull request and remove the CI step. Contributors learn about the Api test command from AGENTS.md.
