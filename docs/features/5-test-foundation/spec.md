---
id: 5
title: Test foundation
artifact: spec
status: draft            # draft | approved | superseded
owner: chsakell
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:spec-approved label is added
created: 2026-11-26
intent: ./intent.md
---

# Spec: Test foundation

## Summary

Harmony gets its first automated tests. A shared test folder, one unit test project for the core Application layer, and a testing-strategy ADR give every later roadmap item a place to put its failing test first. The first tests:

- pin the behaviour of the core helpers and the object mappings;
- show the reference pattern for testing a request handler;
- run with one command from the repository root, with no Docker, database or network.

See [intent.md](./intent.md).

## Definitions

| Term | Meaning |
|---|---|
| **Test command** | The solution-wide command documented in AGENTS.md: `dotnet test --solution Harmony.slnx`, run from the repository root. |
| **Requirement tag** | A test trait named `Requirement` whose value is a requirement ID, for example `FR-006`. |
| **Unit test project** | `Harmony.Application.UnitTests`, the first and, for now, only test project. |
| **Test double** | A stand-in object (substitute) that replaces a real dependency in a test and records the calls made to it. |
| **Guard test** | A test that checks the test suite or configuration itself rather than product behaviour, for example "every test is tagged". |
| **Successful / failed result** | The result object every Application handler returns: `Succeeded` is true or false. |
| **Cobertura** | A widely supported XML format for code coverage reports. |
| **Microsoft.Testing.Platform** | The .NET test runner that replaces VSTest. `dotnet test` uses it when `global.json` opts in. |
| **Card helper** | The Application routine that extracts a card key from free text (used for GitHub branch, commit and pull request names). |
| **Card key** | The human-readable issue key in free text, for example `HAR-42`: 3 to 5 letters (any case), a hyphen, then one or more digits. It is returned exactly as written in the text. |
| **Allowed neighbour** | The characters that may come directly before or after a card key: space, `_`, `-`, `/`, or the start or end of the text. Every other character (including `.`, `,`, `#`, letters and digits) is not allowed. |
| **Card date display** | The Application routine that turns a card's start and due dates into short text for the UI and emails. The month is the three-letter abbreviation of the current culture, and the day is two digits, zero-padded. |
| **Issue-type filter** | The Application routine that removes subtask types from a list of issue types. A subtask type is one whose summary (its name) contains the word "Subtask". |
| **Query-filter builder** | The Application routine that combines two filter conditions into one with "and" or "or". |
| **Title-update handler** | The Application request handler that changes a card's title. |
| **Type map** | One source-to-destination mapping in the mapping configuration, for example `Card → CardDto`. |
| **Mapping with custom rules** | A type map in the Application layer whose profile defines at least one explicit member rule (`ForMember`). On 2026-11-26 there are six: <br>`Board → GetWorkspaceBoardResponse`, `Card → CardDto`, `Card → LoadCardResponse`, `CardLabel → LabelDto`, `Attachment → AttachmentDto` and `UserCard → CardMemberDto`. |
| **Unmapped-member baseline** | A checked-in list of (type map, destination member) pairs that the Application mapping configuration leaves unmapped today. On 2026-11-26 it has 28 members across 12 type maps. |
| **Known-wrong behaviour** | Current behaviour that is probably a defect. A test pins the current output, its method name ends with `_KnownIssue`, and Flagged concern 4 names it. It is not fixed in this feature. |

## User stories

### US1 - Run the tests with one command (P1)

As a contributor or AI agent, I want one command from the repository root that runs every test and tells me whether it passed, so that I can prove a change works before I open a pull request.

- **Why this priority:** without it nothing else in the roadmap can start test-first. Together with US2, P1 is a useful slice on its own.
- **Independent test:** in a clean clone with Docker stopped, run the test command and see the tests pass, a non-zero exit code when a test is broken on purpose, a filtered run by requirement tag, and a coverage file.

### US2 - Core helpers have pinned behaviour (P1)

As a maintainer, I want the core helper routines covered by tests, so that changes to card keys, date display, issue-type filtering and query-filter building are caught.

- **Why this priority:** these helpers are pure and used across services (card keys in GitHub integration, date display in emails). They prove the project setup with real assertions.
- **Independent test:** run the test command filtered to FR-006 to FR-014.

### US3 - Mapping safety net (P2)

As the engineer who will replace AutoMapper (R-14), I want the important mappings pinned by input and output examples, and new unmapped members to be caught, so that the mapper can be replaced without silent data loss.

