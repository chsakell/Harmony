---
name: sdlc-implement
description: Executes an approved plan.md task by task with test-first red-green-refactor cycles, keeps plan.md in sync, and commits each task with Conventional Commits and the GitHub issue reference (#<id>). Use when a plan has the sdlc:plan-approved label and the user asks to implement, build, code or continue a feature. DO NOT USE without an approved plan, for bug investigation (use sdlc-debug), or for small track-S fixes.
argument-hint: "[docs/features/<id>-<slug>] [from task Tn]"
metadata:
  version: "1.0.0"
  source: "Adapted from obra/superpowers executing-plans and test-driven-development (MIT) and the Claude AI-native SDLC playbook (Stage 3: Build). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Implement the plan

Build exactly what `plan.md` says, one task at a time, test first. The plan is the contract: if reality differs, update the plan in the same commit or stop and ask.

## Iron rule

**No production code without a failing test first.** If you wrote code before its test, delete that code and start the task again from the test.

## 1. Preconditions

Stop and report if any of these fails:

1. You are on `feature/<id>-<slug>` and the folder `docs/features/<id>-*/` exists.
2. The plan is approved: label `sdlc:plan-approved`, or `status: approved` in `plan.md`. If only the label is present, record it in `plan.md` front matter (`status: approved`, `approved-by: <login>, <date>`) and commit that first.
3. The working tree is clean (`git status`).
4. The branch is up to date with `develop` (`git fetch` then `git merge origin/develop` if behind; never rebase a branch that already has a PR).
5. The baseline is green: run the build and test commands from `AGENTS.md`. If something already fails, report it and ask whether to continue; do not fix unrelated failures silently.

## 2. Loop over the tasks

For each unchecked task in order (or from the task the user named):

1. **Announce** the task, the requirement IDs and the files.
2. **Red.** Write the test named in the plan, tagged `[Trait("Requirement", "<ID>")]`. Run only that test and confirm it fails for the expected reason (missing behaviour, not a compile error in unrelated code):

   ```powershell
   dotnet test --project <test project> --filter-method "*<TestMethodName>*"
   ```

3. **Green.** Write the minimum production code to make it pass. Follow the installed instruction files for the layer you are in.
4. **Refactor.** Remove duplication and improve names while the test stays green.
5. **Check the neighbourhood.** Run the tests of the affected test projects (or the whole solution if it is fast).
6. **Update the plan.** Tick the task (`- [x]`). If anything differed from the plan - another file, a changed signature, an extra step - add a row to the Deviations log.
7. **Commit** code, tests and the plan update together:

   ```powershell
   git add -A
   git commit -m "<type>(<scope>): <imperative description>" -m "<why, if not obvious>" -m "Refs #<id>"
   ```

   Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `ops`, `chore`. No work item ID in the scope.

## 3. Stop and ask when

- the plan is wrong or incomplete in a way that changes scope, a contract or the data model (the spec may need to change - that goes back to the product owner),
- a test that existed before this feature would have to change,
- a new NuGet package or a version change is needed,
- an action is destructive or irreversible (dropping data, deleting files outside the plan, force-pushing),
- a security-sensitive change is needed that the plan does not mention,
- three attempts at the same task have failed - switch to the `sdlc-debug` skill.

## 4. Rules that are easy to break

- Never weaken, skip, delete or rewrite a failing test to get green. Fix the code.
- Never edit `intent.md` or `spec.md` during implementation.
- EF Core migrations: create them with `dotnet ef migrations add <Name>`; never hand-edit the model snapshot. If `develop` gained a migration since you branched, merge `develop` first, then remove and re-create yours.
- Keep secrets out of code and `appsettings*.json`; use user secrets or the configured secret store.
- Touch only the files the task needs.

## 5. Parallel work

Tasks marked `[P]` can run in separate sessions, each in its own git worktree on its own branch off the feature branch (`git worktree add ../<repo>-<id>-t3 -b feature/<id>-<slug>-t3`), merged back into the feature branch when green. Start with two sessions at most; add more only while review keeps up.

## 6. Finish

When every task is ticked, run `/sdlc-verify`. Do not report the feature as done before it passes.
