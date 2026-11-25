---
name: sdlc-plan
description: Produces an implementation plan.md from an approved spec.md - files that change, ordered test-first tasks tied to requirement IDs, risks, alternatives, rollback and proof. Use when a spec has the sdlc:spec-approved label, when the user asks how to implement a feature, or asks for an implementation plan. Works best in plan mode. DO NOT USE FOR writing the spec (use sdlc-spec) or executing a plan (use sdlc-implement).
argument-hint: "[docs/features/<id>-<slug>]"
metadata:
  version: "1.0.0"
  source: "Adapted from obra/superpowers writing-plans (MIT), github/spec-kit plan and tasks (MIT) and the Claude AI-native SDLC playbook (Stage 3: Build, plan mode). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Write the implementation plan

The plan says **how** the spec will be built: which files change, in which order, and which tests prove it. Design review happens here, while changing course is still editing a document.

Run this in plan mode (VS Code: the Plan agent; Claude Code: plan mode; Copilot CLI: `/plan` or `--plan`) so nothing in the repository changes until the engineer accepts the plan.

## 1. Find the feature and check the gate

1. Take the work item ID from the branch name `feature/<id>-<slug>` or the argument; the folder is `docs/features/<id>-*/`.
2. Check that the spec is approved: label `sdlc:spec-approved` on the PR, or `status: approved` in `spec.md`.

   ```powershell
   gh pr view --json number,labels
   gh api "repos/{owner}/{repo}/issues/<pr-number>/events" --jq '.[] | select(.event=="labeled" and .label.name=="sdlc:spec-approved") | "\(.actor.login), \(.created_at[0:10])"'
   ```

   If not approved, stop.
3. Plan the record of approval: after the plan is accepted you will set `spec.md` to `status: approved` with `approved-by: <login>, <date>`.

## 2. Explore the code (read-only)

- Read `spec.md`, `intent.md`, `AGENTS.md`, the ADRs the spec links, and the instruction files for the layers you will touch.
- Find the closest existing feature and follow its structure: endpoint style, handler or service layout, validation, mapping, data access, DI registration, test project layout and fixtures.
- Note shared hot spots that other branches may also change: `Program.cs` or DI registration, `Directory.Packages.props`, EF Core migrations and the model snapshot, shared contracts.

## 3. Draft the plan

Use [assets/plan-template.md](assets/plan-template.md).

- **Approach:** 3-6 sentences on how the change fits the architecture and which patterns and ADRs it follows.
- **Files that change:** every file to create or modify, with the reason. If you cannot name the file, you do not know enough yet; explore more.
- **Tasks:** small, ordered steps. Each task:
  - names the requirement IDs it covers,
  - names the files it touches,
  - names the test that will fail first and then prove it (`Method_Scenario_Expected`, tagged `[Trait("Requirement", "<ID>")]`),
  - is marked `[P]` only if it shares no file with other `[P]` tasks.
- Order: shared foundations first (contracts, entities, migrations), then one story at a time in priority order, so P1 is shippable on its own.
- **Coverage table:** every FR and NFR in the spec appears at least once.
- **Proof:** the exact build, test and format commands from `AGENTS.md`, plus feature-specific checks.

Write decisions, not code. Short signatures or a contract snippet are fine; full method bodies are not.

## 4. Interrogate the plan

Answer these in the plan itself (Risks, Alternatives, Migration and rollback):

1. What could this change break, including neighbouring flows?
2. Which step is the riskiest, and how will we know early if it fails?
3. Which options did you consider and not choose, and why?
4. How do we roll back in production?
5. Which EF Core migration, if any, and does it run safely on existing data?

Then ask the engineer to challenge it. Iterate until an engineer who has never seen this conversation could implement the change from the plan alone.

## 5. Self-check

- [ ] Every FR and NFR maps to a task and a test.
- [ ] Every task names its files and its proving test.
- [ ] No placeholders: "TBD", "handle edge cases", "add validation as needed".
- [ ] `[P]` tasks share no files.
- [ ] New NuGet packages are on the approved list (cross-cutting-packages instructions); otherwise they are a flagged risk.
- [ ] Anything that changes the spec's scope goes back to the product owner instead of into the plan.

## 6. Save, commit, hand over

When the engineer accepts (leaving plan mode):

1. Write `docs/features/<id>-<slug>/plan.md` with `status: draft`.
2. Record the spec approval in `spec.md` front matter.
3. Commit and push:

   ```powershell
   git add docs/features/<id>-<slug>/
   git commit -m "docs(plan): plan <slug>" -m "Refs #<id>"
   git push
   ```

4. Tell the user: the engineer approves routine plans; if the PR has `sdlc:high-risk`, the tech lead must approve. The approver adds `sdlc:plan-approved`. Next step: `/sdlc-implement`.
