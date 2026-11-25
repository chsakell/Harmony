---
name: sdlc-review
description: Runs an independent pre-PR review of the branch against REVIEW.md, spec.md, plan.md and the repository standards using the code-reviewer subagent, and handles review comments received on a pull request. Use when about to open or mark a PR ready, after finishing a large task, when the user asks for a review, or when a PR has unresolved review comments or @-mentions to address.
argument-hint: "[self | comments]"
metadata:
  version: "1.0.0"
  source: "Adapted from obra/superpowers requesting-code-review and receiving-code-review (MIT) and the Claude AI-native SDLC playbook (Stage 5: AI in the PR review loop). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Review

Two modes: **self** (review the branch before a human sees it) and **comments** (act on review comments on the PR). Pick the one the user asked for; default to self when there is no PR yet.

## Mode: self

1. Find the feature folder from the branch name (`feature/<id>-<slug>` -> `docs/features/<id>-*/`). For `bugfix/` and `hotfix/` branches there may be none.
2. Compute the range:

   ```powershell
   git fetch origin
   $base = git merge-base origin/develop HEAD     # use origin/main for hotfix/ branches
   $head = git rev-parse HEAD
   ```

3. Dispatch the `code-reviewer` subagent with only these inputs, never the conversation history: the feature folder path, `$base`, `$head`, and the branch type. A fresh context is the point: the reviewer must not share the assumptions that produced the code.
4. Triage what comes back:
   - **Important** - fix now, through the normal test-first loop, then run `/sdlc-verify` again.
   - **Nit** - fix if it takes a minute; otherwise list it in the PR description.
   - **Disagree** - write down why, with evidence; do not silently ignore a finding.
5. If a finding repeats a mistake that was already corrected once before, propose a one-line rule for the "Things agents get wrong" section of `AGENTS.md`.

## Mode: comments

1. Fetch the open comments:

   ```powershell
   gh pr view --comments
   gh api "repos/{owner}/{repo}/pulls/<pr-number>/comments" --jq '.[] | {id, path, line, body, user: .user.login}'
   ```

2. Read all of them before changing anything. If any comment is unclear, ask about it first; related comments often change each other's meaning.
3. For each comment, in order:
   1. Restate the request in your own words.
   2. Check it against the code, the spec and the plan. A reviewer can be wrong; so can you.
   3. Decide: agree (fix it), partly agree (fix the valid part, explain the rest), or disagree (reply with technical reasons and evidence).
   4. Fix one comment at a time, test first, and run the affected tests.
   5. Reply in the thread with what you did:

      ```powershell
      gh api "repos/{owner}/{repo}/pulls/<pr-number>/comments/<comment-id>/replies" -f body="<what changed, or why not>"
      ```

4. No performative agreement ("Great catch!", "You're absolutely right"). State what changed.
5. Before adding a feature a reviewer suggests, check it is needed (search for real callers). If it is not in the spec, it goes back to the product owner, not into this PR.
6. Run `/sdlc-verify`, commit with `#<id>` in the footer, push. Do not resolve threads on the reviewer's behalf.
