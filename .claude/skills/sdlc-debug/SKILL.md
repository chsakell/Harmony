---
name: sdlc-debug
description: Finds the root cause of a bug, test failure, build failure or unexpected behaviour before changing code, then fixes it with a failing test committed first. Use when there is a bug report, failing test, broken build, flaky test or production error, for track-S bug fixes, and when an implementation task has failed twice.
argument-hint: "[#<id> | error text | failing test]"
metadata:
  version: "1.0.0"
  source: "Adapted from obra/superpowers systematic-debugging (MIT) and the Claude AI-native SDLC playbook (Stage 4: Test, failing test first). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Debug systematically

## Iron rule

**No fix without a root cause.** Guessing and patching symptoms creates new bugs and hides the real one.

## Track-S bug flow

For a bug reported as a GitHub issue that needs no intent or spec:

```powershell
git switch develop; git pull --ff-only
git switch -c bugfix/<id>-<slug>          # production hotfix: from main, branch hotfix/<id>-<slug>
```

Then follow the four phases below. The PR gets the label `sdlc:track-s`.

## Phase 1 - Reproduce and read

1. Read the full error: message, stack trace, inner exceptions, log lines around it. Do not skim.
2. Reproduce it reliably. If you cannot, collect more data before changing anything.
3. Check what changed recently: `git log --oneline -20`, `git diff origin/develop...HEAD`, dependency updates.
4. For build failures, capture a binary log: `dotnet build -bl` and read the first error, not the last.
5. For multi-component flows (API, handler, database, message broker), add temporary logging at each boundary to see where the data goes wrong. Remove it afterwards.

## Phase 2 - Compare with something that works

Find a similar flow or test that works. List every difference between the working and the broken case, including configuration, data and environment. Do not dismiss any difference as irrelevant before testing it.

## Phase 3 - One hypothesis at a time

State the hypothesis: "X is the cause because Y". Make the smallest change that tests it. If it is wrong, undo it before trying the next one. Never stack fixes.

## Phase 4 - Fix with a failing test first

1. Write a test that reproduces the bug, tagged with the work item: `[Trait("Bug", "<id>")]`. Run it and confirm it fails for the reason you identified.
2. Commit the test on its own:

   ```powershell
   git add <test file>
   git commit -m "test: reproduce <short bug description>" -m "Refs #<id>"
   ```

3. Fix the code. **Do not edit the test from step 1.** Its unchanged pass is the proof the bug is gone.
4. Run `/sdlc-verify`.
5. Commit the fix: `fix(<scope>): <description>` with `#<id>` in the footer.

## Stop after three failed fixes

If three fixes have failed, the problem is probably the design, not the code. Stop, summarize what you tried and what you learned, and discuss the architecture with the user before trying again. If the fix needs a contract or data model change, it is no longer track S: suggest `/sdlc-intent`.

## Flaky and async failures

- Never fix timing problems with `Thread.Sleep` or `Task.Delay`. Wait for the condition (poll with a timeout, or await the event).
- Check for shared state between tests, static caches, test ordering, and `DateTime.Now` or `Guid.NewGuid()` without a seam.
- With Testcontainers, check container readiness and per-test data isolation.
