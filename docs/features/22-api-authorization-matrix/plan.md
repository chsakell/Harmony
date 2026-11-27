---
id: 22
title: Api test host and authorization matrix
artifact: plan
status: approved            # draft | approved | superseded
owner: chsakell
approved-by: chsakell, 2026-11-26
created: 2026-11-26
spec: ./spec.md
---

# Plan: Api test host and authorization matrix

## Approach

- **New project.** `tests/Harmony.Api.IntegrationTests` copies the layout and opt-in of `Harmony.Infrastructure.IntegrationTests` (R-03):
  - xUnit v3 on MTP, Shouldly, NSubstitute and coverlet.MTP;
  - `OutputType Exe`, and `IsTestingPlatformApplication=false` unless `HarmonyIntegrationTests=true`;
  - `GlobalPropertiesToRemove` on the project reference;
  - the shared tag guard.
- **Test host.**
  - `WebApplicationFactory<Program>` runs the Api's own `Program`.
  - The configuration overrides in NFR-003 are passed with `UseSetting`. `Program` reads `DatabaseProvider`, the connection string, `AppConfiguration:Secret` and the endpoints from `builder.Configuration` *before* `Build()`, and only host settings are visible that early.
  - The host's `InitializeDatabase` migrates and seeds a new database on one lazily started `postgres:17-alpine` container.
  - `ConfigureTestServices` swaps in two stand-ins: a recording `INotificationsPublisher` and an NSubstitute `SourceControlServiceClient`.
- **Matrix.**
  - **Endpoints** come from the host's `EndpointDataSource`.
  - **Binding information** comes from ApiExplorer, matched by `ActionDescriptor`. It gives route, query and body parameters, and it flattens `[FromQuery]` objects.
  - **The matrix file** is a Markdown table that is reviewed and checked in. GitHub renders it, so it is also the published answer to "which endpoints need a login".
  - **Test cases:** each row is an xUnit v3 theory case built from the file, so cases can be listed without Docker. Known gaps run under `_KnownIssue` methods.
  - **Reaching the endpoint (FR-015):** a test-only `IStartupFilter` middleware records which endpoint routing selected for each request.
- **ADRs followed:**
  - testing strategy: rules 1-7, and rule 8 (the new package goes in a separate `build:` PR, T0);
  - traceability tags;
  - SQL Server and PostgreSQL: the host uses its own `AddPostgreDatabase`.
- **Production code:** no change is expected (NFR-008). See the decisions on Flagged concern 3.

### Decisions on the spec's open points

| Spec item | Decision |
|---|---|
| Flagged concern 1 (new package) | T0, a separate `build:` PR: `Microsoft.AspNetCore.Mvc.Testing` at `$(AspNetCoreVersion)` (10.0.15). |
| Flagged concern 2 (CI is human-only) | T11 drafts the step in the scratchpad, and chsakell applies it. |
| Flagged concern 3(a) (`Program` visibility) | No change expected. ASP.NET Core 10.0.15 ships `PublicProgramSourceGenerator`, which emits `public partial class Program` for top-level statements. T1 confirms it by compiling `WebApplicationFactory<Program>`. Fallback: one line, `public partial class Program;`, at the end of `Program.cs`, justified in the Deviations log. |
| Flagged concern 3(b) (`/Files` folder and uploads) | No `src/` change. `Program.cs:121` and `UploadService.cs:17` both resolve `Files` from `Directory.GetCurrentDirectory()`. Before the host starts, the harness creates `<temp>/h-api-<8 hex>/Files/` and sets the process's current directory to that temp folder. It restores the directory and deletes the folder in `DisposeAsync`. This is safe in the test process: the results directory is absolute (`tests/Directory.Build.props`), and `RepositoryRoot` walks up from `AppContext.BaseDirectory`. |
| Flagged concerns 5, 7, 8, 9 | The plan follows the spec's proposals: 5, an admin rule in R-07; 7, one line per endpoint is the intended reading; 8, accept; 9, CODEOWNERS review plus a warning comment above the baselines. **chsakell to confirm at plan review.** |
| Matrix file format (carried forward) | A Markdown table. See *Matrix file contract* below. |
| R-03 harness: share or copy | **Link** R-03's `Harness/DockerGuard.cs`, `Harness/ImageTags.cs` and `Harness/RepositoryRoot.cs` as `<Compile Include=... Link=...>`. They depend on Testcontainers, so they cannot move to `tests/Shared/`. The test-data builders are **copied** into `Harness/Fixtures.cs`, because R-03's `TestData` is bound to its `TestDatabase` type. |
| Seeders (open question) | As proposed: FR-001 proves only that they complete. On 2026-11-26, `DatabaseUsersSeeder` and `DatabaseWorkspaceSeeder` are inactive (`_active = false`). The roles seeder creates the Administrator and Basic roles and the admin user. |
| Route template spelling | Lines use routing's `RoutePattern.RawText` exactly, so `api/Board/{id:guid}/load`, `api/Cards/...`, `api/Labels/...`. The spec's lower-case examples (`api/board/...`) name the same endpoints. |
| Scope split | The inventory reproduces the spec: public 6, self 10, administration 8, user 2 and resource 80, for 106. The two user endpoints are `GET api/identity/user/{id}` and `GET api/UserNotifications/{userId}`. `POST api/Test` is self. |

### Matrix file contract

File: `tests/Harmony.Api.IntegrationTests/Authorization/authorization-matrix.md`. The tests read it from the source tree through `RepositoryRoot`.

- **Sections:**
  - A short intro saying what the file is.
  - `## Baselines`: two lines, `- Anonymous known gaps: <n>` and `- Non-member known gaps: <n>`. An HTML comment warns that raising a baseline adds a gap (concern 9).
  - `## Endpoints`: one table.