- **Why this priority:** R-14 depends on it, but R-01 is useful without it.
- **Independent test:** run the test command filtered to FR-015 to FR-017.

### US4 - Reference pattern for handler tests (P2)

As a contributor starting R-05, R-15 or R-16, I want one complete example of a request handler tested with its dependencies replaced by test doubles, so that I copy a proven pattern instead of inventing one.

- **Why this priority:** improves consistency of later items; not needed to run tests.
- **Independent test:** run the test command filtered to FR-018 to FR-020.

## Functional requirements

| ID | Requirement | Story |
|---|---|---|
| FR-001 | When a contributor runs the test command, the test suite shall discover and run every test project in the solution and report the result of each test. | US1 |
| FR-002 | If any test fails, then the test command shall exit with a non-zero exit code. | US1 |
| FR-003 | When a contributor runs the test command with a requirement-tag filter, the test suite shall run only the tests that carry that requirement tag. | US1 |
| FR-004 | When the test command runs, with or without a filter and without extra coverage options, the test suite shall write a Cobertura code coverage report under `TestResults/` that covers the `Harmony.Application` assembly and excludes test assemblies. | US1 |
| FR-005 | If a test method has no requirement tag, either on the method or on its class, or the tag value does not match `FR-nnn` or `NFR-nnn`, then the test suite shall fail a guard test in that test project that names the method. | US1 |
| FR-006 | When a text contains one or more card keys with an allowed neighbour on both sides, the card helper shall return the first such card key. | US2 |
| FR-007 | If a text contains no card key with an allowed neighbour on both sides, then the card helper shall return no key. | US2 |
| FR-008 | When a card has a start date and a due date with the same month number, whatever their years, the card date display shall show `<month> <start day> - <due day>`. | US2 |
| FR-009 | When a card has a start date and a due date with different month numbers, the card date display shall show `<start month> <start day> - <due month> <due day>`, without years. | US2 |
| FR-010 | When a card has only one of start date or due date, the card date display shall show `<month> <day>` for that date. | US2 |
| FR-011 | When a card has neither a start date nor a due date, the card date display shall show an empty text. | US2 |
| FR-012 | When a list of issue types is filtered for normal issues, the issue-type filter shall return the list without the subtask types, and shall return no list when given no list. | US2 |
| FR-013 | When two conditions are combined with "and", the query-filter builder shall produce one condition with exactly one parameter that is true only when both are true, whatever parameter names the two conditions use. | US2 |
| FR-014 | When two conditions are combined with "or", the query-filter builder shall produce one condition with exactly one parameter that is true when at least one is true, whatever parameter names the two conditions use. | US2 |
| FR-015 | When a source object, with every input of the custom rules set to a distinct value, is mapped through each of the six mappings with custom rules, the Application mapping shall produce the destination values that the test states by hand for each rule. | US3 |
| FR-016 | If the Application mapping configuration has an unmapped (type map, destination member) pair that is not in the unmapped-member baseline, then the mapping guard test shall fail and name the type map and member. | US3 |
| FR-017 | If a member in the unmapped-member baseline is no longer unmapped, then the mapping guard test shall fail and ask for the entry to be removed. | US3 |
| FR-018 | While the current user has no user ID (not signed in), when a card title update is requested, the title-update handler shall return a failed result, shall not save the card and shall record no activity. | US4 |
| FR-019 | When a signed-in user updates a card title and the save changes at least one record, the title-update handler shall return a successful result. It shall also record one card activity of type "title updated", publish one search-index notification with the new title to the board's search index, and publish one real-time message of type "card title changed". | US4 |
| FR-020 | If saving the new card title changes no records, then the title-update handler shall return a failed result, record no activity, and publish no notification or message. | US4 |

### Acceptance scenarios

- **FR-001**
  - Given a clean clone and the .NET SDK from `global.json`, when the contributor runs `dotnet test --solution Harmony.slnx` from the root, then the unit test project runs and every test is reported as passed.
- **FR-002**
  - Given one assertion is deliberately broken locally, when the test command runs, then the process exit code is not 0.
  - Proven during verification, not by a committed failing test.
- **FR-003**
  - Given tests tagged FR-006 and FR-008, when the contributor runs the test command with `--filter-trait "Requirement=FR-006"`, then only the FR-006 tests run.
- **FR-004**
  - Given a plain `dotnet test --solution Harmony.slnx` run, then a Cobertura XML file exists under `TestResults/`. It lists the `Harmony.Application` assembly and no test assembly.
  - Given a run filtered with `--filter-trait`, then a Cobertura file is also produced.
  - No minimum percentage is enforced.
