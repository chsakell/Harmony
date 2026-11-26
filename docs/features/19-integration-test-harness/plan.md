---
id: 19
title: Integration test harness
artifact: plan
status: approved            # draft | approved | superseded
owner: chsakell
approved-by: chsakell, 2026-11-26
created: 2026-11-26
spec: ./spec.md
---

# Plan: Integration test harness

## Approach

- **New project.** Add one test project, `tests/Harmony.Infrastructure.IntegrationTests`, following the layout of `Harmony.Infrastructure.UnitTests`:
  - xUnit v3 on MTP, Shouldly, NSubstitute and coverlet.MTP;
  - `OutputType Exe`;
  - inherits `tests/Directory.Build.props`, so it gets warnings as errors, the coverage arguments and the shared `RequirementTagGuard`.
- **Database servers.** An xUnit v3 assembly fixture owns at most one Testcontainers instance per engine. Each instance starts lazily, the first time a test asks for it.
  - Relational tests are theories over `RelationalProvider { SqlServer, PostgreSql }`.
  - Each relational test class gets its own fresh, migrated database per provider through a class fixture.
  - MongoDB tests share one instance and run in a single non-parallel collection.
- **Code under test.** The real repositories, `BoardService` and `CardService` are built over a real `HarmonyContext`. Their non-database dependencies get stand-ins:
  - a pass-through `ICacheService` and the real `CardSummaryService` over it;
  - an NSubstitute `IMediator`;
  - the real AutoMapper profiles;
  - in-memory `IConfiguration` and `IOptions<AppEndpointConfiguration>`.
- **Opt-in.** Chosen by a trial on SDK 10.0.404 (see Alternatives).
  - The project sets `IsTestingPlatformApplication` to `false` unless `HarmonyIntegrationTests=true` is set.
  - `dotnet test --solution Harmony.slnx` therefore builds the project but never runs it.
  - The integration test command is `dotnet test --project tests/Harmony.Infrastructure.IntegrationTests -p:HarmonyIntegrationTests=true`.
- **ADRs followed:**
  - testing strategy: rules 1-7, and rule 8 (packages in a separate `build:` PR);
  - traceability tags;
  - SQL Server and PostgreSQL: both providers, a migration for every schema change, the legacy timestamp switch.
- **Production code:** no change.

### Decisions on the spec's open points

| Spec item | Decision |
|---|---|
| Opt-in mechanism (Design, Open question 1) | A conditional `IsTestingPlatformApplication` property; the command is above. Trial results (scratch projects, SDK 10.0.404): `dotnet test --solution` ran only the unit project, exit 0. `--project ... -p:HarmonyIntegrationTests=true` ran the integration project, exit 0. A filter that selected nothing exited with 8. `--project` without the property failed with "No test projects were found", exit 1. Setting `IsTestProject=false` alone does **not** work: MTP `dotnet test` still ran the project. |
| Flagged concern 2 (provider setup) | Copy the settings into `Harness/ProviderSetup.cs`. A parity test (NFR-005) reads each host's `Extensions/ServiceCollectionExtensions.cs` and fails if they drift. The hosts' `AddDatabase` methods are `internal`, and referencing three web hosts would pull their `appsettings.json` and Hangfire, RabbitMQ and gRPC setup into the test output. Extracting a shared helper is left for a later item. |
| Flagged concern 4 (floating SQL Server tag) | Keep parity with the deployment files. `Harness/ImageTags.cs` holds the three tags, and a test compares them with the compose files (NFR-006). |
| Flagged concern 5(d) (pending model changes) | None today. On 2026-11-26, `dotnet ef migrations has-pending-model-changes` reported "No changes" for all 3 contexts × 2 providers, run against the Api, Automations and Notifications startup projects. FR-011 is therefore an ordinary passing test, with no `_KnownIssue`. The snapshots were made with EF Core 8.0.4. EF Core 9 and later throw from `Migrate()` when there is a pending change, so FR-011 also guards host startup. |
| Flagged concern 6 (Arm64) | Accepted. No exclusion. |
| Flagged concern 7 (SQL Server image licence) | Waived by chsakell, 2026-11-26: Developer edition, test-time only, EULA accepted by Testcontainers (`ACCEPT_EULA=Y`), the same image as the deployment files. When `plan.md` is committed, the waiver is also written into `spec.md`, concern 7. |
| Open question 7 (seeders) | Not covered; they belong to R-04. |

## Files that change