- **Columns:** `| Route | Method | Scope | Anonymous | Non-member | Bindings |`.
  - **Sorting:** by Route, then Method, both with `StringComparer.Ordinal`.
  - **Scope:** `public`, `self`, `resource`, `administration` or `user`.
  - **Anonymous:** `refused`, `gap R-07`, or `-` (only for public).
  - **Non-member:** `refused`, `gap R-08`, or `-` (only for scopes other than resource).
  - **Bindings:** empty, or `;`-separated items, used by the non-member rule:
    - `name=kind`. The name is a route value, a query parameter or a body property path (`IssueType.Id`), matched without regard to case. The kind is one of `workspace`, `board`, `list`, `card`, `card2`, `sprint`, `checklist`, `checklistitem`, `comment`, `label`, `link`, `attachment`, `issuetype`, `caller`, `owner` or `new` (a fresh random ID that names no resource).
    - Optionally `body=<json>` as the **last** item: a fixed body in which the harness still replaces the IDs.
- **Example lines:**
  - `| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 | id=board |`
  - `| api/CheckLists/{id:guid}/title | PUT | resource | gap R-07 | gap R-08 | id=checklist; ListId=checklist |`
- **The parser rejects (FR-012):**
  - a wrong header;
  - a wrong number of cells;
  - an unknown scope, mark or kind;
  - a mark that names the wrong item;
  - a mark that does not fit the scope;
  - duplicates;
  - lines out of order.

  Each message names the line number and its text.
- **Overrides the inventory already shows** (others come from the T9 dry run):
  - every route `{id}`;
  - `ListId=checklist` on `PUT api/CheckLists/{id:guid}/title`;
  - `IssueType.Id=issuetype` on `POST api/Cards`, `POST api/Cards/backlog`, `PUT api/Cards/backlog/{id:guid}` and `POST api/Sprints/{id:guid}/cards`;
  - `Id=workspace` on `PUT api/Workspace/{id:guid}/status` and `/rename`;
  - `skipCardId=card2` on `GET api/Cards/search`;
  - `UpdateId=new` on `PUT api/Cards/{id:guid}/move`;
  - fixed bodies for `POST api/Board` (the `Key` must match its pattern) and `POST api/Board/{id:guid}/sprints` (`StartDate` and `EndDate` are required).

### Request rules that refine FR-031 (implementation choices within the spec)

- A collection property named `<X>Ids` maps through `<X>Id` and gets one element: `IssueTypeIds` and `ListIds` on board load.
- An ID-named `string` property gets the ID's text form: `CreateBoardCommand.WorkspaceId`. A `Guid?` gets the ID.
- Properties whose names do not end in `Id`, for example `Cards` or `ListPositions`, get their type's default: empty collections. A line may give a fixed body when that makes the owner request inconclusive.
- Placeholders:
  - strings: `Matrix` (6 characters, which satisfies the minimum lengths of 5 and 6);
  - route numbers: `1`;
  - enums and body numbers: 0;
  - nested objects: built recursively, with depth at most 4.
- The client uses base address `https://localhost` with `AllowAutoRedirect=false`, so `UseHttpsRedirection` cannot redirect.
- Every request carries `X-Matrix-Request: <guid>` so that the recorder can match it.

## Files that change