- **FR-005**
  - Given a test method with no `Requirement` trait on the method or its class, when the test command runs, then the guard test fails and its message names that method.
  - Given a test tagged `Requirement=TODO`, then the guard fails in the same way.
  - Given a class-level `Requirement=FR-006` trait and no method-level trait, then the guard passes for that method.
  - The committed suite has no untagged method, so the guard passes.
- **FR-006**
  - Given `"HAR-42 fix login"`, then `HAR-42`.
  - Given `"feature/HAR-42_login"`, then `HAR-42`.
  - Given `"fixes abc-7 and HAR-9"`, then `abc-7` (the first key wins, and matching ignores case).
- **FR-007**
  - Given `"no key here"`, then no key.
  - Given `"#HAR-42"` (`#` is not an allowed neighbour), then no key.
  - Given `"see HARMONY-12"` (7 letters: the last 5 match, but they are preceded by a letter), then no key.
  - Given `"HAR-42x"`, then no key.
  - Given `"AB-1"` (only 2 letters), then no key.
  - **Known-wrong (4f):** given `"Fixes HAR-42."`, then no key, because `.` is not an allowed neighbour.
  - **Known-wrong (4d):** given no text (null), then the helper throws.
- Unless a scenario says otherwise, the date scenarios (FR-008 to FR-011) run with the current culture set to the invariant culture.
- **FR-008**
  - Given start 2026-03-05 and due 2026-03-19, then `Mar 05 - 19`.
  - **Known-wrong (4a):** given start 2026-01-05 and due 2027-01-10, then `Jan 05 - 10`. The year is ignored.
  - **Known-wrong (4b):** given start 2026-03-05 and due 2026-03-19 with the current culture set to `el-GR`, then the month is the Greek abbreviation, not `Mar`.
- **FR-009**
  - Given start 2026-03-28 and due 2026-04-03, then `Mar 28 - Apr 03`.
- **FR-010**
  - Given only start 2026-03-05, then `Mar 05`.
  - Given only due 2026-04-03, then `Apr 03`.
- **FR-011**
  - Given neither date, then an empty text.
- **FR-012**
  - Given types "Story", "Bug", "Subtask", then "Story" and "Bug".
  - Given no list, then no list.
  - **Known-wrong (4c):** given a type with no summary, then the filter throws.
- **FR-013**
  - Given `x => x > 2` and `y => y < 5`, combined with "and", then the compiled condition matches 3 and does not match 1 or 6.
  - The combined condition has exactly one parameter.
- **FR-014**
  - Given `x => x < 2` and `y => y > 5`, combined with "or", then the compiled condition matches 1 and 6 and does not match 3.
  - The combined condition has exactly one parameter.
- **FR-015**
  - For each of the six mappings with custom rules, given a source with every rule input set to a distinct value, then each destination member that has a rule equals the value written in the test.
  - Example: for `Card → CardDto`, given 3 check-list items of which 1 is checked, and 2 attachments, then the total items is 3, the completed items is 1 and the total attachments is 2.
- **FR-016**
  - Given the configuration on 2026-11-26 and the baseline of 28 members across 12 type maps, the guard passes.
  - Given a new destination member with no mapping, the guard fails and names it.
- **FR-017**
  - Given a baseline entry whose member becomes mapped, the guard fails and names the stale entry.
- **FR-018**
  - Given the current user has no user ID, when the handler runs, then:
    - the result is failed;
    - the card is never saved;
    - no activity is recorded.
- **FR-019**
  - Given a signed-in user and a save that reports 1 changed record, when the handler runs, then:
    - the result is successful;
    - one "title updated" activity is recorded for the card;
    - one search-index notification carrying the new title is published to the board's search index;
    - one real-time "card title changed" message is published.
- **FR-020**
  - Given a signed-in user and a save that reports 0 changed records, when the handler runs, then the result is failed, no activity is recorded and nothing is published.

## Non-functional requirements

