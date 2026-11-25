---
name: sdlc-verify
description: Proves work is done with fresh evidence - build, tests, formatting, requirement-to-test traceability and plan completeness - and produces the verification block for the PR. Use when about to claim any task, fix or feature is complete, fixed or passing, before committing a finished task, and before opening or updating a PR.
argument-hint: "[docs/features/<id>-<slug>]"
metadata:
  version: "1.0.0"
  source: "Adapted from obra/superpowers verification-before-completion (MIT) and the Claude AI-native SDLC playbook (Stage 4: Test, feedback loop). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Verify before claiming done

## Iron rule

**No claim of "done", "fixed", "passing" or "works" without evidence produced in this session.** Words like "should", "probably" or "looks fine" are not evidence. Run the command, read the output, then make the claim and show the output.

## 1. Get the commands

Use the Build, Test and Format commands listed in `AGENTS.md`. They are the source of truth. Only if `AGENTS.md` has none, fall back to:

```powershell
dotnet build <Solution>.slnx
dotnet test --solution <Solution>.slnx
dotnet format <Solution>.slnx --verify-no-changes
```

## 2. Run them

| Check | Pass condition |
|---|---|
| Build | exit code 0, 0 errors, 0 warnings (warnings fail the build in strict repositories) |
| Test | exit code 0, every test passed, no test newly skipped |
| Format | exit code 0 |

Read the whole output, not only the last line. Record the exact command, exit code and counts.

## 3. Trace requirements to tests

For a feature (track M or L), read the IDs from `spec.md` and check each one:

```powershell
dotnet test --solution <Solution>.slnx --filter-trait "Requirement=FR-001"
```

Every FR and NFR must have at least one passing test. An NFR that cannot be proven by an automated test (for example a load target) needs the evidence named in the plan's Proof section.

## 4. Check the plan

- Every task in `plan.md` is ticked.
- Every difference from the plan is in the Deviations log.
- Compare the changed files with "Files that change":

  ```powershell
  git diff --name-only origin/develop...HEAD
  ```

  Any file not in the plan and not in the Deviations log is a finding.

## 5. Optional: independent check

For user-visible behaviour, dispatch the `verifier` subagent. It runs the application in a fresh context, exercises the changed behaviour and two neighbouring flows, and reports without fixing.

## 6. Report

Produce this block for the PR description:

```markdown
## Verification evidence
| Check | Command | Result |
|---|---|---|
| Build | `<command>` | exit 0, 0 warnings |
| Test | `<command>` | exit 0, <n> passed, 0 failed, 0 skipped |
| Format | `<command>` | exit 0 |

| Requirement | Test(s) | Result |
|---|---|---|
| FR-001 | `<Class.Method>` | pass |

Plan: <n>/<n> tasks done, <k> deviations logged. Unplanned files: none.
```

## If something fails

Do not claim partial success. Go back to the failing task (`sdlc-implement`) or investigate (`sdlc-debug`), fix the code - never the test - and run every check again.