| File | Change | Why |
|---|---|---|
| `src/Directory.Packages.props` | modify, **separate `build:` PR** (T0) | `Microsoft.AspNetCore.Mvc.Testing` `$(AspNetCoreVersion)`, MIT. It brings `Microsoft.AspNetCore.TestHost` (MIT). |
| `Harmony.slnx` | modify | Adds the project under `/tests/` (FR-002). |
| `tests/Harmony.Api.IntegrationTests/Harmony.Api.IntegrationTests.csproj` | new | Packages: xunit.v3, coverlet.MTP, NSubstitute, Shouldly, Testcontainers.PostgreSql and Mvc.Testing. References `Harmony.Api` with `GlobalPropertiesToRemove="HarmonyIntegrationTests"`. The opt-in block and its comment are copied from R-03. The three R-03 harness files are linked. |
| `.../AssemblyInfo.cs` | new | `[assembly: AssemblyFixture(typeof(ApiTestHost))]` |
| `.../Harness/ApiTestHost.cs` | new | Assembly fixture. `GetAsync()` starts these lazily, once: the PostgreSQL container (random port, no name or volume, through `DockerGuard`), the temp folder and current directory, the factory, the owner and non-member users, and the clients. `DisposeAsync` disposes only what started, then restores the current directory and deletes the folder. It exposes `Services`, `CreateClient(token?)`, `Owner`, `NonMember`, `Tokens`, `Publisher` and `Recorder`. |
| `.../Harness/HarmonyApiFactory.cs` | new | `WebApplicationFactory<Program>`. `UseEnvironment("Testing")`, plus `UseSetting` for the NFR-003 keys: the connection string to `h_<8 hex>`, `DatabaseProvider=PostgreSQL`, `AppConfiguration:Secret` (64 random bytes as base64), `ElasticConfiguration:Uri=""`, `CacheProvider=InMemory` and `OTEL_EXPORTER_OTLP_ENDPOINT=""`. `ConfigureTestServices` replaces `INotificationsPublisher` and `SourceControlService.SourceControlServiceClient` (`RemoveAll`, then add) and adds the recorder's startup filter. |
| `.../Harness/RecordingNotificationsPublisher.cs` | new | Records every `Publish*` call in a thread-safe list (FR-006). |
| `.../Harness/RequestRecorder.cs` | new | An `IStartupFilter`, the outermost middleware. After `next`, it records the endpoint for the request's `X-Matrix-Request`: `IExceptionHandlerFeature.Endpoint` when the exception handler re-executed to `/Error`, otherwise `HttpContext.GetEndpoint()`. |
| `.../Harness/TestUsers.cs` | new | Creates a user through the host's `UserManager<HarmonyUser>` and gives them the Basic role, as registration does (`UserService.cs:129`). |
| `.../Harness/TestTokens.cs` | new | Mirrors `IdentityService.GenerateEncryptedToken`: `JwtSecurityTokenHandler`, HmacSha256, the claims name identifier, email, name, surname, mobile phone and role, signed with the run key. It also has `ForKey(user, otherKey)` for the FR-007 negative check. |
| `.../Harness/Fixtures.cs` | new | The `Fixture` record (IDs of every resource kind) and `FixtureFactory.CreateAsync(owner)`. It writes through a host-scoped `HarmonyContext`: a workspace with the owner as member; a **private** board with the owner as `UserBoardAccess.Admin`; a list; an issue type; a sprint; two cards; a checklist and an item; a comment; a label; a link from card to card2; and an attachment. The builders are copied from R-03's `Harness/TestData.cs`. |
| `.../Authorization/authorization-matrix.md` | new | The matrix file (contract above). |
| `.../Authorization/MatrixFile.cs` | new | Parses and validates the file into `MatrixLine` (route, method, scope, anonymous mark, non-member mark, bindings, fixed body) and `Baselines`. Pure code, no host. |
| `.../Authorization/EndpointDiscovery.cs` | new | Reads the host's `EndpointDataSource`. It returns `DiscoveredEndpoint` (method, `RawText`, `AllowAnonymous` from `IAllowAnonymous` metadata, `ActionDescriptor`, `ApiDescription`) for controller actions, and `OtherEndpoint` for everything else. An action without exactly one HTTP method fails discovery. |
| `.../Authorization/MatrixCheck.cs` | new | Pure comparison of the discovered endpoints with the file. It reports FR-011 (missing line, with a ready-to-paste line in the message), FR-012 (line with no endpoint), FR-013 (an other endpoint outside the allowed set) and FR-022, and checks the baselines (FR-042). |
| `.../Authorization/RequestBuilder.cs` | new | Request values for both rules (FR-020, FR-031) from the `ApiDescription`, the line and a `Fixture` or random IDs. It fails and names any ID-named parameter or property that has no kind. |
| `.../Authorization/RowRunner.cs` | new | Sends the request and classifies the outcome as refused, let in or inconclusive, with the matched endpoint and the status. For the non-member rule it also runs the owner request on a new fixture (FR-032, FR-033). |
| `.../Authorization/RowVerdict.cs` | new | Turns a mark and an outcome into pass or fail with a message: FR-015, FR-034, FR-040 and FR-041 (`remove the known-gap mark`). |
| `.../Authorization/MatrixRows.cs` | new | Theory data from the file: `TheoryDataRow<string, string>` (method, route) with `TestDisplayName` such as `Anonymous DELETE api/identity/role/{id}` or `Non-member POST api/Board/{id:guid}/load`. One source per method. If a filter matches no line, it yields one placeholder row, `<Rule> (no line: every endpoint is marked)`. That row passes only while the file has no such line, so a theory never fails for lack of data and is never skipped. |
| `.../Conventions/RequirementTagGuardTests.cs` | new | FR-005; copied from R-03. |
| `.../Conventions/ImageTagTests.cs` | new | NFR-004 |
| `.../Harness/ApiTestHostTests.cs` | new | FR-001, FR-003, NFR-002 |
| `.../Harness/StandInTests.cs` | new | FR-006, NFR-003 |
| `.../Harness/TestTokenTests.cs` | new | FR-007 |
| `.../Authorization/MatrixFileTests.cs` | new | FR-012, FR-040 (mark rules), FR-042 (parsing), on in-memory text |
| `.../Authorization/MatrixCheckTests.cs` | new | FR-011, FR-012, FR-013, FR-022 and FR-042 on synthetic `RouteEndpoint`s. One live test checks the real host and file. |
| `.../Authorization/RowVerdictTests.cs` | new | FR-015, FR-034, FR-040, FR-041 |
| `.../Authorization/RequestBuilderTests.cs` | new | FR-031 |
| `.../Authorization/FixtureIsolationTests.cs` | new | FR-032 |
| `.../Authorization/RowRunnerTests.cs` | new | FR-015 against the live host |
| `.../Authorization/AnonymousMatrixTests.cs` | new | FR-014, FR-020, FR-021, FR-040 |
| `.../Authorization/NonMemberMatrixTests.cs` | new | FR-030, FR-033, FR-034, FR-040 |
| `AGENTS.md` | modify (Commands) | The Api test command, that it needs Docker, and how to filter it (NFR-010). |
| `docs/architecture/overview.md` | modify (Known issues lines 112-113, CI/CD line 101) | NFR-010 |
| `docs/roadmap.md` | modify | R-04 row; notes on R-07 (list size, matrix path, admin rule from concern 5); notes on R-08 (list size, path, 403/404 from concern 4); a note on R-24 (concern 4). |
| `docs/features/22-api-authorization-matrix/spec.md` | modify | Approval front matter now. In T10, *Expected sizes* only if a recorded number differs. |
| `.github/workflows/ci.yml` | **drafted by the agent in the scratchpad, applied by chsakell** | FR-050 |

## Tasks

Tasks run in order. `[P]` marks a task that shares no file with another `[P]` task: T3, T4 and T5 can run in parallel after T2. Each task starts with a failing test, and its commit ticks its box here.

- [x] **T0** `build:` pull request: `Microsoft.AspNetCore.Mvc.Testing` (Flagged concern 1)
  - Covers: NFR-007.
  - Files: `src/Directory.Packages.props`.
  - Branch `feature/22-mvc-testing-package` from `develop`. Commit `build(deps): add Microsoft.AspNetCore.Mvc.Testing for Api integration tests`, footer `Refs #22`. PR title `#22 build: add Microsoft.AspNetCore.Mvc.Testing`; it needs `sdlc:track-s` to pass the gate. Push and create the PR in separate commands.
  - Check: restore reports no NU1903 and no NU1109 (transitive pinning is on) for the new transitive packages. List each one's licence. After it merges, merge `develop` into this branch.
  - Proof: `dotnet build Harmony.slnx` reports 0 errors and no more than 585 code warnings; the licences are MIT.
- [x] **T1** Project skeleton, opt-in and tag guard
  - Covers: FR-002, FR-004, FR-005, NFR-006, NFR-009.
  - Files: the csproj, `Harmony.slnx`, `Conventions/RequirementTagGuardTests.cs`, `AGENTS.md` (Commands).
  - Trial (concern 3a): a throwaway `new WebApplicationFactory<Program>()` compiles. Expected: yes, through the generated public `Program`.
  - Proof:
    - `RequirementTagGuardTests.TestAssembly_EveryTestMethod_HasRequirementTag` passes under the Api test command.
    - The test command's summary lists no Api test project.
    - A filter that selects nothing exits non-zero (FR-004).
    - A Cobertura file appears under `TestResults/Harmony.Api.IntegrationTests/`.
