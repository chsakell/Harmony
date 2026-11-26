---
id: 19
title: Integration test harness
artifact: spec
status: draft            # draft | approved | superseded
owner: chsakell
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:spec-approved label is added
created: 2026-11-26
intent: ./intent.md
---

# Spec: Integration test harness

## Summary

Harmony gets a second kind of test: tests that run against real SQL Server, PostgreSQL and MongoDB instances. Testcontainers starts and removes these instances for each run. The new tests:

- prove that every database upgrade applies on both relational providers and matches the current data model;
- prove the main board and card queries on both providers, including the fixes for #11 (backlog reorder) and #17 (card search);
- cover the MongoDB stores for automation rules and GitHub activity.

The everyday test command stays Docker-free. A separate, required CI check runs the integration tests on every pull request. See [intent.md](./intent.md).

## Definitions

| Term | Meaning |
|---|---|
| **Test command** | `dotnet test --solution Harmony.slnx` from the repository root, as documented in AGENTS.md. It needs no Docker. |
| **Integration test command** | The one documented command that runs the integration tests, and only them. The plan chooses its exact form (see Design). |
| **Integration test project** | `tests/Harmony.Infrastructure.IntegrationTests`, named by the [testing strategy ADR](../../adr/2026-11-26-testing-strategy.md), rule 6. |
| **Provider** | One of the two relational databases Harmony supports: SQL Server or PostgreSQL ([ADR](../../adr/2026-11-25-sql-server-and-postgresql.md)). MongoDB is not a provider in this sense. |
| **Engine** | A database server product the run starts: SQL Server, PostgreSQL or MongoDB. |
| **Relational test** | An integration test that uses a provider, as opposed to a MongoDB test. |
| **Disposable instance** | A database server container that the integration test run starts, uses and removes. It has a random host port and no named volume. The Testcontainers clean-up container (Ryuk), which removes the instances if the process is killed, is not counted as one. |
| **Docker available** | A Docker daemon that runs Linux containers can be reached from the test process. It is unavailable when Docker is not installed, is stopped, or Docker Desktop is in Windows-containers mode. |
| **Empty database** | A database that exists on a disposable instance but has no tables, created for one migration test case. |
| **Fresh database** | A database newly created on a disposable instance for one test class, with all of that provider's migrations applied and no other data. |
| **Test data rule** | Every relational test creates its own users, workspace and boards, with unique IDs and keys, and asserts only on them. Tests in the same class share a fresh database, never their data. |
| **Context** | One of the three EF Core data models that have migrations: `HarmonyContext` (the Harmony database), `AutomationContext` (the Automations jobs database) and `NotificationContext` (the Notifications jobs database). |
| **Pending model change** | A difference between a context's current model and the last migration snapshot of a provider: a schema change that has no migration for that provider. |
| **Hidden subtask** | A card with a parent card. The card query filter (`CardConfiguration`) hides it from every card query that does not opt out of filters. |
| **Inactive workspace** | A workspace whose status is not `Active`. The workspace query filter hides it. |
| **Active card** | A card with status `Active`. The other statuses are `Backlog`, `Archived` and the rest of the `CardStatus` values. |
| **Known-wrong behaviour** | Current behaviour that is probably a defect. A test pins today's outcome, its method name ends with `_KnownIssue`, and Flagged concern 5 lists it. It is not fixed in this feature (testing ADR, rule 7). |
| **Requirement tag** | A `Requirement` trait with an `FR-nnn` or `NFR-nnn` value, enforced by the shared guard ([traceability ADR](../../adr/2026-11-26-test-traceability-tags.md)). A `Bug` trait with an issue number is also valid. IDs are unique only within a feature: in the integration test project they refer to this spec, and the integration test command runs only that project, so `Requirement=FR-010` selects this feature's tests. |
| **Card terms** | A **backlog card** has status `Backlog` and no list; its **backlog position** is numbered per board. A **top-level card** has no parent card. A card's **serial number** is the number in its key (for example 42 in `HAR-42`). **Checklist totals** are the number of items and of checked items per checklist. The **child count** is the number of active subtasks (FR-035). |
| **Board access terms** | A board's visibility is **private**, **workspace** (workspace-visible) or **public**. A **board member** has a board membership; a **workspace member** has a workspace membership. **The user's board list** is the result of `BoardService.GetUserBoards`. |
| **Workspace card search** | `CardService.SearchWorkspaceCards` with its count, `CountWorkspaceCards`. The **term** is matched against card titles, and an empty term matches every title. The **skipped card** is an optional card ID to leave out. Pages start at 1. |
| **Automation store** and **source-control store** | The MongoDB repositories `AutomationRepository` (automation rules and their **templates**, which are grouped by notification type) and `SourceControlRepository` (repositories, branches, pushes, tags and pull requests). |
| **Test summary** | The Markdown test report that the run writes and the CI job appends to the GitHub run summary, as the `Build and test` job already does. |

