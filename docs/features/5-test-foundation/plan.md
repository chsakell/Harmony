---
id: 5
title: Test foundation
artifact: plan
status: draft            # draft | approved | superseded
owner: chsakell
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:plan-approved label is added
created: 2026-11-26
spec: ./spec.md
---

# Plan: Test foundation

## Approach

The solution gets its first test project, `tests/Harmony.Application.UnitTests`, which references only `Harmony.Application`. It follows the accepted ADR [2026-11-26-testing-strategy](../../adr/2026-11-26-testing-strategy.md):

- **Packages:** xUnit v3 on Microsoft.Testing.Platform (MTP), NSubstitute, Shouldly and coverlet.MTP.
- **Runner opt-in:** `global.json` opts into MTP.
- **Shared settings:** `tests/Directory.Build.props` and `tests/Directory.Packages.props` import their `src/` counterparts, so the target framework and the single version list stay where they are.

All code under test is already `public`, so there are **no production code changes**. The only file under `src/` that changes is `src/Directory.Packages.props`, where a new "Testing" group is appended.

Tests call the routines directly:

- `CardHelper.GetSerialKey` and `CardHelper.DisplayDates`;
- `IssueTypeExtensions.Normal`;
- `PredicateBuilder.And` and `PredicateBuilder.Or`;
- `WorkspaceProfile`, through a `MapperConfiguration` built with `AddMaps(typeof(WorkspaceProfile).Assembly)`. This mirrors `AddApplicationLayer`.

`UpdateCardTitleCommandHandler` is constructed directly with NSubstitute doubles, not through the MediatR pipeline.

Two guard tests enforce conventions:

- **Requirement tag (FR-005):** checks every test method in the assembly by reflection.
- **Unmapped members (FR-016, FR-017):** compares AutoMapper's validation errors with a checked-in baseline.

Each guard's logic is a pure function, so its failure scenarios are proven with test fixtures, without breaking the real suite.

**Test-first for pinning tests.** The helper, mapping and handler code already exists, so the new tests pass on first run. Their red step is to run each test once with a deliberately wrong expected value, see it fail with a clear message, and then restore it.

## Files that change

