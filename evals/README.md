# Agent evals

Evals check that the agent configuration in this repository (`AGENTS.md`, `.claude/skills`, `.claude/hooks`, the installed instructions) still makes the agent do the right thing. They run in CI on every change to that configuration and every weekday night (`.github/workflows/agent-evals.yml`), so a change to a skill or a model upgrade that makes the agent worse is caught before it reaches the team.

## How a case works

Each file in `cases/` is one task:

```json
{
  "name": "spec-from-intent",
  "description": "What this case proves",
  "setup": [ { "from": "evals/fixtures/9001-export-orders", "to": "docs/features/9001-export-orders" } ],
  "branch": "feature/9001-export-orders",
  "prompt": "The prompt given to the agent, non-interactively",
  "checks": [
    { "type": "fileExists", "path": "docs/features/9001-export-orders/spec.md" },
    { "type": "fileMatches", "path": "docs/features/9001-export-orders/spec.md", "pattern": "FR-001" },
    { "type": "fileNotMatches", "path": "...", "pattern": "..." },
    { "type": "noChanges", "paths": ["src", "tests"] },
    { "type": "command", "run": "dotnet build" }
  ]
}
```

`run-evals.ps1` creates a throwaway git worktree, copies the setup files, optionally switches to `branch`, runs the agent (`copilot -p` or `claude -p`), then runs the checks. The run fails when the pass rate is below `-MinPassRate`.

## Run locally

```powershell
./evals/run-evals.ps1 -Agent copilot            # or -Agent claude
./evals/run-evals.ps1 -Agent claude -Filter spec-*
```

## Grow the suite

- Replace the sample fixtures with small features from your own domain.
- Collect 20 to 50 real tasks from recent work, each with the outcome the team accepted.
- Every production incident gets a case, written by the team that owned it, and keeps it as a regression test.
- Every new policy skill gets at least one case (see the `sdlc-write-skill` skill).
- Prefer checks on observable results (files, sections, IDs, build and test exit codes) over exact wording; agent output varies between runs.
