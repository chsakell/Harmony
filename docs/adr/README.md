# Architecture decision records

One file per decision: `YYYY-MM-DD-<slug>.md`. The date prefix keeps names unique across parallel branches and sorts the folder chronologically.

| Status | Meaning |
|---|---|
| proposed | Under review in a pull request |
| accepted | In force |
| rejected | Considered and not adopted; kept so the reasoning is not lost |
| superseded | Replaced; `superseded-by:` names the new ADR |
| deprecated | No longer relevant, not replaced |

Rules:

- Write an ADR for decisions that are hard to reverse, affect more than one feature or team, or depart from the installed standards. Use `/sdlc-adr`.
- Never rewrite an accepted decision. Write a new ADR that supersedes it.
- Architects are code owners of this folder and approve ADRs in the pull request.
- Link ADRs from the spec or plan that needed them and from the decision index in [../architecture/overview.md](../architecture/overview.md).