- [x] **T2** Test host on PostgreSQL
  - Covers: FR-001, FR-003, NFR-002, NFR-004.
  - Files: `AssemblyInfo.cs`, `Harness/ApiTestHost.cs`, `Harness/HarmonyApiFactory.cs`, `Harness/ApiTestHostTests.cs`, `Conventions/ImageTagTests.cs`.
  - Proof:
    - `ApiTestHostTests.HealthReady_HostStarted_Returns200`
    - `ApiTestHostTests.Database_HostStarted_AppliedMigrationsEqualPostgreSqlMigrations`
    - `ApiTestHostTests.Roles_HostStarted_SeededRolesExist` (Administrator, Basic)
    - `ApiTestHostTests.Database_HostStarted_UsesNpgsqlAndRunSecret`. This is the early check that `UseSetting` reaches code that runs before `Build()`.
    - `ApiTestHostTests.DockerGuard_EndpointUnreachable_ThrowsMessageNamingDocker`, which uses the container start path pointed at `tcp://127.0.0.1:1`.
    - `ApiTestHostTests.Container_NameDoesNotStartWithHarmony`
    - `ImageTagTests.PostgreSqlImage_MatchesDockerComposePostgres`
- [x] **T3** [P] Stand-ins, configuration and files
  - Covers: FR-006, NFR-003.
  - Files: `Harness/HarmonyApiFactory.cs` (`ConfigureTestServices`), `Harness/RecordingNotificationsPublisher.cs`, `Harness/StandInTests.cs`.
  - Proof:
    - `StandInTests.Services_TestHost_PublisherAndSourceControlClientAreStandIns`: no `RabbitMQNotificationPublisher` descriptor remains.
    - `StandInTests.Search_TestHost_DatabaseSearchAndNoAlgoliaClient`
    - `StandInTests.Configuration_TestHost_ElasticAndOtlpEmpty`
    - `StandInTests.TestEndpoint_AnonymousPost_RecordsEmailNotification`
    - `StandInTests.UploadCardFile_OwnerValidFile_WritesUnderRunFolderOnly`. It checks for the file under the temp `Files/` folder and compares a listing of `src/Services/Harmony.Api/Files` before and after.
- [x] **T4** [P] Users and tokens
  - Covers: FR-007.
  - Files: `Harness/TestUsers.cs`, `Harness/TestTokens.cs`, `Harness/TestTokenTests.cs`.
  - Proof:
    - `TestTokenTests.UserBoards_HarnessToken_IsNot401`
    - `TestTokenTests.UserBoards_TokenSignedWithOtherKey_IsNot2xx`. Today it gets 500, which R-11 owns.
- [x] **T5** [P] Matrix file parser and verdicts
  - Covers: FR-012, FR-040, FR-041, FR-042 (parsing), FR-015 and FR-034 (verdict).
  - Files: `Authorization/MatrixFile.cs`, `Authorization/RowVerdict.cs`, `Authorization/MatrixFileTests.cs`, `Authorization/RowVerdictTests.cs`.
  - Proof:
    - `MatrixFileTests.Parse_LineMovedToEnd_FailsOnSortOrder`, `Parse_DuplicateLine_Fails`, `Parse_UnknownScope_FailsNamingLine`, `Parse_GapMarkNamesWrongItem_Fails` and `Parse_NonMemberMarkOnSelfScope_Fails`
    - `RowVerdictTests.KnownGap_Refused_FailsSayingRemoveMark`
    - `RowVerdictTests.KnownGap_LetIn_Passes`
    - `RowVerdictTests.NotReached_IsInconclusive`
    - `RowVerdictTests.NonMember_400_IsInconclusiveWithBody`, and the same for 401, 405 and 415
- [x] **T6** Discovery, consistency and the request recorder
  - Covers: FR-010, FR-011, FR-012 (live), FR-013, FR-022, FR-042 (check).
  - Files: `Authorization/EndpointDiscovery.cs`, `Authorization/MatrixCheck.cs`, `Harness/RequestRecorder.cs`, `Harness/HarmonyApiFactory.cs` (adds the startup filter), `Authorization/MatrixCheckTests.cs`.
  - Early trial (riskiest piece of plumbing): send one request that ends in an unhandled exception and one that falls through to `index.html`, and check what the recorder sees. If `IExceptionHandlerFeature.Endpoint` is missing after re-execution, switch to the `Microsoft.AspNetCore.Routing.EndpointMatched` `DiagnosticListener` event and record the deviation.
  - Proof:
    - `MatrixCheckTests.Discovery_TestHost_FindsOneEndpointPerControllerAction`. It compares with a reflection count over `Harmony.Api`; 106 on 2026-11-26.
    - `MatrixCheckTests.Check_EndpointWithoutLine_FailsNamingItAndAskingForScope`
    - `MatrixCheckTests.Check_LineWithoutEndpoint_FailsNamingLine`
    - `MatrixCheckTests.Check_UnexpectedOtherEndpoint_FailsNamingIt` (a `MapGet`-style endpoint)
    - `MatrixCheckTests.Check_UnknownGrpcService_Fails`
    - `MatrixCheckTests.Check_AllowAnonymousOnNonPublicScope_FailsNamingIt`
    - `MatrixCheckTests.Check_BaselineDiffers_FailsGivingBothNumbers`
    - `MatrixCheckTests.TestHostAndMatrixFile_AreConsistent` (live)
  - Other endpoints that are allowed: health ×2, `/Error`, the fallback, and gRPC endpoints whose `GrpcMethodMetadata.ServiceType` is one of the six services, plus gRPC's unimplemented-method catch-alls. The trial confirms the exact shape, and any difference is logged.
