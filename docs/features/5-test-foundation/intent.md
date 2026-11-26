---
id: 5
title: Test foundation
artifact: intent
status: draft            # draft | approved | superseded
owner: chsakell
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:intent-approved label is added
created: 2026-11-26
source: "#5 (roadmap R-01)"
---

# Intent: Test foundation

> One page at most. Business language only: say what is wrong and what "better" looks like, not how to build it.

## Problem

Harmony has no automated tests. Running the test command for the whole solution executes nothing, so:

- No change can be proven correct. The SDLC requires every requirement to be backed by a passing test, and today that is impossible.
- Nothing protects against regressions. The roadmap lists about 30 items, including security fixes (R-07 to R-13) and data-corrupting bugs (R-15, R-16, R-22). Each of them needs a failing test first, and there is nowhere to put one.
- Contributors and AI agents have no example to follow, so the first tests each item writes would invent their own conventions.

Evidence: `docs/architecture/overview.md` (Known issues) and AGENTS.md ("There are no test projects yet, so this currently runs nothing").

## Who is affected

- **Maintainers and contributors:** they get a place to put tests and a pattern to copy. From then on, every change is expected to come with tests.
- **AI agents working in the repository:** the verify step can finally check that each requirement has a passing test.
- **Self-hosting teams:** they benefit indirectly, because later fixes come with proof and are less likely to break again.
- **End users:** no visible change.

## Desired outcome

Anyone can run one command from the repository root and see a set of fast, passing tests. The tests:

- run without Docker, a database or network access;
- are tagged with the requirement they prove;
- cover the core helper routines (card key generation, date display, issue type rules, building query filters) and the object mapping configuration;
- include one example of testing a request handler with its dependencies replaced by test doubles. Later items (R-05, R-15, R-16) copy this pattern instead of inventing their own.

The testing approach (frameworks, libraries, project layout and how tests are run) is recorded in an architecture decision. Later test projects (integration, API and architecture tests) then follow the same rules.

### Success measures

- The solution's test command runs and passes from a clean clone on a developer machine, with no Docker running.
- Every test carries a requirement tag, and tests can be selected by that tag.
- A code coverage report is produced for every test run. No minimum coverage is enforced yet; this first report is the baseline.
- The build adds no new warnings above the recorded baseline of 669.
- AGENTS.md documents the working test command.

## Scope

- **In scope:**
  - The shared test folder and its common settings.
  - The first unit test project.
  - Tests for the helpers and the mapping configuration.
  - One reference test for a request handler.
  - Coverage reporting.
  - The testing strategy decision record.
  - Updating the test command in AGENTS.md.
- **Out of scope:**
  - A CI workflow (R-02).
  - Database and container-based integration tests (R-03).
  - API host and authorization tests (R-04).
  - Characterization tests for Scrum and automations (R-05).
  - Architecture rule tests (R-06).
  - A minimum coverage threshold.
  - Fixing any bug the new tests reveal. Bugs found are recorded, not fixed here.

## Constraints

- **Open-source licences only.** No commercial libraries; FluentAssertions 8 is excluded for this reason.
- **No behaviour change** to the application. Production code changes only where it is strictly needed to make code testable, and each such change is justified in the plan.
- The build stays at 0 errors, with no warnings above the 669 baseline. Formatting must pass for the changed files.
- Must work with the existing .NET 10 SDK pin and central package management. Today the SDK pin does not opt into the newer test runner (see Open questions).
- Tests must stay fast and self-contained so that every pull request can run them.

## Affected systems

- The core Application layer (helpers, mapping configuration, one request handler). Read only, apart from testability changes justified in the plan.
- The shared helpers library.
- Solution-level build configuration: the solution file, central package versions and the SDK pin.
- AGENTS.md (the test command).
- No runtime services, databases, message broker or other teams' systems are touched.

## Open questions

- [ ] Opt into Microsoft.Testing.Platform or stay on VSTest? This decides the test command, how tests are filtered by requirement tag (the verify step expects `--filter-trait`), and which coverage tool works. - chsakell (tech lead), in the spec/ADR
- [ ] Which assertion library: Shouldly (MIT) or AwesomeAssertions (Apache 2.0)? - chsakell, in the ADR
- [ ] Which coverage tool fits the chosen runner (coverlet.collector, coverlet.MTP or Microsoft code coverage)? - chsakell, in the ADR
- [ ] Which request handler is the reference example? Proposed approach to evaluate in the spec: one that touches only interfaces and is used by an upcoming roadmap item. - chsakell, in the spec
- [ ] Can the mapping test assert real input and output pairs, so that it still holds after AutoMapper is replaced (R-14)? Proposed approach to evaluate in the spec. - chsakell
- [ ] Should the new test packages go in a separate `build:` pull request (the SDLC guide's rule for central package changes), or is an exception logged in the plan? - chsakell