## User stories

### US1 - Run tests against real databases on purpose (P1)

As a contributor or AI agent with Docker, I want one command that starts real database instances, runs the integration tests and cleans up, so that I can prove a data-layer change on both providers. The everyday test command must stay unchanged.

- **Why this priority:** every other story needs it. With US2 it is a useful slice on its own.
- **Independent test:** with Docker running, run the integration test command and see tests pass on both providers and no containers left behind. With Docker stopped, run the test command and see it pass. Then run the integration test command and see it fail with a clear message.

### US2 - Database upgrades proven on both providers (P1)

As a self-hosting team, I want every database upgrade applied in a test on both providers, so that an upgrade that breaks on my database is caught before release.

- **Why this priority:** a broken upgrade blocks a whole installation. This is the cheapest and widest check.
- **Independent test:** run the integration test command filtered to FR-010 and FR-011.

### US3 - Board and card queries proven on both providers (P1)

As a maintainer, I want the most-used and most fragile queries run on both providers, so that query bugs and differences between providers are found by tests, not by users.

- **Why this priority:** #11 and #17 were proven only in memory. R-22 and R-23 need this harness for their first failing test.
- **Independent test:** run the integration test command filtered to FR-020 to FR-042.

### US4 - MongoDB storage proven (P2)

As a maintainer, I want the automation-rule and GitHub-activity stores tested against a real MongoDB, so that changes to them are caught.

- **Why this priority:** in scope, but it covers fewer user-facing paths than the relational stories.
- **Independent test:** run the integration test command filtered to FR-050 and FR-051.

### US5 - Every pull request runs the integration tests (P2)

As a reviewer, I want a separate CI check that runs the integration tests, so that no pull request can merge with a broken data layer.

- **Why this priority:** it makes US1 to US4 permanent, but they are useful locally without it.
- **Independent test:** open a pull request and see the `Integration tests` check run, pass, and publish its results.

## Functional requirements

