---
name: sdlc-write-skill
description: Guides a policy owner or engineer through turning a standard, policy or repeated procedure into a tested Agent Skill (SKILL.md) that works in GitHub Copilot and Claude Code. Use when someone wants to create, change or review a skill, or asks where a piece of team knowledge should live (AGENTS.md, instruction file, skill or hook).
argument-hint: "[policy or procedure name]"
metadata:
  version: "1.0.0"
  source: "Adapted from anthropics/skills skill-creator (Apache-2.0), obra/superpowers writing-skills (MIT) and the Claude AI-native SDLC playbook (Skills as institutional knowledge). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Write a skill

A skill makes the agent apply a piece of institutional knowledge consistently, at the moment it is needed. It is advisory: anything that must always hold also needs a deterministic check behind it.

## 1. Is a skill the right place?

| Knowledge | Where it goes |
|---|---|
| Facts about this repository: commands, layout, things agents get wrong | `AGENTS.md` (under one page) |
| Coding rules for a file type or layer ("Minimal APIs return ProblemDetails") | An instruction file in the standards repository, installed by `dotnet-instructions` |
| A procedure, or a policy applied to a task ("how we review an API for security", "how we classify data in a spec") | A skill |
| A rule that must never be broken | A hook, analyzer, banned-API entry or CI check - plus the skill, so violations are rare before they are blocked |
| Review-only rules | `REVIEW.md` |

If it is not a skill, stop and point the user to the right place.

## 2. Start from one policy with an owner

Name the policy owner and the written source of truth (document, standard, regulation). The skill is a faithful, actionable rendering of that source; the owner signs off on it.

## 3. Write the evaluation first

1. Write three realistic prompts where the skill should change the agent's behaviour, and what good behaviour looks like for each.
2. Run them without the skill and note what the agent gets wrong, word for word. These failures are what the skill must fix.
3. Add each prompt as an eval case in `evals/cases/` (see `evals/README.md`) so changes to the skill are regression-tested.

## 4. Write SKILL.md

Copy [assets/skill-template.md](assets/skill-template.md) to `.claude/skills/<name>/SKILL.md`.

**Front matter**

- `name`: lowercase letters, digits and hyphens, 1-64 characters, no leading, trailing or double hyphen, and exactly the folder name. An invalid name makes VS Code skip the skill silently.
- `description`: up to 1024 characters, third person. Say what it does, then "Use when ..." with the words a user would actually type and the situations that should trigger it. Add "DO NOT USE FOR ..." when a neighbouring skill overlaps. Do not summarize the steps: agents follow a summary instead of reading the body.
- `disable-model-invocation: true` only for skills that must run on explicit request (slash command), such as releases.
- `metadata.version` and `metadata.owner`.

**Body**

- Under 500 lines. Assume the model is capable; cut explanations it does not need.
- Numbered steps and checklists. One recommended way, not a menu.
- Explain why a rule exists when it is not obvious; reserve "never" and "must" for real hard rules.
- Concrete examples of good output.
- Long reference material goes in `references/<topic>.md`, one level deep, linked from the body.
- Deterministic checks go in `scripts/` (PowerShell, run with `pwsh -File`), with the body saying when to run them and what output to include.
- Templates go in `assets/`.

## 5. Test that it triggers

In both VS Code (Copilot) and Claude Code:

- Run about ten prompts that should load the skill, phrased differently, and about ten near-misses that should not.
- Run the three eval prompts with the skill and compare with the baseline from step 3.
- If it under-triggers, add the missing trigger words to the description. If it over-triggers, add a "DO NOT USE FOR" clause.

## 6. Ship it

- Open a PR. Add the policy owner as code owner of `.claude/skills/<name>/` in `.github/CODEOWNERS`.
- The agent-evals workflow runs on changes to `.claude/**`; a drop in pass rate blocks the merge until reviewed.
- When the policy changes, change the skill in the same way and bump `metadata.version`. Track how often PR review findings still cite the policy: they should trend towards zero.

See [references/example-policy-skill.md](references/example-policy-skill.md) for a complete example.