| ID | Category | Requirement (measurable) |
|---|---|---|
| NFR-001 | Performance | The unit test project's test execution (excluding build) completes in under 30 seconds on a developer machine. Verified by the duration the test runner reports. |
| NFR-002 | Isolation | After packages are restored, the test command passes with Docker stopped and the network disconnected (`--no-restore`). The unit test project references no Persistence, Infrastructure, Messaging, Caching or service project. Verified by one run under those conditions during `/sdlc-verify` and by the project's references. |
| NFR-003 | Determinism | Test results do not depend on the machine's culture, time zone or clock. Tests that format dates set the current culture explicitly. No test reads the current time: date inputs are fixed values. Verified by running the suite with the machine culture set to `en-US` and to `el-GR`, with the same results both times. Also verified by a search showing that no test file uses `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now` or `TimeZoneInfo.Local`. |
| NFR-004 | Licensing | Every package added is under an OSI-approved licence (MIT, Apache-2.0 or BSD). No commercial or source-available test library is used. Verified in review against each package's licence. |
| NFR-005 | Build hygiene | `dotnet build Harmony.slnx` reports 0 errors and no more than 669 warnings. The new test project contributes 0 warnings. `dotnet format --verify-no-changes` passes for every changed file. |
| NFR-006 | Maintainability | Test package versions are declared once, in central package management; test project files have no `Version` attributes. Settings shared by test projects live in one `tests/Directory.Build.props`. |
| NFR-007 | Behaviour preservation | No production behaviour changes. `git diff develop -- src/` shows only the central package entries and any testability changes listed and justified in `plan.md`. Verified during `/sdlc-verify`. |

### Deliverables (verified in review, not by tests)

| ID | Deliverable |
|---|---|
| DEL-1 | `docs/adr/2026-11-26-testing-strategy.md`, with `status: accepted` before `sdlc:plan-approved`. It records the runner, frameworks, assertion and coverage libraries, test project layout and naming, the requirement-tag rule, the `_KnownIssue` convention and the package-PR exception. It is also listed in the overview's decision index. |
| DEL-2 | AGENTS.md documents the test command, the requirement filter example and where the coverage report is written. During `/sdlc-verify` the documented commands are run exactly as written and succeed. The overview's Known issues entry "No automated tests" is updated. |
| DEL-3 | Every known-wrong behaviour (Flagged concern 4) is recorded where the owner decides, and each recording points to the test that pins it. |

## Design

### Components affected

See [docs/architecture/overview.md](../../architecture/overview.md). No runtime component changes.

- **New `tests/` folder:**
  - `tests/Directory.Build.props` holds the settings shared by test projects (for example, not packable, test-runner opt-in). It also imports `src/Directory.Build.props`, so the target framework stays defined once.
  - `tests/Directory.Packages.props` imports `src/Directory.Packages.props`, so central package management reaches `tests/` (Flagged concern 2).
- **New `Harmony.Application.UnitTests`:**
  - references `Harmony.Application` only (Domain and Shared come through it);
  - is added to `Harmony.slnx` under a `/tests/` solution folder.
- **`global.json`:** opts the solution into Microsoft.Testing.Platform, so `dotnet test --solution` and `--filter-trait` work.
- **Central package versions:**
  - the test framework (xUnit v3 on Microsoft.Testing.Platform), NSubstitute, Shouldly and coverlet.MTP for coverage (all decided in the ADR);
  - added in this PR as a logged exception to the separate-`build:`-PR rule, decided 2026-11-26.
- **AGENTS.md:** the Test command line is updated, and so is the overview's Known issues entry "No automated tests".
- **Production code:**
  - unchanged by default;
  - any change made only for testability (for example, internals made visible to the test project) must be listed and justified in `plan.md`.

The testing strategy is recorded in the ADR [2026-11-26-testing-strategy](../../adr/2026-11-26-testing-strategy.md) (status: proposed). It covers:
- the runner, frameworks and assertion and coverage libraries;
- the test project layout and naming for later items (`*.UnitTests`, `*.IntegrationTests` for R-03 and R-04, `Harmony.ArchitectureTests` for R-06);
- the requirement-tag convention;
- the known-wrong naming convention.

### API contract

Not applicable. No endpoint, gRPC or message contract changes.

### Data model

Not applicable. No entity, schema or migration changes.

### Events and integrations

Not applicable. The handler tests verify, with test doubles, that publishing calls happen. No broker is used.

### UX notes

Not applicable.

## Standards and policies applied

- **dotnet-instructions:** not installed in this repository (AGENTS.md, Standards). The spec follows the surrounding conventions instead: central package management, the `src/Directory.Build.props` target framework, and Conventional Commits.
- **AGENTS.md commands:**
  - the build warning baseline (669) and format-changed-files-only rule became NFR-005;
  - "never skip or delete a failing test" became the known-wrong convention: current behaviour is pinned, not skipped.
- **SDLC kit, test tagging:** every test carries `[Trait("Requirement", ...)]`. FR-005 makes this enforced, not advisory. The `--filter-trait` command in the verify skill drove the choice of Microsoft.Testing.Platform (FR-003).
- **SDLC kit, hot spots:** the rule that package additions go in a separate `build:` PR was waived for this PR. The decision is logged here and in the plan.
- **Intent constraints:** open-source licences only became NFR-004; no behaviour change became the production-code rule under Design.

