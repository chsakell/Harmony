---
name: sdlc-adr
description: Records an architecture decision as docs/adr/YYYY-MM-DD-<slug>.md, or supersedes an existing one. Use when a spec or plan needs a decision that outlives the feature - architecture style, data store, messaging, authentication, hosting, a new cross-cutting library, a public contract change - or when the user asks to write, update or supersede an ADR.
argument-hint: "[decision title]"
metadata:
  version: "1.0.0"
  source: "Adapted from github/awesome-copilot create-architectural-decision-record (MIT). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Record an architecture decision

An ADR explains **why** the system is the way it is. It is short, permanent, and never rewritten: a changed decision gets a new ADR that supersedes the old one.

## When an ADR is needed

Write one when the decision:

- is hard or expensive to reverse,
- affects more than one feature or team,
- chooses between real alternatives (framework, store, pattern, protocol, hosting),
- deviates from an installed standard or an earlier ADR.

Do not write one for choices that only affect a single feature's internals; those go in `plan.md` under Alternatives considered.

## Steps

1. **Collect the inputs.** You need: context, the decision, at least two alternatives with their trade-offs, and the deciders. Ask for anything missing, one question at a time.
2. **Check what exists.** Search `docs/adr/` for ADRs on the same topic. If one is being replaced, this ADR supersedes it.
3. **Check the standards.** If the decision departs from an installed instruction file, say so in Context and explain why.
4. **Write the file.** Copy [assets/adr-template.md](assets/adr-template.md) to `docs/adr/<YYYY-MM-DD>-<slug>.md` (today's date). Dated names never collide between parallel branches. Keep it to one page. Set `status: proposed` and `work-item:` when there is one.
5. **Link it.** Reference the ADR from the spec's Design section or the plan's Approach, and from `docs/architecture/overview.md` (Decision index) if the architecture changes.
6. **Supersede, don't edit.** When replacing an ADR, set the old one to `status: superseded` and `superseded-by: <new file>`, and set `supersedes:` in the new one. Do not change the old decision text.
7. **Commit** on the current feature branch:

   ```powershell
   git add docs/adr/ docs/architecture/overview.md
   git commit -m "docs(adr): <decision title>" -m "Refs #<id>"
   ```

## Acceptance

The architects (code owners of `docs/adr/`) approve the ADR in the PR. When they approve, set `status: accepted` before the PR merges, so the merged file states the decision in force. A rejected ADR stays in the folder with `status: rejected`, so the reasoning is not lost.
