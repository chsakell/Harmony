---
title: Tag every test with a Requirement trait, or with a Bug trait for a Track S bug fix, enforced by a shared guard
status: proposed         # proposed | accepted | rejected | superseded | deprecated
date: 2026-11-26
deciders: chsakell
work-item: "#12"
supersedes: 2026-11-26-testing-strategy.md (rule 7, first bullet only)
superseded-by:
---

# Tag every test with a Requirement trait, or with a Bug trait for a Track S bug fix, enforced by a shared guard

## Context

The [testing strategy ADR](2026-11-26-testing-strategy.md), rule 7, says every test has a `Requirement` trait matching `FR-nnn` or `NFR-nnn`, and that a guard test in each test project enforces it. Two things now conflict with that rule:

- **Track S bug fixes have no spec.** The SDLC guide sends small bug fixes through Track S: a failing test first, no intent, spec or plan. There is no requirement ID to tag the reproduction test with. The `sdlc-debug` skill already tags it `[Trait("Bug", "<id>")]`, which the guard rejects. The first case is #11 (roadmap R-15).
- **The guard cannot be reused.** It lives in `Harmony.Application.UnitTests`. #11 adds a second test project, `Harmony.Infrastructure.UnitTests`, and R-03 to R-06 add more. Each one needs the same guard.

The rest of the testing ADR (runner, libraries, layout, the `_KnownIssue` and clock rules) is unchanged. Spec [#5](../features/5-test-foundation/spec.md) FR-005 describes the guard as rule 7 originally defined it.

## Decision

1. Every test method carries, on the method or its class, at least one of:
   - a `Requirement` trait matching `FR-nnn` or `NFR-nnn`, for work with a spec (Track M and L), or
   - a `Bug` trait whose value is the GitHub issue number (for example `[Trait("Bug", "11")]`), for a Track S bug fix that has no spec.
2. Any `Requirement` or `Bug` value that does not match its format fails the guard, even when another valid tag is present.
3. The guard (`RequirementTagGuard`) lives in `tests/Shared/`. `tests/Directory.Build.props` compiles every file in that folder into every test project. Each test project has one test that applies the guard to its own assembly. The guard's own behaviour is tested once, in `Harmony.Application.UnitTests`.

Select a bug's tests with `dotnet test --solution Harmony.slnx --filter-trait "Bug=<id>"`.

## Alternatives considered

| Option | Pros | Cons | Why not chosen |
|---|---|---|---|
| Keep Requirement-only and give each bug fix a spec or a pseudo requirement ID | One tag kind | Goes against Track S ("no documents"). Invented IDs point to nothing. | Adds paperwork to the smallest changes, or traceability that is not real |
| Exempt bug tests from the guard by convention (for example a naming rule) | No guard change | Untraceable to the issue. The guard can no longer tell a forgotten tag from an exemption. | Weakens the guard that rule 7 introduced |
| Copy the guard into each test project | No shared folder | Copies drift apart. Every fix has to be made N times. | One shared source is cheaper to keep correct |
| A shared test-support class library project | Normal project reference; can grow other helpers | A project that needs xUnit references but is not itself a test project; more solution plumbing for one file | A linked source folder is enough for one file. Revisit if `tests/Shared` grows. |

## Consequences

- **Positive:**
  - Track S bug tests are traceable to their issue, and `--filter-trait "Bug=<id>"` selects them.
  - New test projects get the guard without copying it.
- **Negative:**
  - Two tag kinds to know about. Spec #5 FR-005 alone no longer describes the guard; read it together with this ADR.
  - Files in `tests/Shared/` compile into every test project, so they must only depend on what every test project references (xUnit).
- **Follow-up work:** none. AGENTS.md ("How we work") and spec #5 (a note under FR-005) point to this ADR.