| File | Change | Why |
|---|---|---|
| `src/Directory.Packages.props` | modify, **separate `build:` PR** (T0) | Adds `Testcontainers.MsSql`, `Testcontainers.PostgreSql` and `Testcontainers.MongoDb` 4.17.0 (MIT). They bring `Testcontainers` 4.17.0 and its MIT dependencies Docker.DotNet.Enhanced, SSH.NET and SharpZipLib. |
| `Harmony.slnx` | modify | Adds the project to the `/tests/` folder, so the solution build compiles it (FR-002). |
| `tests/Harmony.Infrastructure.IntegrationTests/Harmony.Infrastructure.IntegrationTests.csproj` | new | Packages: xunit.v3, coverlet.MTP, NSubstitute, Shouldly and the three Testcontainers modules. References `Harmony.Infrastructure` and both migrations projects, each with `GlobalPropertiesToRemove="HarmonyIntegrationTests"` so the `src/` projects are not rebuilt under a second set of global properties. Holds the conditional `IsTestingPlatformApplication`, with a comment explaining it. |
| `.../AssemblyInfo.cs` | new | `[assembly: AssemblyFixture(typeof(DatabaseServers))]` |
| `.../Harness/DatabaseServers.cs` | new | Assembly fixture. Exposes `GetSqlServerAsync`, `GetPostgreSqlAsync` and `GetMongoDbAsync`; each is a `Lazy<Task<...>>` that starts the container on first use, so a MongoDB-only run starts no SQL container. `DisposeAsync` disposes only the containers that were started. Builder settings: random host ports, no names, no volumes. Every Build or Start failure goes through `DockerGuard`. |
| `.../Harness/DockerGuard.cs` | new | Turns Testcontainers' `DockerUnavailableException` (and any failure to start or reach Docker) into an `InvalidOperationException` whose message starts with "Docker with Linux containers is required to run the integration tests". |
| `.../Harness/ImageTags.cs` | new | The constants `mcr.microsoft.com/mssql/server:2022-latest`, `postgres:17-alpine` and `mongo:8.3`, defined in one place. |
| `.../Harness/RelationalProvider.cs` | new | `enum RelationalProvider { SqlServer, PostgreSql }` |
| `.../Harness/BothProvidersAttribute.cs` | new | An xUnit v3 `DataAttribute` that yields `SqlServer` and `PostgreSql`, so the theory display name reads `...(provider: SqlServer)` (FR-005). |
| `.../Harness/ProviderSetup.cs` | new | `DbContextOptions<TContext> For<TContext>(RelationalProvider, string connectionString)`: `UseSqlServer` or `UseNpgsql` with `MigrationsAssembly("Harmony.Persistence.Migrations.SqlServer"/".PostgreSql")`. Sets `Npgsql.EnableLegacyTimestampBehavior` once, before any Npgsql use. Exposes the assembly names and the switch name for the parity test. |
| `.../Harness/TestDatabase.cs` | new | Creates a uniquely named empty database on a server (`h_<8 hex>`, lower case) with `IRelationalDatabaseCreator.CreateAsync`. Has `Migrate()` and `CreateContext<TContext>()`, which returns a new context per call. |
| `.../Harness/RelationalDatabaseFixture.cs` | new | Class fixture; takes `DatabaseServers`. `Task<TestDatabase> GetAsync(RelationalProvider)` lazily creates and migrates one fresh `HarmonyContext` database per provider for the class (FR-006). |
| `.../Harness/TestData.cs` | new | Seed builders that write through their own context: user (`HarmonyUser`), workspace (unique name, status), membership, board (unique key, visibility), list, issue type, sprint, card (status, position, list, parent, sprint, issue type), label, member, checklist with items, comment, link, attachment. Every builder sets all required columns. |
| `.../Harness/ServiceFactory.cs` | new | Builds `CardRepository`, `BoardService` and `CardService` over a new context with the stand-ins listed in Approach. AutoMapper uses `AddMaps` over the Application and Infrastructure assemblies, as production does. |
| `.../Harness/PassThroughCacheService.cs` | new | An `ICacheService` that never caches: `GetOrCreate` calls the retriever, reads return empty, writes return success. Every read therefore reaches the database. |
| `.../Harness/MongoDbFixture.cs`, `.../Harness/MongoDbCollection.cs` | new | A `MongoClient` on the shared instance. `MongoDbConfiguration.Configure()` is called once per process, because it throws when called twice. The collection definition has `DisableParallelization = true`. A per-test helper drops `harmony_automation` and `harmony_source_control`. |
| `.../Harness/RepositoryRoot.cs` | new | Walks up from the test assembly to the folder that holds `Harmony.slnx`. Used by the parity and image-tag tests. |
| `.../Conventions/RequirementTagGuardTests.cs` | new | The shared guard applied to this assembly (FR-007), copied from the unit test project. |
| `.../Conventions/HarnessConventionTests.cs` | new | FR-005: every relational test runs on both providers. |
| `.../Conventions/ProviderSetupParityTests.cs` | new | NFR-005 |
| `.../Conventions/ImageTagTests.cs` | new | NFR-006 |
| `.../Conventions/MongoDbCollectionConventionTests.cs` | new | NFR-003 |
| `.../Harness/DatabaseServersTests.cs` | new | FR-001, FR-003, NFR-002 |
| `.../Harness/DatabaseIsolationTests.cs` | new | FR-006 (two classes, A and B) |
| `.../Migrations/MigrationTests.cs` | new | FR-010, FR-011 |
| `.../Repositories/CardRepositoryTests.cs` | new | FR-020, FR-033, FR-034, FR-035 |
| `.../Services/BoardServiceLoadBoardTests.cs` | new | FR-030, and the board-load part of FR-020 |
| `.../Services/BoardServiceAccessTests.cs` | new | FR-021, FR-031, FR-032 |
| `.../Services/CardServiceBacklogReorderTests.cs` | new | FR-040 (also tagged `Bug` 11) |
| `.../Services/CardServiceSearchTests.cs` | new | FR-041 (also tagged `Bug` 17), FR-042 |
| `.../MongoDb/AutomationRepositoryTests.cs` | new | FR-050 |
| `.../MongoDb/SourceControlRepositoryTests.cs` | new | FR-051 |
| `AGENTS.md` | modify (Commands) | The integration test command (NFR-010). |
| `docs/architecture/overview.md` | modify (Known issues line 112, CI/CD line 101) | NFR-010 |
| `docs/roadmap.md` | modify | Notes on R-22 (5a, 5e) and R-08 (5b). A "Known defects found by R-03" group under Later, holding 5c (the new entry the spec asks for) and lines for 5a, 5b and 5e. |
| `.github/workflows/ci.yml` | **drafted by the agent in the scratchpad, applied by chsakell** | A new `Integration tests` job (FR-060, FR-061). The SDLC guard blocks agent edits. |