| ID | Requirement | Story |
|---|---|---|
| FR-001 | When a contributor runs the integration test command with Docker available, the integration test project shall start at most one disposable instance of each engine the selected tests need, run the selected tests, and remove those instances when the run ends, whether tests passed or failed. | US1 |
| FR-002 | When a contributor runs the test command, the test suite shall run none of the integration tests and shall pass without Docker. The solution build shall still compile the integration test project. | US1 |
| FR-003 | If Docker is not available when the integration test command runs, then the run shall exit with a non-zero exit code and report the selected tests as failed, with a message that contains `Docker`. It shall not report them as passed or skipped. | US1 |
| FR-004 | If the integration test command selects no tests, then the run shall exit with a non-zero exit code. | US1 |
| FR-005 | The integration test project shall run each relational test case once per provider, and each test result's display name shall contain `SqlServer` or `PostgreSql`. | US1 |
| FR-006 | The integration test project shall give each relational test class, except the migration tests, its own fresh database per provider, and every relational test shall follow the test data rule. | US1 |
| FR-007 | If an integration test method has no valid `Requirement` or `Bug` tag, then the shared tag guard in the integration test project shall fail and name the method. | US1 |
| FR-010 | When a context's migrations for a provider are applied to an empty database, the migration shall complete without error, and the database's applied migrations shall equal that provider's migrations for the context, in order. This holds for each of the three contexts on both providers. | US2 |
| FR-011 | If a context has a pending model change for a provider, then the migration check for that context and provider shall fail, with the context and provider in its display name. A pending change that exists on 2026-11-26 is pinned by a `_KnownIssue` test instead, and any other pending change still fails. | US2 |
| FR-020 | While a card is a hidden subtask, the card repository's list counts (`CountCards`, `CountActiveCards`) and the board load (FR-030) shall leave it out, and the card repository's single-card reads (`Get`, `Load`) shall return it. | US3 |
| FR-021 | While a workspace is inactive, the user's board list shall not include its boards, even boards the user is a member of. This rule wins over FR-032. | US3 |
| FR-030 | When a board is loaded, each of its lists shall hold that list's active top-level cards, each with its labels, member IDs, checklist totals, child count, and comment, link and attachment counts. When a sprint, issue types or assignees are given, a card shall be included only if it matches every given filter; it matches a filter when it matches any of that filter's values. When a cards-per-list limit N is given, each list shall hold at most N cards. The order of cards within a list is not specified. | US3 |
| FR-031 | When board access is checked for a user, the board service shall grant access to a private board only to its board members, to a workspace-visible board only to its workspace members, and to a public board to anyone. If the board does not exist, then it shall refuse access. | US3 |
| FR-032 | When a user's boards are listed, the board service shall return, once each, the workspace-visible and public boards of the user's workspaces and the private and public boards the user is a member of. When a workspace is given, it shall return only that workspace's boards. | US3 |
| FR-033 | When the next card serial number is requested for a board, the card repository shall return the count of every card on that board, plus one. The count includes cards in lists, backlog cards, subtasks and archived cards. This pins today's behaviour (Flagged concern 5e). | US3 |
| FR-034 | When the highest active position in a list is requested, the card repository shall return the highest position among the list's active cards that are not subtasks, or -1 when there are none. When the next backlog position of a board is requested, it shall return the highest backlog position on that board plus one, or 1 when the backlog is empty. | US3 |
| FR-035 | When child counts are requested for one card or a set of cards, the card repository shall count only active subtasks of each card. In the set form, cards without active subtasks shall be absent from the result. | US3 |
| FR-040 | When a backlog card is moved from position `old` to position `new`, the card service shall give it position `new` and shift by one, towards `old`, only the backlog cards of the same board in positions [`new`, `old`) when moving up, or (`old`, `new`] when moving down. Cards on other boards, and cards that are not in the backlog, shall keep their positions. | US3 |
| FR-041 | When a workspace card search runs, the card service shall return and count the same cards: the cards of that workspace, including subtasks, that are not archived and whose title contains the term, without the skipped card when one is given, ordered by position and paged. Test data uses distinct positions, because the order of equal positions is not specified. Terms with the wildcard characters `%` or `_` are out of scope. | US3 |
| FR-042 | When a workspace card search runs with a term that differs from a card title only in the case of ASCII letters, the card service shall still match the card, on both providers, with each image's default collation. | US3 |
| FR-050 | When automation rules are created, replaced (the whole rule document), enabled or disabled, and removed, the automation store shall return the stored rules by type and board as they were last saved. Templates shall be listed in full and by notification type. | US4 |
| FR-051 | When repositories and branches are recorded, the source-control store shall return a repository by its ID and a branch by its name and repository ID. A branch search and its total shall agree. A push recorded on a branch shall make that branch findable by any of the push's commit IDs. A tag added to a branch shall be stored on it. Recording a pull request with the same pull request ID a second time shall update it, not add a copy. Deleting a branch shall remove it. | US4 |
| FR-060 | When the CI workflow runs (on the same triggers as the `Build and test` job: pull requests to `develop`, `main` and `release/**`, and pushes to `develop` and `main`), CI shall run a separate `Integration tests` job that runs the integration test command and fails if any test fails, if no test ran, or if Docker is unavailable. | US5 |
| FR-061 | When the `Integration tests` job finishes, whatever its outcome, it shall publish a test summary to the run and upload the test results and coverage report as an artifact kept for 14 days. | US5 |

