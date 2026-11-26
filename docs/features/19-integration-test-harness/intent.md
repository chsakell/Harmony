---
id: 19
title: Integration test harness
artifact: intent
status: approved         # draft | approved | superseded
owner: chsakell
approved-by: chsakell, 2026-11-26
created: 2026-11-26
source: "#19 (roadmap R-03)"
---

# Intent: Integration test harness

> One page at most. Business language only: say what is wrong and what "better" looks like, not how to build it.

## Problem

Harmony lets self-hosting teams choose SQL Server or PostgreSQL. Nothing checks that the product works on either of them:

- **Database upgrades are unproven.** Each release ships schema changes for both databases. No test applies them, so a broken upgrade on one database is found by the team that installs it.
- **Queries are proven only in memory.** The unit tests from R-01 run queries against in-memory lists. They cannot show how a real database runs the query. The two bugs fixed so far (#11 backlog reorder, #17 card search) were proven this way only. For card search, nobody knows whether matching a title ignores upper and lower case on PostgreSQL. SQL Server ignores it by default; PostgreSQL does not.
- **Hidden rules surprise developers.** The data layer silently hides some records, for example every subtask in card queries and every inactive workspace. Only a real database shows what each query actually returns.
- **The document store is untested.** Automation rules and GitHub activity live in MongoDB, and no test touches them.
- **Later roadmap items depend on this.** R-22 (card keys, positions and paging) and R-23 (reliable messaging and transactions) need a failing test against a real database first. Today there is nowhere to put one.

Evidence: roadmap R-03; `docs/architecture/overview.md` (Known issues: "Still missing: integration tests against the databases"); the in-memory tests for #11 and #17 in the Infrastructure unit tests.

## Who is affected

- **Self-hosting teams on either database:** fewer broken upgrades and fewer bugs that appear on only one database.
- **Maintainers and contributors:** they get a place and a pattern for tests against real databases. Running these tests needs Docker, but only when asked for: the everyday test command stays as it is.
- **Reviewers:** every pull request shows whether the data layer still works on both databases.
- **AI agents:** they can prove data-layer fixes, such as R-22 and R-23, with a failing test first.
- **End users:** no visible change.

## Desired outcome

A contributor with Docker can run one command and see the data layer tested against real SQL Server, PostgreSQL and MongoDB instances that start and stop on their own. The tests:

- apply every database upgrade from an empty database, on both providers, for each database Harmony owns;
- run the most-used and most fragile queries on both providers: loading a board, checking whether a user may open a board, listing a user's boards, card storage, the backlog reorder (#11) and the card search (#17);
- cover the stored automation rules and GitHub activity in MongoDB;
- are tagged with the requirement they prove, like every other test.

The everyday test command still runs without Docker. Every pull request runs the integration tests in a separate CI check, and that check must pass before the pull request merges.

### Success measures

- Every database upgrade applies cleanly from an empty database on SQL Server and on PostgreSQL, proven on every pull request.
- Each query named above has at least one passing test on both providers. Any difference between them is recorded as a known issue, not hidden.
- `dotnet test --solution Harmony.slnx` still passes with Docker stopped.
- The integration CI check is required on `develop` and `main` and passes on this item's pull request.
- AGENTS.md documents how to run the integration tests.
- No new build warnings above the baseline of 585 code warnings.

## Scope

- **In scope:**
  - The integration test project for the data layer, with disposable SQL Server, PostgreSQL and MongoDB instances.
  - Upgrade (migration) tests on both providers.
  - Tests for the queries and repositories named above.
  - Tests for the MongoDB storage of automation rules and GitHub activity.
  - A way to run the integration tests on purpose, while the everyday command leaves them out.
  - A separate CI job, made a required check.
  - Updates to AGENTS.md and the overview's Known issues.
- **Out of scope:**
  - Fixing any defect the new tests reveal, including a case-sensitive card search on PostgreSQL. Defects are pinned as known issues and recorded on the roadmap, not fixed here.
  - API host and authorization tests (R-04).
  - Tests for the message broker, cache, search index, email or the gRPC links between services.
  - The unused SQL Server-only board loading code (Known issues); it is neither tested nor removed here.
  - Tests for every repository. Only the ones named above, plus what the spec finds necessary.
  - A minimum coverage threshold.

## Constraints

- **Open-source licences only**, as in the testing strategy ADR.
- **No behaviour change** to the application. Production code changes only where strictly needed for testability, each justified in the plan.
- **Docker required for these tests only.** They must run on Docker Desktop for Windows and on the GitHub-hosted Linux runner. They must never touch a developer's own local Harmony containers or data.
- **Testing strategy ADR applies:** xUnit v3 on Microsoft.Testing.Platform, requirement tags, and current defects pinned as `_KnownIssue` tests, never skipped. New packages follow the normal rule of a separate `build:` pull request (ADR rule 8).
- The CI check must stay fast enough to run on every pull request (see Open questions).
- Build at 0 errors, no warnings above the baseline, formatting passes for changed files.

## Affected systems

- The data layer: database model and upgrade scripts for both providers, repositories, and the board and card services. Read only, apart from testability changes.
- The MongoDB storage used by the Automations and SourceControl services. Read only.
- The test tree, central package versions and the solution file.
- The CI workflow and the branch protection settings on `develop` and `main` (one more required check).
- AGENTS.md, `docs/architecture/overview.md` and `docs/roadmap.md`.
- No running service, message broker or other team's system changes.

## Open questions

- [ ] How are the integration tests left out of the everyday command but easy to run on purpose (a trait filter, a separate command, an environment switch)? Proposed approach to evaluate in the spec. - chsakell, in the spec
- [ ] What run-time budget should the CI check meet (for example under 10 minutes), and should one database instance be shared across a test run to meet it? - chsakell, in the spec
- [ ] Do the upgrade tests cover all three databases (Harmony, Automations jobs, Notifications jobs), or only the main Harmony database? Proposed: all three, since each has its own upgrades on both providers. - chsakell, in the spec
- [ ] Which SQL Server, PostgreSQL and MongoDB versions are tested: the ones the Aspire AppHost and Docker Compose use, or the oldest supported? - chsakell, in the spec
- [ ] Does Testcontainers need an ADR, or is it covered by the testing strategy ADR (which names it only as an example)? - chsakell, in the spec
- [ ] Who adds the new CI check to branch protection? Workflows and repository settings are changed by a human. - chsakell