## Tasks

Tasks run in order, except the `[P]` tasks: T6-T10 and T11 share no file and can run in parallel once T5 is done. Each task starts with a failing test, and its commit ticks its box in this plan.

- [x] **T0** `build:` pull request, Testcontainers packages (Flagged concern 1)
  - Covers: NFR-008.
  - Files: `src/Directory.Packages.props`.
  - Work:
    - Branch `feature/19-testcontainers-packages` from `develop`.
    - Commit `build(deps): add Testcontainers 4.17.0 for integration tests` with footer `Refs #19`; PR title `#19 build: add Testcontainers packages`.
    - Check that restore reports no NU1903 for the new transitive packages; they would fail test builds under warnings as errors.
    - After it merges, merge `develop` into this branch.
  - Proof: `dotnet build Harmony.slnx` reports 0 errors and the same warning count; the NuGet licence metadata of all seven packages is MIT.
- [x] **T1** Project skeleton, opt-in and tag guard
  - Covers: FR-002, FR-004, FR-007.
  - Files: the csproj, `Harmony.slnx`, `Conventions/RequirementTagGuardTests.cs`, `AGENTS.md` (Commands).
  - Proof: `RequirementTagGuardTests.TestAssembly_EveryTestMethod_HasRequirementTag` passes under the integration test command. The test command's summary lists no integration test project. A filter that selects nothing exits with 8 (FR-004).
  - Also check: a second integration run right after `dotnet build Harmony.slnx` does not recompile the `src/` projects.
- [x] **T2** Database servers and Docker guard
  - Covers: FR-001, FR-003, NFR-002, NFR-006.
  - Files: `AssemblyInfo.cs`, `Harness/DatabaseServers.cs`, `Harness/DockerGuard.cs`, `Harness/ImageTags.cs`, `Harness/RepositoryRoot.cs`, `Harness/DatabaseServersTests.cs`, `Conventions/ImageTagTests.cs`.
  - Proof:
    - `DatabaseServersTests.GetSqlServerAsync_RequestedTwice_ReturnsSameContainer` (and the same for PostgreSQL and MongoDB)
    - `DatabaseServersTests.DockerGuard_DockerUnavailable_ThrowsMessageNamingDocker`
    - `DatabaseServersTests.StartedContainers_NamesDoNotStartWithHarmony`
    - `ImageTagTests.ImageTags_MatchDockerComposeFiles`
