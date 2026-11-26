---
id: 22
title: Api test host and authorization matrix
artifact: intent
status: draft            # draft | approved | superseded
owner: chsakell
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:intent-approved label is added
created: 2026-11-26
source: "#22 (roadmap R-04)"
---

# Intent: Api test host and authorization matrix

> One page at most. Business language only: say what is wrong and what "better" looks like, not how to build it.

## Problem

Nobody can say which parts of the core API need a login, and nothing would notice if a new one were left open.

- **Most of the API is open.** The core API has 20 controllers with 106 actions. One controller and three single actions say "login required"; four actions say "public on purpose". The rest have no rule, and the API has no "login required unless stated" default. So anonymous callers can reach role, permission and user administration (R-07).
- **Board membership is not checked.** Any logged-in user can act on a board, card, list or workspace whose ID they know (R-08).
- **There is no way to test the API as a whole.** The tests from R-01 and R-03 call the code below the API directly. No test sends a real request through the API with or without a login, so a request that is let in when it should be refused cannot be proven.
- **The security fixes need a failing test first.** R-07, R-08, R-11 and R-13 change who may do what. Without a test that shows today's gaps, nobody can show those fixes are complete or stop them from coming back.

Evidence: roadmap R-04, R-07, R-08; `src/Services/Harmony.Api/Controllers/**`; `docs/architecture/overview.md` (Known issues: "Still missing: tests against ... the Api host and authorization (R-04)").

## Who is affected

- **Self-hosting teams:** they get a published, tested answer to "which endpoints need a login", and the security fixes that follow are proven.
- **Maintainers and contributors:** they get a place and a pattern for tests that call the API end to end. Every new endpoint must state whether it is public, or the build fails.
- **Reviewers:** an unprotected endpoint shows up as a failing check, not as something to spot in the diff.
- **AI agents:** they can prove R-07 and R-08 fixes with a failing row first.
- **End users:** no visible change.

## Desired outcome

A test host starts the core API in memory, with a test database and test logins, and with email, messaging, search and caching replaced so nothing leaves the test. Through it, an **authorization matrix** lists every API endpoint automatically and proves two rules:

1. An anonymous caller is refused (401) on every endpoint that is not deliberately public.
2. A logged-in user who is not a member is refused on another user's board, card, list and workspace.

Today most rows fail. R-04 lands on its own with those rows checked in as a reviewed **expected-failure list**, pinned like the `_KnownIssue` tests, never skipped. R-07 and R-08 empty the list. Any endpoint added later without a decision fails at once.

### Success measures

- Every core API endpoint appears in the matrix without anyone adding it by hand.
- The expected-failure list names every gap that exists today, and its size is recorded in the overview's Known issues and on R-07 and R-08.
- A new unprotected endpoint fails the test run on the pull request that adds it.
- An endpoint that becomes protected but is still on the list also fails, so the list can only shrink.
- `dotnet test --solution Harmony.slnx` passes, and the build adds no warnings above the baseline of 585 code warnings.
- AGENTS.md says how to run the Api tests.

## Scope

- **In scope:**
  - An Api integration test project and test host, with test logins and stand-ins for email, messaging, search and caching.
  - The authorization matrix for the anonymous rule and the non-member rule, over the core API's REST endpoints.
  - The checked-in expected-failure list, and its link to R-07 and R-08.
  - Updates to AGENTS.md, `docs/architecture/overview.md` and `docs/roadmap.md`.
- **Out of scope:**
  - Fixing any gap the matrix shows: that is R-07 and R-08.
  - Board role rules (Admin manages members, Observers read only): R-08 decides them and adds their rows.
  - The core API's internal gRPC endpoints (R-09), and the Automations, SourceControl, SignalR and gateway services (R-10).
  - Token, password and login hardening (R-11) and file upload safety (R-13).
  - Functional tests of each endpoint beyond "let in" or "refused".

## Constraints

- **No behaviour change** to the application. Production code changes only where strictly needed to host it in tests, each justified in the plan.
- **Nothing leaves the test:** no real email, broker message, search index update or call to GitHub.
- **Testing strategy ADR applies:** xUnit v3 on Microsoft.Testing.Platform, requirement tags, known defects pinned and never skipped. New packages arrive in a separate `build:` pull request (ADR rule 8).
- Open-source licences only. Build at 0 errors, no warnings above the baseline, formatting passes for changed files.

## Affected systems

- The core API service: its startup and controllers. Read only, apart from testability changes.
- The test tree, central package versions, the solution file and possibly the CI workflow.
- AGENTS.md, `docs/architecture/overview.md` and `docs/roadmap.md`.
- No running service, message broker or other team's system changes.

## Open questions

- [ ] Which database does the test host use: the real SQL Server or PostgreSQL containers from R-03 (needs Docker, so it joins the integration test run), or an in-memory store that keeps it in the everyday command? Proposed approach to evaluate in the spec. - chsakell, in the spec
- [ ] What shape does the expected-failure list take (a data file or a list in code), and how is it reviewed? - chsakell, in the spec
- [ ] For the non-member rule, which endpoints count as "naming" a board, card, list or workspace, and how does the matrix find the ID to use? - chsakell, in the spec
- [ ] Should the matrix also cover the six gRPC services the core API exposes, now or in R-09? Proposed: R-09. - chsakell
- [ ] Does the list of deliberately public endpoints (login, register, token refresh) get agreed here or in R-07? - chsakell, in the spec
