---
name: sdlc-intent
description: Captures a new idea, GitHub issue, bug report, incident or scan finding as docs/features/<id>-<slug>/intent.md on a feature branch. Use when someone wants to start new work, describes a problem or an idea, says "capture this", "write an intent", or brings an alert or incident that needs a fix larger than one small PR. DO NOT USE FOR small bug fixes (use sdlc-debug) or for writing the spec (use sdlc-spec).
argument-hint: "[idea text | #<id> | incident reference]"
metadata:
  version: "1.0.0"
  source: "Adapted from obra/superpowers brainstorming (MIT) and the Claude AI-native SDLC playbook (Stage 1: Plan). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Capture intent

Turn what the originator says into `intent.md`: the problem, who is affected, what better looks like, and the limits. Stay in the originator's language. Do not design a solution here.

## 1. Pick the track first

Ask yourself, from what you know so far:

| Track | When | What to do |
|---|---|---|
| S | Bug fix or small change in one component, no API contract or data model change, reviewable in one sitting | Stop. Tell the user to use a `bugfix/<id>-<slug>` branch and the `sdlc-debug` skill. No intent is needed. |
| M | New or changed behaviour users can see, or any API contract, data model or integration change that fits one PR | Continue. |
| L | Several PRs, a new service or product, cross-team impact, or an architecture decision | Continue, and mark the intent as an epic (step 6). |

If unsure, ask the user one question to decide.

## 2. Get the work item ID

The folder and the branch are named after the GitHub issue number: no issue, no folder.

- If the user gave `#<id>` or a number, use it. Read the issue title, body and any acceptance criteria with `gh issue view <id>` (or the GitHub MCP server if available).
- If there is no issue yet, ask the user to create one (or create it with `gh issue create` if the user agrees), then continue.

## 3. Collect the source material

- **Idea:** let the originator describe it in their own words.
- **Work item:** start from its description; treat it as the originator's words.
- **Incident, alert or scan finding:** gather the evidence first (alert text, metric values, timestamps, stack traces, the finding) and quote it. Read the related code read-only to describe the affected systems accurately.

Read `AGENTS.md` and `docs/architecture/overview.md` so you do not ask what the repository already answers.

## 4. Brainstorm, one question at a time

Ask the questions an analyst would ask, one per message, and only when the answer changes what gets built:

1. What can't users do today, and what does it cost? (evidence)
2. Who is affected, and who must change how they work?
3. What does better look like, and how would we measure it?
4. What is explicitly out of scope?
5. Which constraints apply (regulation, security, data, deadline, contract)?
6. Which systems and teams are involved?

If the user starts describing a solution, write down the need behind it and move the solution idea to Open questions as "Proposed approach to evaluate in the spec".

Stop asking when every template section can be filled without guessing. Anything still unknown goes under Open questions with a named person who can answer.

## 5. Create the branch

```powershell
git switch develop
git pull --ff-only
git switch -c feature/<id>-<slug>
```

`<slug>` is 2-5 lowercase words joined by hyphens, taken from the title. Skip this step if the user is already on `feature/<id>-<slug>`.

## 6. Write the file

Copy [assets/intent-template.md](assets/intent-template.md) to `docs/features/<id>-<slug>/intent.md` and fill it in:

- Keep it to one page. Business language only: no class names, endpoints or tables.
- `status: draft`, `created:` today, `source:` the origin of the work.
- For track L add `level: epic` to the front matter, and list the expected features (slices) under Desired outcome.

Show the draft to the user and correct anything they say you misunderstood.

## 7. Commit and open the draft PR

Ask before pushing or creating the PR.

```powershell
git add docs/features/<id>-<slug>/intent.md
git commit -m "docs(intent): capture <slug>" -m "Refs #<id>"
git push -u origin feature/<id>-<slug>
gh pr create --draft --base develop --title "#<id> <title>" --body-file .github/pull_request_template.md
```

## 8. Hand over

Tell the user:

- The product owner reviews `intent.md` in the draft PR and adds the label `sdlc:intent-approved` when it is right.
- If the work touches security, personal data, payments, a public API contract or a data migration, add `sdlc:high-risk` so the tech lead approves every gate.
- Next step after approval: `/sdlc-spec`.