- [x] **T3** Provider setup and both-provider theories
  - Covers: FR-005, NFR-005.
  - Files: `Harness/RelationalProvider.cs`, `Harness/BothProvidersAttribute.cs`, `Harness/ProviderSetup.cs`, `Conventions/ProviderSetupParityTests.cs`, `Conventions/HarnessConventionTests.cs`.
  - Proof:
    - `ProviderSetupParityTests.HostDatabaseSetup_MatchesHarness` (one case each for Api, Automations and Notifications). It asserts both `MigrationsAssembly` names, `EnableLegacyTimestampBehavior", true`, and none of `EnableRetryOnFailure`, `UseQuerySplittingBehavior`, `AddInterceptors`, `UseCollation` or `ConfigureWarnings`.
    - `HarnessConventionTests.RelationalTests_EveryTestMethod_RunsOnBothProviders`: every test method with a `RelationalProvider` parameter, or in a class that uses `RelationalDatabaseFixture`, has `[BothProviders]`, and the attribute yields exactly the two providers.
- [x] **T4** Migration tests
  - Covers: FR-010, FR-011.
  - Files: `Harness/TestDatabase.cs`, `Migrations/MigrationTests.cs`.
  - Proof:
    - `MigrationTests.HarmonyContext_MigrateEmptyDatabase_AppliesAllMigrationsInOrder`, `AutomationContext_...` and `NotificationContext_...`, each `[BothProviders]`, so six cases. Each asserts that `GetAppliedMigrations()` equals `GetMigrations()` in order.
    - `MigrationTests.<Context>_Model_HasNoPendingModelChanges`, three methods × 2 providers, using `Database.HasPendingModelChanges()`. The context and the provider are in the display name.
  - This is US2. With T1-T3 it is the first shippable slice.
- [ ] **T5** Fresh database per class, test data, service factory
  - Covers: FR-006, NFR-003 (relational part).
  - Files: `Harness/RelationalDatabaseFixture.cs`, `Harness/TestData.cs`, `Harness/ServiceFactory.cs`, `Harness/PassThroughCacheService.cs`, `Harness/DatabaseIsolationTests.cs`.
  - Proof: `DatabaseIsolationTestsA.FreshDatabase_BoardWithSharedKey_SeesOnlyItsOwn` and the same in `DatabaseIsolationTestsB`. Both insert key `ISOL` and assert a count of 1. This is the one documented exception to the test data rule.
  - Early check (not committed): one throwaway call each of `LoadBoard`, `GetUserBoards` and `SearchWorkspaceCards` on both providers, to surface query translation failures before T6-T10 (see Risks).
- [ ] **T6** [P] Card repository
  - Covers: FR-020, FR-033, FR-034, FR-035.
  - Files: `Repositories/CardRepositoryTests.cs`.
  - Proof:
    - `CountCards_ListWithSubtask_ExcludesSubtask` and `CountActiveCards_...`
    - `Get_HiddenSubtask_ReturnsIt` and `Load_HiddenSubtask_ReturnsIt`
    - `GetNextSerialNumber_MixedCards_ReturnsCountOfAllBoardCardsPlusOne_KnownIssue` (5e): 2 cards in lists, 1 backlog card, 1 subtask and 1 archived card give 6; a card on another board changes nothing
    - `GetMaxActivePosition_EmptyList_ReturnsMinusOne` and `GetMaxActivePosition_ActiveAndArchived_ReturnsHighestActive`
    - `GetNextBacklogPosition_EmptyBacklog_ReturnsOne` and `GetNextBacklogPosition_OtherBoardHigher_ReturnsOwnMaxPlusOne`
    - `GetTotalChildren_ActiveAndArchivedSubtasks_CountsActiveOnly` and `GetTotalChildren_Set_OmitsCardsWithoutChildren`
- [ ] **T7** [P] Board load
  - Covers: FR-030, and the board-load part of FR-020.
  - Files: `Services/BoardServiceLoadBoardTests.cs`.
  - Works as `GetBoardHandler` does: `BoardService.GetBoard(boardId)`, then `LoadBoard(board, query, sprintId)`, where the query has non-null `ListIds` and `Assignees`.
  - Proof:
    - `LoadBoard_FullBoard_ListsHoldActiveTopLevelCardsWithSummaries`
    - `LoadBoard_SprintAndIssueTypeFilters_ReturnsOnlyCardsMatchingBoth`
    - `LoadBoard_SprintFilter_...`, `LoadBoard_IssueTypeFilter_...` and `LoadBoard_AssigneeFilter_...`
    - `LoadBoard_CardsPerListLimit_ReturnsAllCards_KnownIssue` (5a)