### Acceptance scenarios

Unless a scenario says otherwise, relational scenarios run on both providers against a fresh database. Test data is created by the test itself.

- **FR-001**
  - Given Docker available and no test containers present, when the integration test command runs, then all integration tests run and pass. About a minute after the run, `docker ps -a` lists no disposable instance the run started (the Ryuk container removes itself).
  - Given a filter that selects only MongoDB tests, then no SQL Server or PostgreSQL instance is started.
  - Given a run stopped with Ctrl+C, then the instances are removed by Testcontainers' clean-up container within a minute. This is checked once during verification.
  - Given one integration test deliberately broken locally, when the command runs, then the exit code is not 0 and no container the run started is left. This is proven during verification, not by a committed failing test.
- **FR-002**
  - Given Docker stopped, when the test command runs, then it exits with code 0, and its summary lists no test from the integration test project.
  - Given `dotnet build Harmony.slnx`, then the output includes the integration test project.
- **FR-003**
  - Given Docker stopped, when the integration test command runs, then the exit code is not 0 and the output says Docker is required. No test is reported as passed or skipped.
- **FR-004**
  - Given the integration test command with a filter that matches no test, then the exit code is not 0.
- **FR-005**
  - Given a relational test case, when the run's results are listed, then that case appears once with `SqlServer` and once with `PostgreSql` in its display name.
- **FR-006**
  - Given two test classes that each insert a board with the same key, when both run at the same time, then neither sees the other's board.
- **FR-007**
  - The guard test from `tests/Shared/` runs in the integration test project under the integration test command, and therefore in CI (FR-060). The committed suite has no untagged method.
  - Given an untagged test method added locally, then the guard fails and names it. This is proven during verification, not committed.
- **FR-010**
  - Given an empty database, when `HarmonyContext`'s migrations are applied, then the call succeeds, and the list of applied migrations equals the provider's migration list for that context, in order. The same holds for `AutomationContext` and `NotificationContext`, on SQL Server and on PostgreSQL (six cases).
- **FR-011**
  - Given today's models and migrations, then no context has a pending model change on either provider. If one does, the test is pinned as known-wrong and recorded (Flagged concern 5).
  - Given a local change that adds a property to an entity without a migration, then the check fails for that context on both providers. This is proven during verification, not committed.
- **FR-020**
  - Given a list with one card and one subtask of that card, when the list's cards are counted (`CountCards`), then 1.
  - Given the same data, when the subtask is read with `Get` or `Load`, then it is returned.
- **FR-021**
  - Given a user who is a member of an active and an inactive workspace, each with one workspace-visible board, when the user's boards are listed with no workspace, then only the active workspace's board is returned.
- **FR-030**
  - Given a board with two lists, active cards, one archived card, one subtask, labels, a member, a checklist with 3 items (1 checked), one comment, one link and one attachment on a card, when the board is loaded, then each list holds only its active top-level cards. That card shows a child count of 1, 1 comment, 1 link and 1 attachment. Its checklist shows 3 items with 1 checked, and its labels and member ID are present.
  - Given cards matching only the sprint filter, only the issue-type filter, and both, when the board is loaded with both filters, then only the card matching both is returned.
  - Given cards in two sprints, when the board is loaded for one sprint, then only that sprint's cards are returned. The same holds for an issue-type filter and an assignee filter.
  - **Known-wrong (expected):** given a list with 5 active cards, when the board is loaded with at most 2 cards per list, then all 5 are returned. The limit is not applied today (read from the code on 2026-11-26; related to R-22).
- **FR-031**
  - Given a private board, then its member is granted access and a workspace member who is not a board member is refused.
  - Given a workspace-visible board, then a workspace member is granted access and a user outside the workspace is refused.
  - Given a public board, then a user with no membership is granted access.
  - **Known-wrong (expected):** given a board ID that does not exist, then the check throws instead of refusing access (read from the code on 2026-11-26; related to R-08).