## Flagged concerns

| # | Concern | Owner | Resolution |
|---|---|---|---|
| 1 | The AutoMapper configuration is invalid today. `AssertConfigurationIsValid()` reports 12 type maps with unmapped destination members (checked 2026-11-26 with a scratch program against `Harmony.Application`), so a plain validity test would fail. | chsakell | **Resolved 2026-11-26:** use behaviour pairs (FR-015) plus a ratcheting baseline (FR-016, FR-017). No profile changes in R-01. |
| 2 | Central package management lives in `src/Directory.Packages.props`. MSBuild looks for it by walking up from the project folder, so a project under `tests/` will not find it. | chsakell | **Resolved 2026-11-26:** `tests/Directory.Packages.props` and `tests/Directory.Build.props` import their `src/` counterparts, so there is one version list and no file moves. Recorded in the testing-strategy ADR. |
| 3 | Opting into Microsoft.Testing.Platform changes the documented test command to `dotnet test --solution Harmony.slnx`. Every future test project (R-03, R-04, R-06) must also run on that platform, and CI (R-02) must use the new command. | chsakell | **Resolved 2026-11-26:** adopt Microsoft.Testing.Platform. Recorded in the testing-strategy ADR. |
| 4 | Probable defects found while specifying, pinned as known-wrong and not fixed here: <br>(a) date display ignores the year when the months match; <br>(b) date display uses the server's culture for month names, which affects emails sent by Notifications; <br>(c) the issue-type filter throws on a type with no summary; <br>(d) the card helper throws on empty (null) text; <br>(e) the `CardDto → CardCreatedMessage` mapping is never used, because both handlers construct that message directly; <br>(f) the card helper finds no key when it is followed by `.` or `,`, for example the commit message "Fixes HAR-42.". | chsakell | **Resolved 2026-11-26:** recorded under "Known defects found by R-01" in the [roadmap's Later section](../../roadmap.md#later-not-in-the-30) (DEL-3). (e) is also input for R-14, and (f) matters for the GitHub integration. |
| 5 | Adding test packages in this PR departs from the SDLC guide's separate-`build:`-PR rule. | chsakell | **Resolved 2026-11-26:** accepted exception. Logged in `plan.md` and the ADR. |

## Open questions

| Question (from intent or new) | Answer, or "carried forward" | Owner |
|---|---|---|
| Microsoft.Testing.Platform or VSTest? | Microsoft.Testing.Platform. Decided 2026-11-26 because it supports the SDLC kit's `--solution` and `--filter-trait` commands. | chsakell |
| Assertion library? | Shouldly (BSD-3-Clause). Decided 2026-11-26. | chsakell |
| Coverage tool? | coverlet.MTP (MIT), decided in the testing-strategy ADR, since coverlet.collector works only with VSTest. The plan checks that it works with the chosen xUnit v3 package and SDK 10.0.404. | chsakell |
| Which handler is the reference example? | The card title update handler. It depends only on interfaces, covers the success, unauthenticated and nothing-saved paths, and is changed later by R-08 (authorization) and R-24 (missing card returns not-found). The missing-card crash is left to R-24 and not pinned here. | chsakell |
| Can mapping tests survive the R-14 mapper swap? | Yes for FR-015, which asserts on inputs and outputs only. FR-016 and FR-017 are AutoMapper-specific and are expected to be removed or replaced by R-14. | chsakell |
| Separate `build:` PR for packages? | No. Accepted exception, decided 2026-11-26. | chsakell |
| Exact package versions, and xUnit v3 Microsoft.Testing.Platform v1 or v2 compatibility with SDK 10.0.404 | Carried forward to the plan. Checked by a spike before the first task. | chsakell |

## Out of scope

- CI workflow (R-02); integration tests with containers (R-03); API host and authorization matrix (R-04); Scrum and automation characterization (R-05); architecture rule tests (R-06).
- A minimum coverage threshold.
- Fixing any known-wrong behaviour (Flagged concern 4) or the 12 unmapped members.
- Tests for Application code that runs EF Core async operators directly on queries. Those belong in integration tests (R-03).

## Rollout

- No runtime impact. Nothing is deployed and no service behaviour changes.
- After merge, contributors and agents use `dotnet test --solution Harmony.slnx`. AGENTS.md is updated in the same PR, and R-02 (CI) builds on it.
- **Rollback:** revert the merge commit. `global.json`, the central package entries and the `tests/` folder go back together.
