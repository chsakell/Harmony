---
name: sdlc-spec
description: Turns an approved intent.md into a requirements and design spec.md with EARS requirements, acceptance scenarios, non-functional requirements, API and data contracts, applied standards and flagged concerns. Use when an intent has the sdlc:intent-approved label, or the user asks to write, refine or update a spec, requirements or design for a feature. DO NOT USE FOR implementation plans (use sdlc-plan) or capturing a new idea (use sdlc-intent).
argument-hint: "[docs/features/<id>-<slug>]"
metadata:
  version: "1.0.0"
  source: "Adapted from github/spec-kit specify and clarify (MIT), github/awesome-copilot create-specification (MIT), EARS (Mavin et al.) and the Claude AI-native SDLC playbook (Stage 2: Design). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Write the spec

The spec says **what** will be delivered and **how we will know it works**, constrained by the organization's standards and policies. The product owner reviews it but does not have to write it. Flag every concern you cannot resolve instead of hiding it.

## 1. Find the feature and check the gate

1. Take the work item ID from the branch name `feature/<id>-<slug>`, or from the argument. The folder is `docs/features/<id>-*/`. If it is missing, stop and suggest `/sdlc-intent`.
2. Check that the intent is approved: the PR has the label `sdlc:intent-approved`, or `intent.md` already says `status: approved`.

   ```powershell
   gh pr view --json number,labels
   gh api "repos/{owner}/{repo}/issues/<pr-number>/events" --jq '.[] | select(.event=="labeled" and .label.name=="sdlc:intent-approved") | "\(.actor.login), \(.created_at[0:10])"'
   ```

   If there is no approval, stop and tell the user the product owner must approve the intent first.
3. Record the approval in `intent.md` front matter: `status: approved`, `approved-by: <login>, <date>`. If `gh` is not available, ask the user who approved it and when.

## 2. Read before writing

Read, in this order, and keep notes of what constrains the spec:

1. `intent.md` - the problem, outcome, constraints and open questions.
2. `AGENTS.md` - the architecture map and the list of installed standards.
3. `docs/architecture/overview.md` and the ADRs in `docs/adr/` that touch the affected components.
4. The installed instruction files that apply to this change (`.github/instructions/` or `.claude/rules/`): API style, validation, data access, auth, caching, messaging, observability, testing. Read them on purpose; they are not loaded automatically while you write markdown.
5. Any policy skills available in this session (security, data classification, UX, brand). Apply each one that matches.
6. The code around the change, read-only. Look for an existing feature that solves a similar problem and reuse its shape. For broad searches use an exploration subagent so your context stays small.

## 3. Resolve the open questions

For each open question in `intent.md`:

- Answer it from evidence (code, standards, ADRs) and say where the answer came from, or
- Ask the user, one question at a time, offering 2-3 options with your recommendation, or
- Make an informed assumption, write it under Open questions as "Assumed: ...", and carry it forward.

Leave at most three questions marked "carried forward". Prioritize: scope, then security and privacy, then user experience, then technical detail.

## 4. Draft spec.md

Copy [assets/spec-template.md](assets/spec-template.md) to `docs/features/<id>-<slug>/spec.md` and fill it in.

- **User stories** are prioritized (P1, P2, ...) and each can be shipped and tested on its own. P1 alone must be a useful slice.
- **Functional requirements** are EARS statements with IDs `FR-001`, `FR-002`, ... See [references/ears.md](references/ears.md). One behaviour per requirement. Each has at least one Given/When/Then scenario.
- **Non-functional requirements** (`NFR-001`, ...) are measurable: numbers, limits, named standards. Never "fast", "secure", "user-friendly" or "scalable" without a number or a rule.
- **Design** covers contracts and boundaries: components, API shape and status codes, data model changes, events, UX. No file lists or class-by-class steps; those belong to the plan.
- **Standards and policies applied** names each instruction file or skill that constrained the spec and what it changed.
- Keep the intent's language for the problem; do not rewrite the business case.

## 5. Flag concerns

Add a row to **Flagged concerns** for every case where:

- the intent conflicts with a policy, a standard or an ADR,
- two policies contradict each other,
- the change needs data, access or a decision another team owns,
- a security, privacy or compliance risk remains,
- the existing architecture cannot support the request without a new decision.

Name the owner who must decide. If the decision changes the architecture, suggest `/sdlc-adr` and link the ADR from the Design section.

## 6. Reader test

Start a fresh subagent that sees only `intent.md` and `spec.md` (not this conversation). Ask it to list:

- requirements that are ambiguous, untestable or contradict each other,
- intent questions or success measures the spec does not address,
- terms used without definition.

Fix what it finds, then do a last self-check:

- [ ] Every intent success measure maps to at least one FR or NFR.
- [ ] Every FR and NFR has an ID and is testable.
- [ ] No vague adjectives without numbers.
- [ ] At most three questions carried forward.
- [ ] Flagged concerns each have an owner.

## 7. Commit and hand over

```powershell
git add docs/features/<id>-<slug>/
git commit -m "docs(spec): specify <slug>" -m "Refs #<id>"
git push
```

Tell the user:

- The product owner and the tech lead review `spec.md` in the PR, resolve the flagged concerns with their owners, and add `sdlc:spec-approved`.
- Suggest `sdlc:high-risk` if the spec touches security, personal data, payments, a public contract or a data migration.
- Next step after approval: `/sdlc-plan`.

If the spec has to change after it was approved, set it back to `status: draft`, ask the approver to remove and re-add the label, and note the change in the PR.