- **FR-032**
  - Given a user who is a workspace member and also a member of a private board and a public board in that workspace, when the user's boards are listed, then each of those boards and the workspace's workspace-visible boards appear exactly once. A private board the user is not a member of does not appear.
  - Given two workspaces, when the list is requested for one of them, then only that workspace's boards appear.
- **FR-033**
  - Given a board with 2 cards in lists, 1 backlog card, 1 subtask and 1 archived card, then the next serial number is 6. A card on another board does not change it.
- **FR-034**
  - Given an empty list, then the highest active position is -1.
  - Given active cards at positions 0, 1 and 4 and an archived card at position 7, then it is 4.
  - Given an empty backlog, then the next backlog position is 1.
  - Given backlog positions 1, 2 and 5 on the board and 9 on another board, then it is 6.
- **FR-035**
  - Given card A with 2 active subtasks and 1 archived subtask, and card B with none, then A's count is 2. The set form returns `{A: 2}` with no entry for B.
- **FR-040**
  - Given backlog cards at positions 1 to 4 on board X and 1 to 4 on board Y, when X's card at position 4 is moved to position 2, then X's moved card is at 2, and the cards that were at 2 and 3 are now at 3 and 4. All of Y's cards keep their positions. This is the #11 case proven against real SQL.
  - Given the reverse move (2 to 4), then X's cards at 3 and 4 move to 2 and 3.
- **FR-041**
  - Given cards in two workspaces with matching titles, one archived card and one subtask, when workspace W is searched with no skipped card, then W's non-archived matching cards are returned in position order, including the subtask, and the count equals the number returned. This is the #17 case.
  - Given the same data and a skipped card, then the results are the same minus that card.
  - Given 3 matches and a page size of 2, then page 1 has 2 cards, page 2 has 1, and the count is 3.
- **FR-042**
  - Given a card titled `Fix Login` and the term `login`, then the card is found on SQL Server.
  - **Known-wrong (expected):** on PostgreSQL the card is not found, because PostgreSQL compares text case-sensitively by default. Until a later roadmap item fixes it, FR-042 on PostgreSQL is covered by a `_KnownIssue` test that asserts "not found". That item changes the test on purpose.
- **FR-050**
  - Given a rule created for board B, when the rules of its type for B are read, then it is returned. After replace, the new values are returned. After disable, `enabled` is false. After remove, it is not returned. A rule for another board is never returned.
  - Given 3 templates for 2 notification types, then all 3 are listed, and filtering by one type returns only its templates.
- **FR-051**
  - Given a repository and two branches, then each is read back by its ID or by name and repository. Branch existence is true for them and false for an unknown name.
  - Given a branch search term that matches 1 of 2 branches, then the search and the total both give 1.
  - Given a push with a commit on a branch, then that branch is found by the commit ID. After a tag is added, the branch carries it.
  - Given a pull request recorded twice with a changed title, then one pull request with the new title is stored.
  - Given a deleted branch, then it is no longer found.
- **FR-060**
  - Given this feature's pull request, then the `Integration tests` check appears next to `Build and test` and passes.
  - Before this pull request merges, branch protection on `develop` and `main` lists `Integration tests` as a required check (Rollout, step 3).
- **FR-061**
  - Given any finished run of the job, then the run summary shows the test results, and an artifact holds the test result files and the Cobertura report.

## Non-functional requirements