- [ ] **T8** [P] Board access and the user's board list
  - Covers: FR-021, FR-031, FR-032.
  - Files: `Services/BoardServiceAccessTests.cs`.
  - Proof:
    - `HasUserAccessToBoard_PrivateBoard_GrantsMemberRefusesWorkspaceMember`
    - `..._WorkspaceBoard_GrantsWorkspaceMemberRefusesOutsider`
    - `..._PublicBoard_GrantsAnyone`
    - `HasUserAccessToBoard_UnknownBoard_Throws_KnownIssue` (5b)
    - `GetUserBoards_MemberOfPrivateAndPublic_ReturnsEachOnce`
    - `GetUserBoards_WorkspaceGiven_ReturnsOnlyThatWorkspace`
    - `GetUserBoards_InactiveWorkspace_ExcludesItsBoards`
- [ ] **T9** [P] Backlog reorder
  - Covers: FR-040, and #11 against real SQL.
  - Files: `Services/CardServiceBacklogReorderTests.cs`.
  - Proof: `PositionCard_MoveUpInBacklog_ShiftsOnlySameBoard` and `PositionCard_MoveDownInBacklog_ShiftsOnlySameBoard`.
- [ ] **T10** [P] Workspace card search
  - Covers: FR-041, FR-042, and #17.
  - Files: `Services/CardServiceSearchTests.cs`.
  - Proof:
    - `SearchWorkspaceCards_NoSkippedCard_ReturnsNonArchivedMatchesInPositionOrderAndCountAgrees`
    - `SearchWorkspaceCards_SkippedCard_LeavesItOut`
    - `SearchWorkspaceCards_Paging_SplitsAndCountsAll`
    - `SearchWorkspaceCards_TermDiffersInCase_CaseInsensitiveOnSqlServerOnly_KnownIssue` (5c): `[BothProviders]`, asserting "found" on SQL Server and "not found" on PostgreSQL. This keeps FR-005 (every case on both providers) and pins only the PostgreSQL defect. The fix changes it to "found" on both.
- [ ] **T11** [P] MongoDB harness and automation store
  - Covers: FR-050, NFR-003 (MongoDB part).
  - Files: `Harness/MongoDbFixture.cs`, `Harness/MongoDbCollection.cs`, `MongoDb/AutomationRepositoryTests.cs`, `Conventions/MongoDbCollectionConventionTests.cs`.
  - Proof:
    - `AutomationRepository_RuleLifecycle_ReturnsLastSavedState`, which also checks that the other board's rule is never returned
    - `AutomationRepository_Templates_ListedInFullAndByNotificationType`
    - `MongoDbTests_AllInOneNonParallelCollection`
- [ ] **T12** Source-control store
  - Covers: FR-051. Runs after T11.
  - Files: `MongoDb/SourceControlRepositoryTests.cs`.
  - Proof:
    - `GetRepositoryAndBranch_Recorded_ReadBack` (also covers branch existence)
    - `SearchBranches_Term_SearchAndTotalAgree`
    - `CreatePush_Commit_BranchFoundByCommitId`
    - `AddTagToBranch_StoresTag`
    - `AddOrUpdatePullRequest_SameIdTwice_UpdatesNotDuplicates`
    - `DeleteBranch_RemovesIt`
- [ ] **T13** Docs and known issues
  - Covers: NFR-010, Flagged concern 5.
  - Files: `docs/architecture/overview.md`, `docs/roadmap.md`, `AGENTS.md` (final wording).
  - Each `_KnownIssue` test's XML comment names its roadmap line. R-03's roadmap status stays `in progress` and is set to `done` in the PR after #20 merges.
  - Proof: review against NFR-010; `RequirementTagGuardTests` still passes.
- [ ] **T14** CI job draft
  - Covers: FR-060, FR-061, NFR-001, NFR-004.
  - Files: draft only, `<scratchpad>/ci-integration-job.yml`. chsakell applies it to `.github/workflows/ci.yml`.
  - The draft job:
    - `integration:` named `Integration tests`, on `ubuntu-latest`, `timeout-minutes: 20`, in parallel with `Build and test` and on the same triggers;
    - checkout, setup-dotnet and the NuGet cache, the same as `build`;
    - `dotnet test --project tests/Harmony.Infrastructure.IntegrationTests -p:HarmonyIntegrationTests=true --report-xunit-trx --report-xunit-markdown`, with no `--no-build` (see Risks);
    - the summary published from `TestResults/Harmony.Infrastructure.IntegrationTests/*.md` under `!cancelled()`;
    - the `integration-test-results` artifact (TRX, Markdown and Cobertura), kept 14 days.
  - Proof: three green runs in a row on PR #20, each under 10 minutes. After that, chsakell adds the required check (Rollout, step 3).
