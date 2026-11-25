---
name: sdlc-pr
description: Gets a finished branch into a pull request that is ready for review - checks gates and verification evidence, fills the PR template, links the GitHub issue and targets the right GitFlow branch. Use when implementation is verified and the user wants to open, update or mark a PR ready for review, or asks to finish the branch.
argument-hint: "[ready]"
metadata:
  version: "1.0.0"
  source: "Adapted from obra/superpowers finishing-a-development-branch (MIT) and the Claude AI-native SDLC playbook (Stage 5: Deploy). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Open or finish the pull request

## 1. Check the branch and target

| Branch | Target (base) | Artifacts required |
|---|---|---|
| `feature/<id>-<slug>` | `develop` | intent, spec, plan - unless labelled `sdlc:track-s` |
| `bugfix/<id>-<slug>` | `develop` (or `release/x.y.z` during stabilisation) | none; label `sdlc:track-s` |
| `hotfix/<id>-<slug>` | `main` | none; label `sdlc:track-s`; back-merge to `develop` after release |
| `release/x.y.z` | `main` | use `sdlc-release` instead |

Any other branch name: stop and ask the user to rename it to the GitFlow convention.

## 2. Check the gates (features)

For `feature/` branches without `sdlc:track-s`:

- `docs/features/<id>-*/` contains `intent.md`, `spec.md`, `plan.md`, each with `status: approved`.
- The PR has `sdlc:intent-approved`, `sdlc:spec-approved` and `sdlc:plan-approved`.

The required CI check `SDLC gates` enforces the same rules; fix gaps here instead of waiting for CI.

## 3. Check the evidence

If `/sdlc-verify` has not passed in this session since the last code change, run it now. Do not continue on red.

Then run `/sdlc-review` (self mode) if it has not been run since the last code change.

## 4. Bring the branch up to date

```powershell
git fetch origin
git merge origin/develop          # origin/main for hotfix/ branches
```

Merge instead of rebase so reviewers' comments stay attached and no force-push is needed. Re-run `/sdlc-verify` if the merge brought in changes.

## 5. Write the PR description

Fill `.github/pull_request_template.md`:

- **Work item:** `Fixes #<id>` (closes the GitHub issue on merge) or `Refs #<id>` if more PRs will follow.
- **Track** and links to the artifacts.
- **Gates:** who added each label and when (from `gh api "repos/{owner}/{repo}/issues/<pr-number>/events"`).
- **What changed:** short, user-facing.
- **Acceptance criteria to tests** and **Verification evidence:** paste the block from `sdlc-verify`.
- **Deviations from plan:** copy the Deviations log, or "none".
- **Risk and rollout:** from the plan.
- **AI assistance:** which agent was used. The author ticks the box that they reviewed every change; you must not tick it for them.

## 6. Publish

Ask the user before pushing or changing the PR.

```powershell
git push
# Existing draft PR:
gh pr edit --body-file <filled template>
gh pr ready
# No PR yet:
gh pr create --base develop --title "#<id> <title>" --body-file <filled template>
```

Never force-push. Never merge the PR yourself; a code owner approves and merges it.

## 7. After merge

- Delete the branch and any worktree you created for it.
- For `hotfix/` branches, open the back-merge PR from `main` to `develop`.