| ID | Category | Requirement (measurable) |
|---|---|---|
| NFR-001 | Performance | Each of the three runs in NFR-004 finishes the whole `Integration tests` job in at most 10 minutes on `ubuntu-latest`. That includes checkout, restore, build and image pulls. The job timeout is 20 minutes. |
| NFR-002 | Isolation | The run never uses a fixed host port, a named volume, or a container or volume name that starts with `harmony-`. A developer's Aspire containers and data are never read, changed or stopped. Verified once: with the AppHost's containers running, run the integration test command, then check that those containers are still running with the same IDs and that their volumes are unchanged. |
| NFR-003 | Isolation | No test depends on another test's data or on test order. Relational tests follow the test data rule. MongoDB tests share one instance, and the stores use fixed database names, so they run one at a time and drop the automation and source-control databases before each test. |
| NFR-004 | Reliability | The `Integration tests` job passes 3 runs in a row on this feature's pull request without code changes. Failed tests are never rerun automatically. The connection settings the hosts use are allowed; on 2026-11-26 no host enables EF Core connection retries. |
| NFR-005 | Fidelity | Each provider is configured exactly as its host configures it in `ServiceCollectionExtensions`: Api for `HarmonyContext`, Automations for `AutomationContext`, Notifications for `NotificationContext`. On 2026-11-26 the three hosts set the same two things: the context's migrations assembly (`Harmony.Persistence.Migrations.SqlServer` or `.PostgreSql`), and `Npgsql.EnableLegacyTimestampBehavior` on for PostgreSQL ([ADR](../../adr/2026-11-25-sql-server-and-postgresql.md)). Tests set no collation; each image's server default applies. |
| NFR-006 | Fidelity | The image tags are defined once in the test project: `mcr.microsoft.com/mssql/server:2022-latest`, `postgres:17-alpine` and `mongo:8.3`, the tags `docker-compose*.yml` and `.k8s/` use on 2026-11-26. A test fails if they differ from the tags in `docker-compose.yml` and `docker-compose.postgres.yml`. |
| NFR-007 | Build | `dotnet build Harmony.slnx` reports 0 errors and no more than 585 code warnings. The test project builds with warnings as errors (inherited from `tests/Directory.Build.props`). `dotnet format --verify-no-changes` passes for changed files. |
| NFR-008 | Licences | Every new package has an OSI licence. Testcontainers and its modules are MIT. The SQL Server image is not open source: it is a test-time dependency only, already used by the deployment files, and is not shipped. |
| NFR-009 | Coverage | Every integration run writes a Cobertura report under `TestResults/Harmony.Infrastructure.IntegrationTests/`, as the shared test settings already do. No threshold. |
| NFR-010 | Documentation | AGENTS.md (Commands) documents the integration test command, that it needs Docker, and how to filter it by requirement. `docs/architecture/overview.md` (Known issues) no longer says that integration tests against the databases are missing, and lists each known-wrong behaviour the integration tests pin. |
| NFR-011 | Portability | The integration test command passes once on Docker Desktop for Windows (Linux containers) during verification, as well as in CI. On Arm64 machines the SQL Server tests may fail, because there is no Arm64 image; this is accepted (Flagged concern 6), and they are not excluded. |

## Design

### Components affected

See [overview](../../architecture/overview.md).

- **New:** `tests/Harmony.Infrastructure.IntegrationTests`. It references `Harmony.Infrastructure` and both migrations projects. It uses xUnit v3 on Microsoft.Testing.Platform, Shouldly, coverlet.MTP and the Testcontainers modules for SQL Server, PostgreSQL and MongoDB.
- **Harness:**
  - One disposable instance per engine for the whole run, shared through an assembly-level fixture.
  - Each relational test class creates a fresh database with a unique name and applies migrations with `Database.Migrate()` (FR-006).
  - Relational tests are parameterized by provider (FR-005).
  - Services under test (`BoardService`, `CardService`) are built from the real repositories over the real context. Their non-database dependencies get in-memory or substitute stand-ins: cache, card summary cache, mediator and endpoint options. AutoMapper uses the real Application configuration.
- **Opt-in (FR-002 to FR-004).** The project is in `Harmony.slnx` so the build compiles it, but the test command must not run it. The plan picks the mechanism after a short trial on SDK 10.0.404 and states it in AGENTS.md. Candidates:
  - an MSBuild property that marks the project as a test project only when integration tests are requested;
  - a solution filter, or a `--project` command, used only by the integration test command.
  
  A default trait filter is ruled out: the project would then run zero tests under the test command, which exits with code 8 and breaks FR-002. The choice must keep `--filter-trait "Requirement=..."` working for the integration tests. It must not need `--ignore-exit-code 8` on the integration test command (FR-004), and must not change how the test command is written today.