- [x] **T7** Anonymous rule and the full matrix file (US1, shippable on its own)
  - Covers: FR-014, FR-015, FR-020, FR-021, FR-040, FR-041 (R-07).
  - Files: `Authorization/RequestBuilder.cs` (anonymous part), `Authorization/RowRunner.cs` (anonymous part), `Authorization/MatrixRows.cs`, `Authorization/AnonymousMatrixTests.cs`, `Authorization/RowRunnerTests.cs`, `Authorization/authorization-matrix.md` (all 106 lines with scopes and anonymous marks; non-member marks still `refused`, set in T9; baseline anonymous = 94).
  - The FR-011 message supplies the lines. Scopes come from the spec's *Expected sizes* and the inventory.
  - Proof:
    - `AnonymousMatrixTests.AnonymousRequest_ProtectedEndpoint_Returns401` (6 rows today: the `[Authorize]` endpoints)
    - `AnonymousMatrixTests.AnonymousRequest_PublicEndpoint_IsNot401` (6)
    - `AnonymousMatrixTests.AnonymousRequest_KnownGap_IsLetIn_KnownIssue` (94, tagged FR-020 and FR-040)
    - `RowRunnerTests.Request_RouteValueBreaksGuidConstraint_IsInconclusive` (FR-015)
- [x] **T8** Fixtures and the non-member request builder
  - Covers: FR-031, FR-032.
  - Files: `Harness/Fixtures.cs`, `Authorization/RequestBuilder.cs` (non-member part), `Authorization/RequestBuilderTests.cs`, `Authorization/FixtureIsolationTests.cs`.
  - Proof:
    - `RequestBuilderTests.UpdateCardTitle_Fixture_RouteIdAndBodyCardIdAreCardAndBoardIdIsBoard`
    - `RequestBuilderTests.BodyPropertyWithoutKind_FailsNamingIt` (a synthetic `FooId` type)
    - `RequestBuilderTests.NestedAndPluralIds_GetFixtureIds` (`IssueType.Id`, `ListIds`)
    - `RequestBuilderTests.StringWorkspaceId_GetsIdText`
    - `FixtureIsolationTests.DeleteChecklist_NonMemberAndOwner_EachDeleteOnlyTheirOwnFixture`
