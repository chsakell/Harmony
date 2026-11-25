# Review instructions

Review every pull request against:

- the feature's `docs/features/<id>-*/spec.md` and `plan.md` (for `feature/` branches),
- `AGENTS.md`,
- the installed standards in `.github/instructions/` (Copilot) or `.claude/rules/` (Claude Code).

## Passes

Run three passes and tag each finding with its pass.

- **Bugs:** logic errors, unhandled edge cases, null handling, async and cancellation mistakes, disposal, EF Core query problems (N+1, client-side evaluation, missing `AsNoTracking` on reads), race conditions, regressions in neighbouring flows.
- **Security:** missing authentication or authorization, resource access without ownership checks, injection, secrets in code or configuration, personal data in logs or error responses, missing input validation, unsafe deserialization.
- **Compliance:** the diff does what `spec.md` asks and nothing more; it follows `plan.md` or the difference is in the Deviations log; every FR and NFR ID has a test tagged `[Trait("Requirement", "<ID>")]`; the installed standards are followed.

## What Important means here

Important is a finding that would break behaviour, leak data, breach a standard or policy, or leave a requirement untested. Naming, style and optional refactors are Nits.

## Cap the nits

Report at most five nits per review and summarize the rest as a count.

## Bug fixes

On `bugfix/` and `hotfix/` pull requests, an edit to a test that existed before the fix is Important: the fix must make the original failing test pass unchanged.

## Do not report

- Generated files: `**/Migrations/*.Designer.cs`, `**/Migrations/*ModelSnapshot.cs`, `**/obj/**`, `**/bin/**`.
- Anything the build already enforces: analyzer warnings, formatting, banned APIs.
- Wording and Markdown style in `docs/`.
