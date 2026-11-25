---
id: <GitHub issue number>
title: <Same title as intent.md>
artifact: spec
status: draft            # draft | approved | superseded
owner: <Product owner>
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:spec-approved label is added
created: <YYYY-MM-DD>
intent: ./intent.md
---

# Spec: <title>

## Summary

<Two to four sentences: what will be delivered and why. Link the intent: [intent.md](./intent.md).>

## User stories

Stories are ordered by priority. Each one can be built, tested and demonstrated on its own.

### US1 - <title> (P1)

As a <role>, I want <capability>, so that <benefit>.

- **Why this priority:** <...>
- **Independent test:** <how this story is demonstrated without the others>

## Functional requirements

Write each requirement as one EARS statement (see the notation guide in the `sdlc-spec` skill). Every requirement has an ID, at least one acceptance scenario, and later at least one automated test tagged with that ID.

| ID | Requirement | Story |
|---|---|---|
| FR-001 | When <trigger>, the <system> shall <response>. | US1 |
| FR-002 | If <unwanted condition>, then the <system> shall <response>. | US1 |

### Acceptance scenarios

- **FR-001** - Given <context>, when <action>, then <observable result>.
- **FR-002** - Given <context>, when <action>, then <observable result>.

## Non-functional requirements

| ID | Category | Requirement (measurable) |
|---|---|---|
| NFR-001 | Security | <...> |
| NFR-002 | Performance | <for example "p95 latency under 200 ms at 50 requests per second"> |
| NFR-003 | Observability | <...> |

## Design

Describe contracts and boundaries. File-level detail belongs in `plan.md`.

### Components affected

<Projects, modules or services that change, and how. Link docs/architecture/overview.md.>

### API contract

<Endpoints, request and response shapes, status codes, ProblemDetails cases, versioning.>

### Data model

<New or changed entities, columns, indexes, migrations, data retention.>

### Events and integrations

<Messages published or consumed, external calls, idempotency.>

### UX notes

<Screens, states, copy, accessibility - or "not applicable".>

## Standards and policies applied

<Which installed instruction files and policy skills constrained this spec, and what each one changed. Example: "api-design-minimal-apis: ProblemDetails for all 4xx responses".>

## Flagged concerns

Concerns the agent or reviewers raised that need a decision from a named owner before build.

| # | Concern | Owner | Resolution |
|---|---|---|---|
| 1 | <conflict between the intent and a policy, an architectural limit, a data or security risk> | <policy owner> | <decision, or "open"> |

## Open questions

| Question (from intent or new) | Answer, or "carried forward" | Owner |
|---|---|---|
| <...> | <...> | <...> |

## Out of scope

- <...>

## Rollout

<Feature flag, migration order, backward compatibility, data backfill, communication.>