- **Provider setup (NFR-005).** Today each host configures its provider in its own `ServiceCollectionExtensions`. The plan either reuses one shared, behaviour-neutral helper or copies the two settings into the harness with a test that keeps them equal (Flagged concern 2).
- **Production code:** no behaviour change. Changes are allowed only for testability and justified in the plan.
- **CI (`.github/workflows/ci.yml`):** a second job, `Integration tests`, on `ubuntu-latest`, which has Docker. It restores, builds, runs the integration test command, publishes the summary and uploads results the same way as `Build and test`. It does not repeat the warning or format checks. It runs in parallel with `Build and test`.
- **Docs:** AGENTS.md (Commands), `docs/architecture/overview.md` (Known issues, CI/CD), and `docs/roadmap.md`. The roadmap gets the R-03 status and the known-wrong behaviours found, added to the matching items' notes (Flagged concern 5).

### API contract

Not applicable: no endpoint changes.

### Data model

No change. The tests read the existing migrations of the three contexts on both providers. A pending model change found by FR-011 is pinned and recorded, not fixed here.

### Events and integrations

None. The message broker, cache server, search index, email and gRPC are out of scope. The services under test get stand-ins for them.

### UX notes

Not applicable.

## Standards and policies applied

- **No `dotnet-instructions` standard is installed** (AGENTS.md), so the spec follows the ADRs and the conventions of the existing test projects.
- **[Testing strategy ADR](../../adr/2026-11-26-testing-strategy.md):**
  - xUnit v3 on Microsoft.Testing.Platform, Shouldly and coverlet.MTP (rules 1 to 5).
  - The project name `Harmony.Infrastructure.IntegrationTests` (rule 6).
  - `_KnownIssue` pinning instead of skipping (rule 7).
  - Testcontainers packages go in a separate `build:` pull request (rule 8, Flagged concern 1).
  - Testcontainers is already named for integration tests, so no new ADR is needed.
- **[Traceability ADR](../../adr/2026-11-26-test-traceability-tags.md):** every test carries a tag. The shared guard runs in the new project (FR-007).
- **[SQL Server and PostgreSQL ADR](../../adr/2026-11-25-sql-server-and-postgresql.md):**
  - Both providers are tested.
  - The ADR's rule that every schema change adds a migration to both projects becomes enforceable (FR-011).
  - The legacy timestamp switch is honoured (NFR-005).
- **AGENTS.md:** the warning baseline and format rule (NFR-007). Never skip a failing test (FR-003, known-wrong pinning).
- **SDLC guard:** agents cannot write `.github/workflows/**`. The CI job is drafted by the agent and applied by a human (Flagged concern 3).

## Flagged concerns