- [ ] **T15** Verification (`/sdlc-verify`)
  - Covers: FR-001, FR-002, FR-003, FR-007, FR-011, NFR-002, NFR-007, NFR-009, NFR-011.
  - Checks that are proven during verification, not committed:
    - `docker ps -a` is clean about a minute after a run; Ctrl+C during a run is cleaned up by Ryuk; a deliberately broken test gives a non-zero exit and no containers left (FR-001);
    - a MongoDB-only filter starts no SQL Server or PostgreSQL container (FR-001);
    - with Docker stopped, the test command exits 0 and the integration test command fails with "Docker" (FR-002, FR-003);
    - an untagged method added locally fails the guard (FR-007);
    - an entity property added locally without a migration fails FR-011 on both providers;
    - with the Aspire containers running, their IDs and volumes are unchanged after a run (NFR-002);
    - a Cobertura file appears under `TestResults/Harmony.Infrastructure.IntegrationTests/` (NFR-009);
    - the integration test command passes once on this Windows machine with Docker Desktop (NFR-011).

### Coverage

| Requirement | Task(s) | Test(s) or check |
|---|---|---|
| FR-001 | T2, T15 | `DatabaseServersTests.Get*Async_RequestedTwice_ReturnsSameContainer`; checks: `docker ps -a`, Ctrl+C, MongoDB-only filter |
| FR-002 | T1, T15 | Check: test command with Docker stopped exits 0 and lists no integration project; the build output includes it |
| FR-003 | T2, T15 | `DatabaseServersTests.DockerGuard_DockerUnavailable_ThrowsMessageNamingDocker`; check: Docker stopped |
| FR-004 | T1 | Check: a filter that selects nothing exits with 8 |
| FR-005 | T3 | `HarnessConventionTests.RelationalTests_EveryTestMethod_RunsOnBothProviders`; TRX display names |
| FR-006 | T5 | `DatabaseIsolationTestsA/B.FreshDatabase_BoardWithSharedKey_SeesOnlyItsOwn` |
| FR-007 | T1, T15 | `RequirementTagGuardTests.TestAssembly_EveryTestMethod_HasRequirementTag` |
| FR-010 | T4 | `MigrationTests.<Context>_MigrateEmptyDatabase_AppliesAllMigrationsInOrder` (×6) |
| FR-011 | T4, T15 | `MigrationTests.<Context>_Model_HasNoPendingModelChanges` (×6) |
| FR-020 | T6, T7 | `CardRepositoryTests.CountCards_*`, `Get_/Load_HiddenSubtask_ReturnsIt`; `BoardServiceLoadBoardTests.LoadBoard_FullBoard_*` |
| FR-021 | T8 | `BoardServiceAccessTests.GetUserBoards_InactiveWorkspace_ExcludesItsBoards` |
| FR-030 | T7 | `BoardServiceLoadBoardTests.*`, including `_KnownIssue` (5a) |
| FR-031 | T8 | `BoardServiceAccessTests.HasUserAccessToBoard_*`, including `_KnownIssue` (5b) |
| FR-032 | T8 | `BoardServiceAccessTests.GetUserBoards_*` |
| FR-033 | T6 | `CardRepositoryTests.GetNextSerialNumber_*_KnownIssue` (5e) |
| FR-034 | T6 | `CardRepositoryTests.GetMaxActivePosition_*`, `GetNextBacklogPosition_*` |
| FR-035 | T6 | `CardRepositoryTests.GetTotalChildren_*` |
| FR-040 | T9 | `CardServiceBacklogReorderTests.*` |
| FR-041 | T10 | `CardServiceSearchTests.SearchWorkspaceCards_{NoSkippedCard,SkippedCard,Paging}_*` |
| FR-042 | T10 | `CardServiceSearchTests.SearchWorkspaceCards_TermDiffersInCase_*_KnownIssue` (5c) |
| FR-050 | T11 | `AutomationRepositoryTests.*` |
| FR-051 | T12 | `SourceControlRepositoryTests.*` |
| FR-060 | T14 | `Integration tests` check on PR #20 |
| FR-061 | T14 | Run summary and the `integration-test-results` artifact |
| NFR-001 | T14 | Job duration ≤ 10 minutes, three times |
| NFR-002 | T2, T15 | `DatabaseServersTests.StartedContainers_NamesDoNotStartWithHarmony`; check: Aspire containers unchanged |
| NFR-003 | T5, T11 | Isolation tests; `MongoDbCollectionConventionTests.MongoDbTests_AllInOneNonParallelCollection` |
| NFR-004 | T14 | Three green runs in a row; no automatic rerun configured |
| NFR-005 | T3 | `ProviderSetupParityTests.HostDatabaseSetup_MatchesHarness` (×3 hosts) |
| NFR-006 | T2 | `ImageTagTests.ImageTags_MatchDockerComposeFiles` |
| NFR-007 | T1-T13 | Build (0 errors, ≤ 585 code warnings), format of changed files |
| NFR-008 | T0 | NuGet licence metadata (MIT) for all new packages |
| NFR-009 | T15 | Cobertura file under `TestResults/Harmony.Infrastructure.IntegrationTests/` |
| NFR-010 | T1, T13 | AGENTS.md, overview and roadmap reviewed |
| NFR-011 | T15 | One green run on Docker Desktop for Windows |

