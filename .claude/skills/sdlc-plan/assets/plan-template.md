---
id: <GitHub issue number>
title: <Same title as spec.md>
artifact: plan
status: draft            # draft | approved | superseded
owner: <Engineer>
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:plan-approved label is added
created: <YYYY-MM-DD>
spec: ./spec.md
---

# Plan: <title>

## Approach

<Three to six sentences: how the change fits the existing architecture, which existing patterns it reuses, and which ADRs apply.>

## Files that change

| File | Change | Why |
|---|---|---|
| `src/<Project>/<File>.cs` | new | <...> |
| `tests/<Project>.Tests/<File>Tests.cs` | new | <...> |

## Tasks

Tasks run in order. `[P]` marks a task that touches no file used by another `[P]` task, so it can run in a parallel session.
Each task starts with a failing test that proves it.

- [ ] **T1** <task> - covers FR-001 - files: `<...>` - proof: `<TestClass>.<Method_Scenario_Expected>`
- [ ] **T2** [P] <task> - covers FR-002 - files: `<...>` - proof: `<...>`
- [ ] **T3** <task> - covers NFR-001 - files: `<...>` - proof: `<...>`

### Coverage

| Requirement | Task(s) | Test(s) |
|---|---|---|
| FR-001 | T1 | `<...>` |
| NFR-001 | T3 | `<...>` |

## Risks and what could break

| Risk | Likelihood / impact | Mitigation |
|---|---|---|
| <the riskiest step, regressions in neighbouring flows, performance, data> | <...> | <...> |

## Alternatives considered

| Option | Why not chosen |
|---|---|
| <...> | <...> |

## Migration and rollback

<Database migration order, feature flag, how to undo the change in production.>

## Proof (definition of done)

- Build: `<build command from AGENTS.md>` - exit 0, 0 warnings
- Test: `<test command from AGENTS.md>` - all green
- Format: `<format command from AGENTS.md>` - exit 0
- Every FR and NFR ID has at least one passing test tagged `[Trait("Requirement", "<ID>")]`
- <feature-specific check, for example "GET /api/v1/orders/{id}/status returns 200 with the new contract">

## Deviations log

Update this table in the same commit as any change that departs from the plan.

| Date | Task | What changed and why |
|---|---|---|