| # | Concern | Owner | Resolution |
|---|---|---|---|
| 1 | Testing ADR rule 8 says new test packages (Testcontainers modules) are added in a separate `build:` pull request, not in this feature's PR. That PR must merge before implementation starts. | chsakell | Open. The plan makes it task 0. |
| 2 | The provider setup is duplicated in the Api, Automations and Notifications hosts: the migrations assembly and the Npgsql legacy timestamp switch. A copy in the harness could drift from them. Extracting a shared helper touches three hosts. | chsakell (tech lead) | Open. Proposed: a copy in the harness with a check that keeps it equal; extracting a shared helper is left to a later item. Decided in the plan. |
| 3 | The SDLC guard blocks agent edits to `.github/workflows/ci.yml`. Making `Integration tests` a required check on `develop` and `main` is a repository setting. | chsakell | Open. The agent drafts the job in the scratchpad, and chsakell applies it and updates branch protection after the first green run. |
| 4 | `mcr.microsoft.com/mssql/server:2022-latest` is a floating tag, so a new cumulative update can change CI results without any change in the repository. Using the same tag as the deployment files keeps parity (NFR-006). | chsakell | Open. Proposed: keep parity now; pin all deployment files and tests to one exact tag in a later item. |
| 5 | Known-wrong behaviours expected from reading the code on 2026-11-26. They are pinned, not fixed: <br>(a) loading a board ignores the cards-per-list limit (FR-030, R-22); <br>(b) a board-access check for an unknown board throws (FR-031, R-08); <br>(c) card search is case-sensitive on PostgreSQL (FR-042, needs a roadmap entry); <br>(d) any pending model change FR-011 finds; <br>(e) the next serial number is a count of the board's cards, so it would repeat if a card were ever deleted (FR-033, R-22). <br>Each gets a note on the matching roadmap item and a line in the overview's Known issues in this PR (NFR-010). | chsakell | Open. Recorded as each test is written. |
| 6 | The SQL Server image has no Arm64 build. Contributors on Apple Silicon or Windows on Arm can run the SQL Server tests only through emulation, or not at all. | chsakell | Open. Proposed: accept. CI is the reference, and the PostgreSQL and MongoDB tests still run everywhere. |
| 7 | The intent says "open-source licences only". The SQL Server container image is not open source (NFR-008). It is already the deployment image and is used only at test time, but it is an exception to the intent's constraint. | chsakell (intent owner) | Open. Needs an explicit waiver at spec approval. |

## Open questions

| Question (from intent or new) | Answer, or "carried forward" | Owner |
|---|---|---|
| How are the integration tests left out of the test command but easy to run on purpose? | Answered as a contract here (FR-002 to FR-004). Carried forward: the mechanism, chosen in the plan by a trial (Design, Opt-in). | chsakell, in the plan |
| What run-time budget should the CI check meet, and should one instance be shared across a run? | Assumed: at most 10 minutes, with a 20-minute timeout (NFR-001). One instance per engine per run, with a fresh database per test class (FR-006). | chsakell |
| Do the upgrade tests cover all three databases? | Yes. Each of `HarmonyContext`, `AutomationContext` and `NotificationContext` has a migration for both providers (`Harmony.Persistence.Migrations.*`), and each is migrated at its host's startup. Hangfire's own tables are created by Hangfire and are out of scope. | - |
| Which SQL Server, PostgreSQL and MongoDB versions are tested? | The deployment versions: `docker-compose*.yml` and `.k8s/` (NFR-006). See Flagged concern 4. | - |
| Does Testcontainers need an ADR? | No. The testing strategy ADR names Testcontainers for `*.IntegrationTests` (rule 6) and covers its packages (rule 8). | - |
| Who adds the new check to branch protection? | chsakell, after the first green run (Flagged concern 3). | chsakell |
| Should the integration tests also cover the database seeders run at Api startup (roles, default users, workspace)? | Carried forward. Proposed: no. They need the Identity stack and belong with the Api test host (R-04). | chsakell |

## Out of scope

- Fixing any known-wrong behaviour the tests reveal (Flagged concern 5).
- API host, authorization and startup tests, including the seeders (R-04).
- The message broker, Redis, Algolia, email, gRPC and Hangfire job storage.
- The unused SQL Server-only board loading code (`LoadBoardOld`, `LoadBoardListCardsOld`).
- Card repository members not named in FR-020 and FR-033 to FR-035, and the other repositories. They can be added by later items that need them.
- Upgrade paths from older releases. Today each context has one migration per provider, so "from empty" covers the full upgrade path.
- A coverage threshold.
- Pinning exact image tags in the deployment files (Flagged concern 4).

## Rollout

1. **`build:` pull request** with the Testcontainers package versions in `src/Directory.Packages.props` (Flagged concern 1). It merges first.
2. **This feature's pull request** adds the integration test project, the docs and the drafted CI job, which chsakell applies to `ci.yml`.
3. **After the job has passed three times** (NFR-004), and before this pull request merges, chsakell adds `Integration tests` as a required check on `develop` and `main`.

There is no runtime effect and nothing to migrate. To roll back, revert the PR and remove the required check. Contributors learn about the new command from AGENTS.md. The `harmony-docs` GitBook needs no change, because nothing changes for users.