## Risks and what could break

| Risk | Likelihood / impact | Mitigation |
|---|---|---|
| **The riskiest step: queries that do not translate, or behave differently, on PostgreSQL** (LoadBoard's `GetBoardListCards` specification, the `GetUserBoards` Union and Distinct, search). A failure would be a new known-wrong behaviour outside the spec's list. | Medium / scope | The T5 early check calls each one on both providers before T6-T10. Any new failure goes back to the product owner as a spec change, not into the plan. |
| The opt-in relies on how the MTP CLI selects test projects, through `IsTestingPlatformApplication`. A future SDK could change it, and then `dotnet test --solution` would start the containers. | Low / medium | The csproj comment records the trial. The T15 check with Docker stopped catches it, and in CI the `Build and test` job would suddenly take longer. The `HarmonyIntegrationTests=true` environment variable shows the tests in IDE Test Explorer (documented in AGENTS.md). |
| `--no-build` after a plain build: the MTP extension registration (coverlet, reports) is generated only when `IsTestingPlatformApplication` is true. | Medium / CI | The integration test command always builds; the CI job and AGENTS.md say so. |
| CI time: the SQL Server image is about 1.6 GB. | Medium / NFR-001 | Containers start lazily and in parallel. The first CI run measures it. If the job takes more than 10 minutes, raise it with the product owner before tuning anything. |
| A global `-p:` property rebuilds the `src/` projects as a second configuration. | Medium / time | `GlobalPropertiesToRemove` on the project references, checked in T1. |
| `MongoDbConfiguration.Configure()` throws when called twice, and the MongoDB databases have fixed names. | Certain / flaky tests | Called once per process; one non-parallel collection; databases dropped before each test. |
| The Npgsql legacy timestamp switch is process-wide. | Low | Set once in `ProviderSetup` before any Npgsql use, as the hosts do. It does not affect SQL Server. |
| New transitive packages raise NU1903 under warnings as errors. | Low / build | Checked in T0, before this branch depends on them. |
| Neighbouring flows | None at run time | No production code changes. The test command and the warning baseline are unchanged, because the new project builds with warnings as errors. The solution build gets one more project. |

## Alternatives considered

| Option | Why not chosen |
|---|---|
| `IsTestProject=false` unless opted in | Tried: MTP `dotnet test --solution` still ran the project. |
| A solution filter or second `.slnx` for the integration tests | The full `Harmony.slnx` would still run the project under the test command. Removing it from `Harmony.slnx` breaks FR-002, because the build must compile it. |
| A default trait filter | Ruled out by the spec: exit code 8 breaks FR-002. |
| An environment variable as the documented switch | The syntax differs between shells; `-p:` works the same in PowerShell and bash. The environment variable stays as the IDE route. |
| `dotnet run --project` on the test executable | Skips `TestingPlatformCommandLineArguments`, so coverage and the results directory would need extra arguments (NFR-009). |
| An abstract test class with one subclass per provider | Doubles the classes. The theory form gives one method per case, with the provider in the display name. |
| `InternalsVisibleTo` and references to the three hosts, to call `AddDatabase` | Pulls the hosts' `appsettings.json`, Hangfire, RabbitMQ and gRPC setup into the test project. It would also be a production change. |
| A shared provider-setup helper | Touches three hosts. Deferred by the tech lead's proposal in Flagged concern 2. |
| Aspire.Hosting.Testing to start the databases | Brings the whole AppHost, with fixed ports, named volumes and `harmony-` names, which breaks NFR-002. |
| Respawn or truncation between tests | A new package. Not needed, because each class has a fresh database and tests create unique data. |
| Real `InMemoryCacheService` | Caching would hide database reads; the point is to prove the SQL. |
| A container per test class | Slower. The spec chose one instance per engine per run. |

## Migration and rollback

There is no EF Core migration, and no production code or data changes.

- **Rollout:** the T0 `build:` PR merges first. Then PR #20 merges, with chsakell applying the CI job and, after three green runs, making `Integration tests` a required check on `develop` and `main`.
- **Rollback:** revert PR #20 and remove the required check. Revert the `build:` PR only if nothing else uses Testcontainers by then.

## Proof (definition of done)

- Build: `dotnet build Harmony.slnx` reports `0 Error(s)` and no more than 585 code warnings.
- Test: `dotnet test --solution Harmony.slnx` passes, and the summary lists no integration project.
- Integration: `dotnet test --project tests/Harmony.Infrastructure.IntegrationTests -p:HarmonyIntegrationTests=true` passes with Docker running. Per requirement, for example: `... --filter-trait "Requirement=FR-010"` (total above 0).
- Format: `dotnet format Harmony.slnx --verify-no-changes --include <changed files>` exits 0.
- Every FR and NFR in the coverage table has a passing tagged test or a recorded verification check.
- The `Integration tests` check passes three times in a row on PR #20, each in 10 minutes or less.

## Found while planning (for the product owner, not in scope)

Read from the code on 2026-11-26. They are not pinned, because the spec does not list them.

- `SourceControlRepository.DeleteBranch` matches on branch name only, across repositories.
- `FindBranchByCommit` ignores its `repositoryId`.
- `AutomationRepository.GetAutomations(List<AutomationType>, Guid)` always returns `SyncParentAndChildIssuesAutomationDto`.
- `BoardService.GetBoard` includes the filtered `Workspace`, so a board in an inactive workspace probably comes back as null.

Decided by chsakell (2026-11-26): T13 adds them as unpinned lines under the roadmap's Later section. No tests and no change to the spec's scope.

## Deviations log

| Date | Task | What changed and why |
|---|---|---|
| 2026-11-26 | T0 | The `build:` PR is #21. Its branch was merged into this branch before #21 merged, so T1-T13 could start (chsakell's decision). The change is identical, so merging `develop` after #21 merges brings nothing new; that merge is still to do. |
| 2026-11-26 | T0 | The transitive closure is 16 packages, not seven. It also includes BouncyCastle.Cryptography 2.7.0 and six `Docker.DotNet.Enhanced.*` sub-packages. All 16 are MIT, and none has a known vulnerability. |
| 2026-11-26 | T0 | `develop` has no `docs/features/19-*/` folder, so #21 fails the SDLC gate once it is not a draft. chsakell adds `sdlc:track-s` to #21 to pass it. |
| 2026-11-26 | T1 | The check "a second integration run right after `dotnet build Harmony.slnx` does not recompile the `src/` projects" cannot pass as written. Two plain `dotnet build Harmony.slnx` runs in a row already rewrite 16-17 `src/` assemblies, with or without this feature, because each `*.GeneratedMSBuildEditorConfig.editorconfig` is rewritten (it was not investigated further; that behaviour predates this feature). What the check guards against was verified instead: the build log of an integration run shows each `src/` project built once, under one set of global properties, so `GlobalPropertiesToRemove` stops a second configuration. The test project itself recompiles when you switch between the two commands, because xUnit generates a different entry point when `IsTestingPlatformApplication` is false. |
| 2026-11-26 | T2 | One extra test, `DatabaseServersTests.DockerGuard_BuildFindsNoDocker_ThrowsMessageNamingDocker` (FR-003). Builders throw `DockerUnavailableException` from Build when there is no endpoint, while an unreachable endpoint fails in Start, so each path gets a test. The planned test covers Start, using a real builder pointed at `tcp://127.0.0.1:1`, so neither test has to stop Docker. `DockerGuard.StartAsync` takes the build step as a `Func<TContainer>`, so the Build failure goes through the guard too. |
| 2026-11-26 | T3 | `HarnessConventionTests` checks methods with a `RelationalProvider` parameter now. The other half of the rule, "or in a class that uses `RelationalDatabaseFixture`", moves to T5, which creates the fixture. The check is also stricter than planned: a relational test must have `[BothProviders]` and no other data attribute, because an extra `[InlineData(SqlServer)]` would run one provider twice. `ProviderSetupParityTests` compares case-sensitively (Shouldly's default ignores case) and also checks that the host calls `AddDbContext<context>`, `UseSqlServer` and `UseNpgsql`. `ProviderSetup` turns on `EnableDetailedErrors` and `EnableSensitiveDataLogging` as the hosts do; they change only error messages. |
| 2026-11-26 | T4 | `TestDatabase` has `CreateEmptyAsync`, `MigrateAsync<TContext>()` and `CreateContext<TContext>()`, plus `CreateUnconnectedContext<TContext>(provider)`. The FR-011 checks use the last one: `HasPendingModelChanges()` compares the model with the snapshot and opens no connection, so they create no database. The database name is `h_` plus 8 hex digits, as planned. |