- [x] **T9** Non-member rule (US2)
  - Covers: FR-030, FR-033, FR-034, FR-040, FR-041 (R-08).
  - Files: `Authorization/RowRunner.cs` (owner request), `Authorization/MatrixRows.cs`, `Authorization/NonMemberMatrixTests.cs`, `Authorization/authorization-matrix.md` (bindings, non-member marks, baseline).
  - Dry run first, not committed: classify all 80 resource endpoints and print a table of outcome, status and body. Then fix every inconclusive row with a binding or a fixed body. Only rows that are let in get `gap R-08`.
  - Proof:
    - `NonMemberMatrixTests.NonMemberRequest_ResourceEndpoint_Returns403Or404` (rows marked `refused`, or the placeholder row)
    - `NonMemberMatrixTests.NonMemberRequest_KnownGap_IsLetIn_KnownIssue` (expected 80, tagged FR-030 and FR-040). Both scenarios for FR-030 (`POST .../load`, `POST .../members` with the caller's own ID) are among its rows.
- [x] **T10** Documentation
  - Covers: NFR-010; Flagged concerns 4 and 5.
  - Files: `AGENTS.md`, `docs/architecture/overview.md`, `docs/roadmap.md`, and `spec.md` (*Expected sizes*, only if a number differs).
  - R-04's status stays `in progress` until #23 merges.
  - Proof: review against NFR-010; the guard still passes.
- [ ] **T11** CI step draft
  - Covers: FR-050, NFR-001, NFR-005.
  - Files: draft only, `<scratchpad>/ci-api-integration-step.yml`; chsakell applies it.
  - In the `integration` job, after `Integration tests`:
    - a step `Api integration tests` with `if: ${{ !cancelled() }}` that runs `dotnet test --project tests/Harmony.Api.IntegrationTests -p:HarmonyIntegrationTests=true --report-xunit-trx --report-xunit-markdown` (no `--no-build`);
    - `Publish test summary` reads `*.md` from both `TestResults/` folders;
    - `Upload test results` adds `TestResults/Harmony.Api.IntegrationTests/` to the path of `integration-test-results`; it is still `always()` and kept 14 days.
  - Proof: three green runs in a row on #23. Each Api step takes 5 minutes or less, and each job 15 minutes or less.
- [x] **T12** Verification (`/sdlc-verify`)
  - Covers: FR-001 to FR-005, FR-011, FR-013, FR-015, FR-022, FR-034, FR-041, NFR-002, NFR-008, NFR-009.
  - Checks proven during verification, not committed:
    - `docker ps -a` is clean within 60 s after a run.
    - With Docker stopped, the test command exits 0 and the Api command fails with `Docker`.
    - An untagged method fails the guard.
    - A controller action without a line fails, naming it.
    - A `MapGet` fails.
    - `[AllowAnonymous]` on `GET api/identity/user` fails.
    - A deliberately broken route value is inconclusive.
    - A wrong-type body gives 400, inconclusive.
    - `[Authorize]` on a known-gap endpoint fails with "remove the mark".
    - The Aspire containers are unchanged after a run.
    - The AppHost reaches `https://localhost:7108/healthz/ready` 200.
    - The R-03 integration command still passes.
    - A Cobertura file is written.

### Coverage

| Requirement | Task(s) | Test(s) or check |
|---|---|---|
| FR-001 | T2, T12 | `ApiTestHostTests.HealthReady_*`, `Database_*_AppliedMigrations*`, `Roles_*`; `docker ps -a` check |
| FR-002 | T1, T12 | Test command summary lists no Api project; build output includes it |
| FR-003 | T2, T12 | `ApiTestHostTests.DockerGuard_EndpointUnreachable_*`; Docker-stopped check |
| FR-004 | T1 | Empty filter exits non-zero |
| FR-005 | T1, T12 | `RequirementTagGuardTests.*`; untagged-method check |
| FR-006 | T3 | `StandInTests.*` |
| FR-007 | T4 | `TestTokenTests.*` |
| FR-010 | T6 | `MatrixCheckTests.Discovery_TestHost_FindsOneEndpointPerControllerAction` |
| FR-011 | T6, T12 | `MatrixCheckTests.Check_EndpointWithoutLine_*`; local check |
| FR-012 | T5, T6 | `MatrixFileTests.Parse_*`; `MatrixCheckTests.Check_LineWithoutEndpoint_*` |
| FR-013 | T6, T12 | `MatrixCheckTests.Check_UnexpectedOtherEndpoint_*`, `Check_UnknownGrpcService_*`; `MapGet` check |
| FR-014 | T7 | Row display names in TRX (`Anonymous DELETE api/identity/role/{id}`) |
| FR-015 | T5, T7, T12 | `RowVerdictTests.NotReached_*`; `RowRunnerTests.Request_RouteValueBreaksGuidConstraint_*` |
| FR-020 | T7 | `AnonymousMatrixTests.AnonymousRequest_ProtectedEndpoint_Returns401`, `..._KnownGap_IsLetIn_KnownIssue` |
| FR-021 | T7 | `AnonymousMatrixTests.AnonymousRequest_PublicEndpoint_IsNot401` |
| FR-022 | T6, T12 | `MatrixCheckTests.Check_AllowAnonymousOnNonPublicScope_*`; local check |
| FR-030 | T9 | `NonMemberMatrixTests.*` |
| FR-031 | T8 | `RequestBuilderTests.*` |
| FR-032 | T8 | `FixtureIsolationTests.*` |
| FR-033 | T9 | Owner request in every `NonMemberMatrixTests` row |
| FR-034 | T5, T12 | `RowVerdictTests.NonMember_400_*` (and 401, 405, 415); wrong-type body check |
| FR-040 | T5, T7, T9 | `*_KnownIssue` theories; `MatrixFileTests.Parse_GapMarkNamesWrongItem_Fails` |
| FR-041 | T5, T12 | `RowVerdictTests.KnownGap_Refused_FailsSayingRemoveMark`; local `[Authorize]` check |
| FR-042 | T5, T6 | `MatrixCheckTests.Check_BaselineDiffers_*`, `TestHostAndMatrixFile_AreConsistent` |
| FR-050 | T11 | `Integration tests` check on #23; summary and artifact |
| NFR-001 | T11 | Step ≤ 5 min, job ≤ 15 min |
| NFR-002 | T2, T12 | `ApiTestHostTests.Container_NameDoesNotStartWithHarmony`; Aspire check |
| NFR-003 | T2, T3 | `ApiTestHostTests.Database_*_UsesNpgsqlAndRunSecret`; `StandInTests.*` |
| NFR-004 | T2 | `ImageTagTests.PostgreSqlImage_MatchesDockerComposePostgres` |
| NFR-005 | T11 | Three green runs in a row; no automatic rerun |
| NFR-006 | T1-T10 | Build, warnings as errors, format of changed files |
| NFR-007 | T0 | Licence metadata |
| NFR-008 | T12 | No `src/` diff (or the logged fallback); AppHost, test and R-03 commands pass |
| NFR-009 | T1, T12 | Cobertura file under `TestResults/Harmony.Api.IntegrationTests/` |
| NFR-010 | T1, T10 | AGENTS.md, overview and roadmap reviewed |

## Risks and what could break

| Risk | Likelihood / impact | Mitigation |
|---|---|---|
| **Riskiest: the non-member owner requests come back inconclusive** (400 from `[ApiController]` validation or MediatR validators, 404 from the harness's own IDs) on many of the 80 endpoints. | High / scope and time | The T9 dry run classifies all 80 before any mark is written, and each one is fixed with a binding or a fixed body. If more than 10 cannot be made conclusive, it goes back to the product owner, because the spec forbids marking them. |
| `UseSetting` values are not visible to the `builder.Configuration` reads before `Build()`; the host would then use SQL Server or the committed secret. | Medium / blocks everything | Caught in T2 by `Database_HostStarted_UsesNpgsqlAndRunSecret` and the FR-007 token tests. Fallback: process environment variables set by the fixture before the host starts, logged as a deviation. |
| The exception handler re-executes `/Error` outside Development and hides the original endpoint. | Medium / FR-015 false results | The recorder reads `IExceptionHandlerFeature.Endpoint`. The T6 trial checks it, and the `DiagnosticListener` event is the fallback. |
| Changing the process's current directory could affect other code in the test process. | Low | One host per process. The results directory is absolute and `RepositoryRoot` uses `AppContext.BaseDirectory`. The directory is restored in `DisposeAsync`, and the NFR-009 check confirms coverage still lands in the right folder. |
| Anonymous rows change shared state, for example role or permission endpoints with `{}`. | Low | No row depends on roles: tokens carry the role claim, and no endpoint uses a permission policy. Non-member rows use their own fixtures. Rows use random IDs. |
| The linked R-03 files are renamed or moved. | Low / build | The Api project fails to compile at once; the csproj comment names the link. |
| CI time with about 270 requests and 160 fixtures | Low / NFR-001 | One host and one container; fixtures are a few inserts each. Measured on the first CI run. |
| Neighbouring flows | None at run time | No `src/` change. The test command is unchanged. The R-03 project is only read, through linked files. |

## Alternatives considered

| Option | Why not chosen |
|---|---|
| JSON, CSV or a C# list as the matrix file | Harder to read and review, and none of them renders as the published answer. YAML would need a new package. |
| Endpoints from reflection over the controllers | The spec requires the host's routing (FR-010). Reflection is used only as the cross-check count. |
| Only the `DiagnosticListener` `EndpointMatched` event | It depends on an internal event name. It is kept as the fallback. |
| A `src/` change so the files folder comes from configuration | Breaks NFR-008, and changing the current directory achieves the same. |
| Tokens from `POST api/identity/token` | Ties every row to a public endpoint and to passwords. FR-007 says the harness issues tokens. |
| Fixtures through the API | Forbidden by the spec (Definitions, *Fixture*). |
| Moving `DockerGuard` and the others to `tests/Shared/` | They depend on Testcontainers, and the traceability ADR allows only xUnit-only code there. |
| `[Theory(SkipTestWithoutData = true)]` for a theory with no rows | It reports a skipped test, which goes against "never skipped". The placeholder row is used instead. |
| Aspire.Hosting.Testing, SQL Server, or an in-memory EF provider | Ruled out by NFR-002 and by the spec's database decision. |

## Migration and rollback

There is no EF Core migration and no production code or data change.

- **Rollout:**
  1. The T0 `build:` PR merges.
  2. #23 merges once chsakell has applied the T11 step and `Integration tests` has passed three times in a row.
- **Rollback:** revert #23 and remove the CI step. Revert the T0 PR only if nothing else uses Mvc.Testing by then.

## Proof (definition of done)

- **Build:** `dotnet build Harmony.slnx` reports `0 Error(s)` and no more than 585 code warnings.
- **Test:** `dotnet test --solution Harmony.slnx` passes, and the summary lists no Api test project.
- **Api tests:** `dotnet test --project tests/Harmony.Api.IntegrationTests -p:HarmonyIntegrationTests=true` passes with Docker running. A per-rule run, for example `... --filter-trait "Requirement=FR-030"`, selects every non-member row.
- **R-03 tests:** `dotnet test --project tests/Harmony.Infrastructure.IntegrationTests -p:HarmonyIntegrationTests=true` still passes.
- **Format:** `dotnet format Harmony.slnx --verify-no-changes --include <changed files>` exits 0.
- **Feature checks:**
  - the matrix file has 106 lines;
  - the anonymous baseline is 94;
  - the non-member baseline is the recorded number (≤ 80);
  - every row has its own result.
- **CI:** `Integration tests` passes three times in a row on #23.

## Found while planning (for the product owner, not in scope)

Read from the code on 2026-11-26. None of these is pinned.

- **Route IDs that are not used:**
  - 22 actions ignore their route `{id}` and take the ID from the body, for example most of `api/Cards/{id:guid}/*`, `api/CheckListItems/*` and `api/Workspace/{id:guid}/status`.
  - `PUT api/Board/{id:guid}/positions` binds an unused query `boardId` instead of the route.

  This is wider than the `SprintsController` evidence on R-08.
- **`UserController`'s four `[AllowAnonymous]` attributes** have no effect today, because nothing requires authentication.
- **`profile-picture/{userId}` and `UserNotifications/{userId}`** ignore the route user ID and act on the caller.
- **`CreateBoardCommand.WorkspaceId`** is a `string`; it is a `Guid` everywhere else.
- **`POST api/Comments/{commentId:guid}`** deletes a comment.

## Deviations log

| Date | Task | What changed and why |
|---|---|---|
| 2026-11-26 | T0 | PR #24 (draft, needs `sdlc:track-s`). The closure has a third package, `Microsoft.Extensions.DependencyModel` 10.0.15 (MIT), as well as `Microsoft.AspNetCore.TestHost`. No NU1903 or NU1109, and no known vulnerabilities. |
| 2026-11-26 | T0 | Instead of waiting for #24 to merge, `feature/22-mvc-testing-package` was merged into this branch locally. The content is identical, so merging `develop` after #24 merges is clean. |
| 2026-11-26 | T1 | Concern 3(a) trial confirmed: `new WebApplicationFactory<Program>()` compiles through the generated public `Program`. No `src/` change. The linked R-03 files sit under `Harness/Linked/` in the project view and keep their `Harmony.Infrastructure.IntegrationTests.Harness` namespace. |
| 2026-11-26 | T2 | `HarmonyApiFactory` also calls `UseContentRoot(<repo>/src/Services/Harmony.Api)`. WebApplicationFactory looks for `MvcTestingAppManifest.json` in the *current* directory, which the harness moves to the temp folder (concern 3(b)), so without this it guessed a content root that does not exist. This is a test-side change only; NFR-003 is unchanged, because the content root is the one a deployed Api uses. `UseSetting` reaches the reads before `Build()`, as planned, so no environment-variable fallback was needed. |
| 2026-11-26 | T4 | Done before T3, because T3's upload test needs an owner token. `TestUser` also carries the user's roles, so `TestTokens` can add the role claims. `CreateClient(token?)` sets the bearer header. |
| 2026-11-26 | T3 | `Harness/Fixtures.cs` (planned for T8) was brought forward: the upload test needs a real card, because the handler returns "Card not found" before it writes the file. T8 keeps `FixtureIsolationTests` and the request builder. The factory also exposes `ServiceDescriptors` (the collection after the overrides) and the `SourceControlClient` substitute, for the stand-in checks. The fixture board is `Scrum`, so the sprint and backlog endpoints apply. |
| 2026-11-26 | T5 | Extra tests beyond the plan's list: a valid file parses (FR-042), `body=` keeps JSON that has `;` in it, malformed lines (cell count, mark, kind, `name=kind`, `body=` not last, method case), wrong header, missing baseline, and verdicts for FR-020, FR-021, FR-030 and FR-033 (owner refused means inconclusive). The parser collects every problem and throws one `MatrixFileException` that lists them. `RowVerdict.Judge` takes the owner response as an optional argument and requires it for the non-member rule. |
| 2026-11-26 | T6 | Trial results. (1) After an unhandled exception, `IExceptionHandlerFeature.Endpoint` holds the original action, so no `DiagnosticListener` fallback was needed. (2) `MapFallbackToFile` clears the endpoint before it serves the file, so the recorder sees *no* endpoint for a request that falls through. That is still "not reached", so the row is inconclusive (FR-015); the message says "no endpoint" instead of "fallback". (3) The other endpoints are exactly the planned set. gRPC unimplemented catch-alls have only route metadata, so they are recognised by route (`{unimplementedService}/...` and `<one of the six>/{unimplementedMethod:grpcunimplemented}`). The fallback is recognised by route `{*path:nonfile}` and its `Fallback` display name, because `FallbackMetadata` is internal. 106 actions and 106 API descriptions. |
| 2026-11-26 | T6 | The matrix file (planned for T7) was created here, because `TestHostAndMatrixFile_AreConsistent` needs it. It has all 106 lines with scopes from the spec's lists: public 6, self 10, administration 8, user 2 and resource 80. Anonymous marks: `refused` for the six `[Authorize]` endpoints, `gap R-07` for 94. Non-member marks are `refused` with baseline 0 until T9. T7 proves the anonymous marks by running them. Extra tests: `Check_AllowedOtherEndpoints_Pass` and `Discover_ActionWithoutOneHttpMethod_Throws`. |
| 2026-11-26 | T7 | The 106 anonymous rows ran as recorded in T6: 6 refused (401), 6 public (not 401), 94 known gaps let in (200, 400 or 500). No row was inconclusive, so no mark changed. Extra tests: `RowRunnerTests.Request_WellFormed_ReachesEndpoint`, `AnonymousRequest_FillsRouteIdsWithNewGuidsAndSendsEmptyBody`, and `MatrixRowsTests` (rows cover every line, names are unique, a filter with no line yields one placeholder). xUnit appends the arguments to the display name (`Anonymous DELETE api/identity/role/{id}(method: ..., route: ...)`); the name still contains the rule, method and route (FR-014). The host caches its `Discovery` through `MatrixRows.Discovery(host)`. Route values: a value is an ID when it has a `:guid` constraint, a `Guid` type or a name ending in `Id`. |
| 2026-11-26 | T8 | `RowRunner.RunNonMemberAsync` (the owner request, planned for T9) was written here, because `FixtureIsolationTests` needs both requests. The isolation test allows for R-08: the non-member's fixture loses its checklist only when the request is let in. The matrix line `DELETE api/CheckLists/{id:guid}` got `id=checklist` now, because the isolation test reads it; T9 adds the rest of the bindings. Extra tests: `QueryIdAndUserId_GetFixtureAndCallerIds`, `FixedBody_KeepsValuesAndReplacesIds` and `RouteIdWithoutKind_FailsNamingIt`. A binding matches the full body path first, then the property's own name. Dictionaries get `{}`, `byte[]` gets `null`, and nullable values that are not IDs get `null`. |
| 2026-11-26 | T9 | Dry run, in four passes. **(1)** 66 lines needed a route `{id}` binding. Each got its controller's kind: Board → board; BoardLists and Lists → list; Cards and Cards/backlog → card; CheckListItems → checklistitem; CheckLists → checklist; Labels → label; Links → link; Sprints → sprint; Workspace → workspace. The plan's other overrides were applied too. **(2)** `Id=workspace` on `PUT api/Workspace/{id:guid}/status` and `/rename` was dropped. Binding names match without regard to case, so the route `id=workspace` already covers the body `Id`, and the parser rejects the pair as a duplicate. **(3)** Two harness fixes, each test-first: `bool` body properties got `0` instead of `false` (5 rows were inconclusive with 400), and a required non-ID query parameter (`term` on both `members/search`) was left out (2 rows). A required parameter now gets a placeholder of its type: `ApiParameterDescription.IsRequired`, or `ModelMetadata.IsRequired` for reference types. This applies only to the non-member request. **(4)** Result: all 80 are let in and none is inconclusive, so the non-member baseline is 80, as the spec expected. Non-member statuses: 73 × 200 and 7 × 500. Owner statuses: 71 × 200 and 9 × 500. The 7 rows with 500 for both callers count as let in under the spec. The two FR-030 scenarios behave as the spec says: `POST .../load` answers 200 with `Succeeded=false`, and `POST .../members` adds the non-member. *Expected sizes* in `spec.md` is unchanged. |
| 2026-11-26 | T10 | `spec.md` is unchanged, because all expected sizes held (106, 94, 80). The overview's CI/CD line describes the `Integration tests` job with the Api step, which is true once chsakell applies the T11 draft (rollout step 2, before #23 merges). R-04's row keeps `in progress`, and its section notes how it was delivered. The roadmap notes cover R-07 (94, the token endpoints, the admin rule), R-08 (80, 403 or 404) and R-24 (concern 4). |
| 2026-11-26 | T11 | Draft written (`<scratchpad>/ci-api-integration-step.yml`). Its command was run locally: 271 tests passed in 21 s, writing TRX, Markdown and Cobertura under `TestResults/Harmony.Api.IntegrationTests/`. The box stays unticked until chsakell applies the step and `Integration tests` passes three runs in a row on #23. |
| 2026-11-27 | T11 | chsakell applied the draft to `ci.yml` unchanged. It is committed with T12. |
| 2026-11-27 | T12 | Local checks, all run on 2026-11-27 after `develop` (with #24) was merged in. **(1)** Clean build: 0 errors; 585 code warnings (baseline 585), 90 NuGet, none from the Api test project. **(2)** Test command: 61 passed, exit 0. **(3)** Api tests: 271 passed, 0 skipped, 21 s, Cobertura written. **(4)** R-03 integration tests: 100 passed. **(5)** 15 s after a run, `docker ps -a` showed no container from it, Ryuk included. **(6)** With Docker Desktop stopped, the test command exited 0 and ran only the unit-test projects, and the Api command exited 2 naming Docker. 209 tests that need the host failed and none was skipped. **FR-003 reading (accepted by chsakell):** 62 pure tests still passed (parser, verdicts, synthetic checks, guard, image tag, the DockerGuard test and the empty placeholder row). The requirement holds, because every test that needs Docker fails and none is skipped; the scenario's "no test is reported as passed" is read as "no test that needs Docker". **(7)** Temporary local breaks, then reverted, each failing the expected test with a message naming its subject: an untagged method (guard), a new controller action without a line (FR-011, plus the 106 count), a `MapGet` (FR-013), `[AllowAnonymous]` on `GET api/identity/user` (FR-022), `[Authorize]` on `DELETE api/identity/role/{id}` ("remove the known-gap mark", FR-041), and `"Visibility":"wrong"` in the `POST api/Board` body (400, inconclusive with the body, FR-034). The broken route value is covered by `RowRunnerTests.Request_RouteValueBreaksGuidConstraint_IsInconclusive`. **(8)** The Aspire containers and `harmony-*` volumes were unchanged by the test runs. Afterwards, the AppHost logged `Distributed application started` and the gateway's `/healthz/ready` returned 200. **(9)** An empty filter exits 8 (FR-004). The format check passed on all 30 changed C# and project files. There is no `src/` change. |