| File | Change | Why |
|---|---|---|
| `global.json` | modify | Add `"test": { "runner": "Microsoft.Testing.Platform" }` (ADR decision 1, FR-001, FR-003) |
| `src/Directory.Packages.props` | modify | Append a `<!-- Testing -->` ItemGroup with the versions chosen in T0: xUnit v3 package, Shouldly, NSubstitute, coverlet.MTP (NFR-004, NFR-006). Logged exception to the separate `build:` PR rule (spec Flagged concern 5). |
| `tests/Directory.Build.props` | new | `<Import>`s `../src/Directory.Build.props`, then sets: <br>- `IsPackable=false`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true` (NFR-005); <br>- coverage on every run, in Cobertura format, with results under the repository root `TestResults/<project>/` (FR-004; mechanism chosen in T0); <br>- `NuGetAuditSuppress` for the single AutoMapper 14.0.0 advisory URL reported by NU1903, with a comment pointing to the overview Known issue and R-14 (decided 2026-11-26). |
| `tests/Directory.Packages.props` | new | `<Import>`s `../src/Directory.Packages.props`, so central package management reaches `tests/` (spec Flagged concern 2) |
| `tests/Harmony.Application.UnitTests/Harmony.Application.UnitTests.csproj` | new | `OutputType=Exe`; references the T0 packages (no `Version`) and a `ProjectReference` to `Harmony.Application` only (NFR-002, NFR-006) |
| `tests/Harmony.Application.UnitTests/Conventions/RequirementTagGuard.cs` | new | Pure function `IReadOnlyList<string> FindViolations(IEnumerable<Type> testClasses)`. <br>- A test method is any method with an attribute that derives from `FactAttribute`. <br>- The `Requirement` trait is read from the method or, failing that, the class. <br>- The value must match `^(FR\|NFR)-\d{3}$`. <br>- Each violation names `Class.Method`. |
| `tests/Harmony.Application.UnitTests/Conventions/RequirementTagGuardTests.cs` | new | FR-005 tests. Fixture classes are nested `public abstract` classes, so xUnit does not run them. The assembly guard selects only public non-abstract classes, the same classes xUnit runs. |
| `tests/Harmony.Application.UnitTests/Conventions/TestProjectIsolationTests.cs` | new | NFR-002: the test assembly references no `Harmony.Persistence*`, `Harmony.Infrastructure`, `Harmony.Messaging`, `Harmony.Caching`, `Harmony.Api`, `Harmony.Automations`, `Harmony.Notifications`, `Harmony.SignalR` or `Harmony.Integrations.*` assembly |
| `tests/Harmony.Application.UnitTests/Helpers/CardHelperGetSerialKeyTests.cs` | new | FR-006, FR-007 |
| `tests/Harmony.Application.UnitTests/Helpers/CardHelperDisplayDatesTests.cs` | new | FR-008 to FR-011 |
| `tests/Harmony.Application.UnitTests/TestSupport/CultureScope.cs` | new | `IDisposable` that sets `CultureInfo.CurrentCulture` and `CurrentUICulture`, then restores them. Culture flows with the execution context, so it is safe when tests run in parallel (NFR-003). |
| `tests/Harmony.Application.UnitTests/Extensions/IssueTypeExtensionsTests.cs` | new | FR-012 |
| `tests/Harmony.Application.UnitTests/Extensions/PredicateBuilderTests.cs` | new | FR-013, FR-014 |
| `tests/Harmony.Application.UnitTests/Mapping/MappingConfigurationFactory.cs` | new | Builds the `MapperConfiguration` from the `Harmony.Application` assembly once, for T6 and T7 |
| `tests/Harmony.Application.UnitTests/Mapping/WorkspaceProfileMappingTests.cs` | new | FR-015: the six mappings with custom rules |
| `tests/Harmony.Application.UnitTests/Mapping/UnmappedMemberGuard.cs` | new | Pure function `Compare(IEnumerable<(string TypeMap, string Member)> actual, IEnumerable<(string, string)> baseline)`. It returns new unmapped pairs and stale baseline entries, and its message names each one. |
| `tests/Harmony.Application.UnitTests/Mapping/UnmappedMemberBaseline.cs` | new | Checked-in list: 28 `(TypeMap, Member)` entries across 12 type maps. `TypeMap` is written as `"Source -> Destination"`. |
| `tests/Harmony.Application.UnitTests/Mapping/UnmappedMemberGuardTests.cs` | new | FR-016, FR-017 |
| `tests/Harmony.Application.UnitTests/Features/Cards/UpdateCardTitleCommandHandlerTests.cs` | new | FR-018 to FR-020: the reference handler test pattern (US4) |
| `Harmony.slnx` | modify | Add a `/tests/` folder with the project. Add `tests/Directory.Build.props` and `tests/Directory.Packages.props` to `/Solution Items/`. |
| `AGENTS.md` | modify | DEL-2, in the Commands section: <br>- the Test line becomes `dotnet test --solution Harmony.slnx`; <br>- add the filter example `--filter-trait "Requirement=FR-006"` and where the coverage report is written (`TestResults/<project>/`); <br>- remove "There are no test projects yet" and "`dotnet test --solution` does not work". |
| `docs/architecture/overview.md` | modify | DEL-2: replace the Known issues entry "No automated tests" with what now exists and what is still missing (CI, integration tests). The decision index already lists the ADR. |
| `docs/roadmap.md` | modify | DEL-3: each "Known defects found by R-01" line names the exact `_KnownIssue` test method that pins it |
| `docs/features/5-test-foundation/plan.md` | modify | Tick tasks and record the T0 spike results and any deviations |

## Tasks

Tasks run in order. `[P]` marks a task that touches no file used by another `[P]` task, so it can run in a parallel session. Each task starts with a failing test that proves it.

- [ ] **T0 Spike (no repository change).** Settle the open questions the spec carried forward. Work in a scratch folder outside the repository: copy `global.json` and the two `src/Directory.*.props` files, then add one scratch test project that references `src/Core/Harmony.Application` by absolute path. On SDK 10.0.404, confirm:
  1. **Packages:** xunit.v3 4.0.3. Check whether it uses MTP v2 by default; if not, use `xunit.v3.mtp-v2`. Check that coverlet.MTP 10.1.1, Shouldly 4.3.0 and NSubstitute 6.2.0 all restore against the **same MTP major version**. Record each package's licence expression from NuGet (NFR-004).
  2. **Runner:**
     - `dotnet test --solution <scratch.slnx>` runs the project;
     - `--filter-trait "Requirement=FR-006"` filters;
     - a failing assertion gives a non-zero exit code;
     - a non-test project in the same solution is skipped, not failed.
  3. **Coverage with no command-line options (FR-004).** Try `TestingPlatformCommandLineArguments` in Directory.Build.props first (`--coverage --coverage-output-format cobertura --results-directory ...`), then the coverlet.MTP settings in `testconfig.json`. Check that the Cobertura file lists `Harmony.Application` and no `*.UnitTests` assembly. **If neither works, stop and take FR-004 back to the product owner. Do not document `--coverage` as a workaround.**
  4. **Audit:** NU1903 appears for transitive AutoMapper. Record the advisory URL. Check that `NuGetAuditSuppress` with that URL removes it and nothing else.
  5. **AutoMapper:** `new MapperConfiguration(cfg => cfg.AddMaps(appAssembly)).AssertConfigurationIsValid()` throws `AutoMapperConfigurationException`, and its `Errors` give 28 unmapped members across 12 type maps. **If the numbers differ from the spec, stop and report.**

  Record the outcome, package versions and chosen mechanism in "Spike results" in this plan. Commit as `docs(plan): record test-foundation spike`. Covers: the open question carried forward in the spec, and NFR-004.
- [ ] **T1 Test infrastructure and requirement-tag guard.**
  - **Covers:** FR-001, FR-002, FR-003, FR-004, FR-005, NFR-002, NFR-005, NFR-006.
  - **Files:**
    - `global.json`, `src/Directory.Packages.props`, `tests/Directory.Build.props`, `tests/Directory.Packages.props`, `Harmony.slnx`;
    - `tests/Harmony.Application.UnitTests/Harmony.Application.UnitTests.csproj`;
    - `Conventions/RequirementTagGuard.cs`, `Conventions/RequirementTagGuardTests.cs`, `Conventions/TestProjectIsolationTests.cs`.
  - **Red step:** stub `FindViolations` to return an empty list and watch the fixture tests fail. Then implement it.
  - **Proof:**
    - `RequirementTagGuardTests.FindViolations_MethodWithoutTag_ReturnsMethodName`
    - `RequirementTagGuardTests.FindViolations_TagValueNotRequirementId_ReturnsMethodName` (value `TODO`)
    - `RequirementTagGuardTests.FindViolations_ClassLevelTag_ReturnsNoViolation`
    - `RequirementTagGuardTests.TestAssembly_EveryTestMethod_HasRequirementTag`
    - `TestProjectIsolationTests.TestAssembly_References_NoInfrastructureOrServiceAssembly` (NFR-002)
  - **Also:** run the FR-002, FR-003 and FR-004 command checks listed under Proof.
- [ ] **T2 [P] Card key extraction.**
  - **Covers:** FR-006, FR-007.
  - **File:** `Helpers/CardHelperGetSerialKeyTests.cs`.
  - **Proof:**
    - `GetSerialKey_KeyWithAllowedNeighbours_ReturnsFirstKey`, a theory with three cases: `"HAR-42 fix login"`, `"feature/HAR-42_login"`, `"fixes abc-7 and HAR-9"` → `abc-7`.
    - `GetSerialKey_NoKeyWithAllowedNeighbours_ReturnsNull`, a theory with five cases: `"no key here"`, `"#HAR-42"`, `"see HARMONY-12"`, `"HAR-42x"`, `"AB-1"`.
    - `GetSerialKey_KeyFollowedByPeriod_ReturnsNull_KnownIssue`, for `"Fixes HAR-42."` (concern 4f).
    - `GetSerialKey_NullText_Throws_KnownIssue`, which expects `ArgumentNullException` (concern 4d).
- [ ] **T3 [P] Card date display.**
  - **Covers:** FR-008, FR-009, FR-010, FR-011, NFR-003.
  - **Files:** `Helpers/CardHelperDisplayDatesTests.cs`, `TestSupport/CultureScope.cs`.
  - **Rules:**
    - Every test sets `CultureInfo.InvariantCulture` through `CultureScope`, except the el-GR test.
    - All dates are `new DateTime(...)` literals.
  - **Proof:**
    - `DisplayDates_SameMonth_ShowsMonthAndBothDays` → `Mar 05 - 19`
    - `DisplayDates_DifferentMonths_ShowsBothMonthsWithoutYears` → `Mar 28 - Apr 03`
    - `DisplayDates_OnlyStartDate_ShowsStartDate` → `Mar 05`
    - `DisplayDates_OnlyDueDate_ShowsDueDate` → `Apr 03`
    - `DisplayDates_NoDates_ReturnsEmpty`
    - `DisplayDates_SameMonthDifferentYear_OmitsYear_KnownIssue` → `Jan 05 - 10` (concern 4a)
    - `DisplayDates_GreekCulture_UsesGreekMonthName_KnownIssue` (concern 4b). Expected month: `CultureInfo.GetCultureInfo("el-GR").DateTimeFormat.AbbreviatedMonthNames[2]`, and the output does not start with `Mar`.
- [ ] **T4 [P] Issue-type filter.**
  - **Covers:** FR-012, as amended on 2026-11-26 to use `SubTask`.
  - **File:** `Extensions/IssueTypeExtensionsTests.cs`.
  - **Proof:**
    - `Normal_ListWithSubTask_ReturnsListWithoutSubTask`. Input: "Story", "Bug", `IssueTypesConstants.SUBTASK`. Output: "Story", "Bug".
    - `Normal_NullList_ReturnsNull`
    - `Normal_TypeWithoutSummary_Throws_KnownIssue`, which expects `NullReferenceException` (concern 4c).
- [ ] **T5 [P] Query-filter builder.**
  - **Covers:** FR-013, FR-014.
  - **File:** `Extensions/PredicateBuilderTests.cs`.
  - **Proof:**
    - `And_TwoConditions_MatchesOnlyWhenBothTrue`: compiled `x > 2` and `y < 5` match 3, not 1 or 6.
    - `And_TwoConditions_HasSingleParameter`
    - `Or_TwoConditions_MatchesWhenEitherTrue`: `x < 2` or `y > 5` match 1 and 6, not 3.
    - `Or_TwoConditions_HasSingleParameter`

  The conditions deliberately use different parameter names, `x` and `y`.
- [ ] **T6 [P] Mapping behaviour pairs.**
  - **Covers:** FR-015.
  - **Files:** `Mapping/MappingConfigurationFactory.cs`, `Mapping/WorkspaceProfileMappingTests.cs`.
  - **Approach:** one test per mapping. Each builds a source with distinct values for every rule input and asserts every rule's destination member against a hand-written value.
  - **Proof:**
    - `Map_BoardToGetWorkspaceBoardResponse_CountsUsers`: `TotalUsers`.
    - `Map_CardToCardDto_ComputesTotalsAndLabels`:
      - totals: `TotalItems` 3, `TotalItemsCompleted` 1, `TotalAttachments` 2, `TotalComments`, `TotalChildren`;
      - labels: `Labels`, from `CardLabel.Label`;
      - list fields: `BoardList` is the list title, and `Completed` is `true` when the list's `CardStatus` is `DONE`.
    - `Map_CardWithoutBoardList_SetsBoardListEmptyAndCompletedNull`: the null branches of those two rules.
    - `Map_CardToLoadCardResponse_UsesIssueTypeBoardWhenPresent`: `BoardId` and `BoardTitle` come from `IssueType.Board`; also `BoardType`, `Labels`, and `IsChild` set to `true`.
    - `Map_CardToLoadCardResponse_UsesBoardListBoardWithoutIssueType`
    - `Map_CardLabelToLabelDto_TakesLabelTitle`
    - `Map_AttachmentToAttachmentDto_BuildsFilesUrl`: `files/<description of Type>/<FileName>`. The expected text is written out by hand, not computed with `ToDescriptionString`.
    - `Map_UserCardToCardMemberDto_UsesUserIdAsId`
  - **Starting point:** the six `CreateMap` statements in `src/Core/Harmony.Application/Mappings/Management/ManagementProfile.cs`.
- [ ] **T7 Unmapped-member guard.**
  - **Covers:** FR-016, FR-017.
  - **Files:** `Mapping/UnmappedMemberGuard.cs`, `Mapping/UnmappedMemberBaseline.cs`, `Mapping/UnmappedMemberGuardTests.cs`. It also uses `MappingConfigurationFactory.cs` from T6, so it runs after T6.
  - **Actual pairs:** catch `AutoMapperConfigurationException` from `AssertConfigurationIsValid()` and flatten `Errors[].TypeMap.Types` × `UnmappedPropertyNames`.
  - **Proof:**
    - `Compare_NewUnmappedMember_FailsNamingTypeMapAndMember`
    - `Compare_BaselineEntryNowMapped_FailsAskingToRemoveEntry`
    - `Configuration_UnmappedMembers_MatchBaseline`: the real configuration against the 28-entry baseline. Tagged with both FR-016 and FR-017.

  The class-level trait carries one ID. The per-method traits name the exact requirement.
- [ ] **T8 Reference handler test.**
  - **Covers:** FR-018, FR-019, FR-020.
  - **File:** `Features/Cards/UpdateCardTitleCommandHandlerTests.cs`.
  - **Setup:**
    - The handler is built with substitutes for `ICardRepository`, `ICurrentUserService`, `ICardActivityService`, `ISearchService`, `INotificationsPublisher`, `IMapper`, `IBoardService` and `IStringLocalizer<UpdateCardTitleCommandHandler>`.
    - The localizer returns `new LocalizedString(key, key)`.
    - The card returned by `Get` has a fixed `DateUpdated`. The handler calls `.Value` on it, and in production the persistence layer sets it.
    - `GetBoardInfo` returns a `BoardInfo` with a known `IndexName`.
  - **Proof:**
    - `Handle_UserNotSignedIn_ReturnsFailedAndDoesNotSave`: a theory for a `null` and an empty user ID. It checks: result failed; `Update` not received; `CreateActivity` not received.
    - `Handle_TitleSaved_ReturnsSuccessRecordsActivityAndPublishes`, with `Update` returning 1. It checks:
      - result succeeded;
      - `CreateActivity(cardId, userId, CardActivityType.CardTitleUpdated, fixedDate, newTitle)` received once;
      - `PublishSearchIndexNotificationAsync(Arg.Is<CardTitleUpdatedIndexNotification>(n => n.ObjectID == cardId.ToString() && n.Title == newTitle), indexName)` received once;
      - `PublishMessageAsync(Arg.Is<CardTitleChangedMessage>(m => m.BoardId/CardId/Title match), NotificationType.CardTitleChanged, BrokerConstants.RoutingKeys.SignalR)` received once.
    - `Handle_NothingSaved_ReturnsFailedAndPublishesNothing`, with `Update` returning 0. It checks: result failed; no activity; no call to `INotificationsPublisher`, using `ReceivedCalls()` empty.

  The test class has a short header comment marking it as the pattern to copy for R-05, R-15 and R-16.
- [ ] **T9 Documentation.**
  - **Covers:** DEL-2, DEL-3.
  - **Files:** `AGENTS.md`, `docs/architecture/overview.md`, `docs/roadmap.md`.
  - **Proof:** each documented command is run exactly as written and succeeds. Each roadmap defect line names an existing `_KnownIssue` method, checked with `grep`.
- [ ] **T10 Non-functional verification**, done in `/sdlc-verify`.
  - **Covers:** NFR-001, NFR-002, NFR-003, NFR-005, NFR-007.
  - **Proof:** see Proof below; record the results in the PR verification block.

### Coverage

| Requirement | Task(s) | Test(s) or check |
|---|---|---|
| FR-001 | T1 | Every test reported as passed by `dotnet test --solution Harmony.slnx` (verification) |
| FR-002 | T1, T10 | Break one assertion locally, then check that `$LASTEXITCODE` is not 0. Not committed, as the spec says. |
| FR-003 | T1, T10 | `--filter-trait "Requirement=FR-006"` runs only the `GetSerialKey_KeyWithAllowedNeighbours_*` cases |
| FR-004 | T0, T1, T10 | The Cobertura file exists under `TestResults/Harmony.Application.UnitTests/` for a plain run and for a filtered run. It lists `Harmony.Application` and no `*.UnitTests`. |
| FR-005 | T1 | The `RequirementTagGuardTests.*` tests listed in T1 |
| FR-006 | T2 | `GetSerialKey_KeyWithAllowedNeighbours_ReturnsFirstKey` |
| FR-007 | T2 | `GetSerialKey_NoKeyWithAllowedNeighbours_ReturnsNull`, `GetSerialKey_KeyFollowedByPeriod_ReturnsNull_KnownIssue`, `GetSerialKey_NullText_Throws_KnownIssue` |
| FR-008 | T3 | `DisplayDates_SameMonth_ShowsMonthAndBothDays`, `DisplayDates_SameMonthDifferentYear_OmitsYear_KnownIssue`, `DisplayDates_GreekCulture_UsesGreekMonthName_KnownIssue` |
| FR-009 | T3 | `DisplayDates_DifferentMonths_ShowsBothMonthsWithoutYears` |
| FR-010 | T3 | `DisplayDates_OnlyStartDate_ShowsStartDate`, `DisplayDates_OnlyDueDate_ShowsDueDate` |
| FR-011 | T3 | `DisplayDates_NoDates_ReturnsEmpty` |
| FR-012 | T4 | `Normal_ListWithSubTask_ReturnsListWithoutSubTask`, `Normal_NullList_ReturnsNull`, `Normal_TypeWithoutSummary_Throws_KnownIssue` |
| FR-013 | T5 | `And_TwoConditions_MatchesOnlyWhenBothTrue`, `And_TwoConditions_HasSingleParameter` |
| FR-014 | T5 | `Or_TwoConditions_MatchesWhenEitherTrue`, `Or_TwoConditions_HasSingleParameter` |
| FR-015 | T6 | `WorkspaceProfileMappingTests.Map_*` (8 tests) |
| FR-016 | T7 | `Compare_NewUnmappedMember_FailsNamingTypeMapAndMember`, `Configuration_UnmappedMembers_MatchBaseline` |
| FR-017 | T7 | `Compare_BaselineEntryNowMapped_FailsAskingToRemoveEntry`, `Configuration_UnmappedMembers_MatchBaseline` |
| FR-018 | T8 | `Handle_UserNotSignedIn_ReturnsFailedAndDoesNotSave` |
| FR-019 | T8 | `Handle_TitleSaved_ReturnsSuccessRecordsActivityAndPublishes` |
| FR-020 | T8 | `Handle_NothingSaved_ReturnsFailedAndPublishesNothing` |
| NFR-001 | T10 | The duration reported by the runner is under 30 s |
| NFR-002 | T1, T10 | `TestAssembly_References_NoInfrastructureOrServiceAssembly`, plus one run with `--no-restore`, Docker stopped and the network off |
| NFR-003 | T3, T10 | `CultureScope` used in the date tests. The suite passes with `Set-Culture en-US` and again with `Set-Culture el-GR`, in a new shell each time, and the original culture is restored afterwards. `grep` finds no `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now` or `TimeZoneInfo.Local` under `tests/`. |
| NFR-004 | T0 | The licence expression of each package is recorded in Spike results: Apache-2.0, BSD-3-Clause, BSD-3-Clause, MIT |
| NFR-005 | T1, T10 | `TreatWarningsAsErrors` in `tests/`. The build reports ≤ 669 warnings and 0 from the test project. Format passes for the changed files. |
| NFR-006 | T1 | Review: no `Version=` in the test `.csproj`; shared settings only in `tests/Directory.Build.props` |
| NFR-007 | T10 | `git diff develop --stat -- src/` shows only `src/Directory.Packages.props` |
| DEL-1 | - | The ADR is already accepted and listed in the overview decision index. No change. |
| DEL-2 | T9 | The documented commands run as written |
| DEL-3 | T9 | The roadmap lines name the `_KnownIssue` tests. Concern 4e (an unused map) is a removal for R-14; there is no behaviour to pin. |

## Risks and what could break

| Risk | Likelihood / impact | Mitigation |
|---|---|---|
| **Riskiest:** xUnit v3, coverlet.MTP and SDK 10.0.404 disagree on the MTP major version, or `dotnet test --solution` fails on non-test projects (AppHost, Blazor client) | Medium / blocks everything | T0 spike before any repository change. It runs at solution level with a non-test project included. |
| Coverage cannot be switched on without command-line options | Medium / FR-004 not met | T0 tries two mechanisms. If both fail, take FR-004 back to the product owner; do not document a workaround. |
| The `global.json` runner opt-in changes `dotnet test` for everyone. The old `dotnet test Harmony.slnx` form and older IDE test explorers stop working. | Certain / low | AGENTS.md is updated in the same PR. R-02 uses the new command. Dockerfiles do not copy `global.json`, so container builds are unaffected. |
| `NuGetAuditSuppress` hides a real advisory | Low / medium | Scoped to one advisory URL in `tests/` only. The advisory still shows for every `src/` project and stays in the overview Known issues until R-14. |
| `CentralPackageTransitivePinningEnabled` causes a version conflict or downgrade (NU1109) with test-package dependencies | Low / build break | T0 restores against the real props. Only a new ItemGroup is appended to `src/Directory.Packages.props`, a hot spot other branches also edit, so merge conflicts stay trivial. |
| The AutoMapper validation error count differs from the 28 in 12 recorded in the spec | Low / spec mismatch | T0 checks it. If it differs, stop and report to the product owner. |
| Culture-dependent tests fail on Linux CI (R-02) if ICU is missing or invariant globalization is on | Low / CI failure | The test project does not set `InvariantGlobalization`. Ubuntu runners ship ICU. Noted for R-02. |
| The handler test is brittle against R-08 and R-24 changes | Expected / low | Intended: those items change the tests on purpose. The missing-card path is not pinned (spec). |
| Neighbouring flows | None at runtime | No `src/` code changes. Only package versions are added, and no `src/` project references them. |

## Alternatives considered

| Option | Why not chosen |
|---|---|
| Put the requirement-tag guard in a shared `tests/Harmony.Testing` library now | Only one test project exists. The ADR asks for a guard per project. R-03 can extract it when a second project arrives. |
| Store the unmapped-member baseline as a text file | Needs content-copy settings and file I/O. A C# list is compile-checked and diffs just as clearly. |
| Read unmapped members by walking `TypeMap`s through `AutoMapper.Internal` | Internal API that can change between versions. The validation exception's `Errors` is what `AssertConfigurationIsValid` reports, which is the definition the spec's count uses. |
| Test the handler through `IMediator` with DI | Adds validation and behaviour pipelines and a container to the reference pattern. Direct construction is simpler to copy. |
| `InternalsVisibleTo` on `Harmony.Application` | Not needed: everything under test is public. Avoiding it keeps NFR-007's `src/` diff at one file. |
| Coverage results in each project's `bin/.../TestResults` (the MTP default) | Harder for R-02 to collect. The repository root `TestResults/<project>/` is already in `.gitignore`. |
| Frameworks and libraries (VSTest, MSTest, Moq, AwesomeAssertions, Microsoft code coverage) | Decided in the ADR |

## Migration and rollback

- **Migration:** no EF Core migration, data change, feature flag or deployment.
- **Rollback:** revert the merge commit. `global.json`, the Testing package group, `tests/` and the documentation return together. Contributors then go back to `dotnet test Harmony.slnx`, which runs nothing, as before.

## Proof (definition of done)

- **Build:** `dotnet build Harmony.slnx` ends with `0 Error(s)`, ≤ 669 warnings, and none from `Harmony.Application.UnitTests`.
- **Test:** `dotnet test --solution Harmony.slnx` passes with every test green, and the reported duration is under 30 s (NFR-001).
- **Format:** `dotnet format Harmony.slnx --verify-no-changes --include <every changed .cs/.csproj/.props file>` exits with code 0.
- **Requirement filter:** `dotnet test --solution Harmony.slnx --filter-trait "Requirement=FR-006"` runs only FR-006 tests (FR-003).
- **Coverage:** after a plain run and again after the filtered run, `TestResults/Harmony.Application.UnitTests/*.cobertura.xml` exists. It contains `Harmony.Application` and no `UnitTests` package (FR-004).
- **Exit code:** with one assertion broken locally, the exit code is not 0. Revert the break afterwards (FR-002).
- **Isolation:** with Docker stopped and the network disconnected, `dotnet test --solution Harmony.slnx --no-restore` passes (NFR-002).
- **Culture:** the suite passes under `Set-Culture en-US` and under `Set-Culture el-GR` (restore the original afterwards). `grep -rE "DateTime\.(Now|UtcNow)|DateTimeOffset\.Now|TimeZoneInfo\.Local" tests/` finds nothing (NFR-003).
- **Production code:** `git diff develop --stat -- src/` shows only `src/Directory.Packages.props` (NFR-007).
- **Traceability:** every FR and NFR in the Coverage table has a passing test tagged `[Trait("Requirement", "<ID>")]` or the named verification check.

## Spike results

To be filled in by T0: package versions and licences, MTP major version, coverage mechanism, NU1903 advisory URL, and the AutoMapper error count.

## Deviations log

Update this table in the same commit as any change that departs from the plan.

| Date | Task | What changed and why |
|---|---|---|
| 2026-11-26 | - | Spec FR-012 amended: the subtask marker is `SubTask` (`IssueTypesConstants.SUBTASK`, case-sensitive), not `Subtask`. The code and the seeded issue types use `SubTask`, so the original scenario would not have been filtered. Decided by chsakell. |
| 2026-11-26 | T1 | `NuGetAuditSuppress` for the AutoMapper 14.0.0 advisory in `tests/Directory.Build.props` only, so the test project adds 0 warnings (NFR-005). Decided by chsakell. |
| 2026-11-26 | T1 | Test packages added in this PR. This is an exception to the separate `build:` PR rule (spec Flagged concern 5, ADR decision 8). |
