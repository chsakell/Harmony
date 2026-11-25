---
name: sdlc-setup-repo
description: Prepares a repository for the AI-native .NET SDLC - checks the starter kit, GitFlow branches, sdlc labels and installed standards, fills in AGENTS.md from verified commands, and drafts docs/architecture/overview.md. Use when adopting the SDLC in an existing (brownfield) codebase or starting a new (greenfield) product, once per repository, or when AGENTS.md or the architecture overview is missing or stale.
argument-hint: "[brownfield | greenfield]"
disable-model-invocation: true
metadata:
  version: "1.0.0"
  source: "Adapted from the Claude AI-native SDLC playbook (CLAUDE.md /init guidance) and github/spec-kit existing-projects guidance (MIT). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Set up the repository

Decide the mode: **brownfield** if the repository has a solution and code, **greenfield** if it is empty or only has the starter kit. Ask if unclear.

Work on a branch: `feature/<id>-sdlc-setup` from `develop`, labelled `sdlc:track-s`. Ask the user for the work item ID.

## 1. Check the starter kit

These must exist at the repository root (copied from the `starter-kit/` folder of the dotnet-sdlc kit): `AGENTS.md`, `CLAUDE.md`, `REVIEW.md`, `.claude/skills/sdlc-*`, `.claude/agents/`, `.claude/hooks/`, `.claude/settings.json`, `.github/pull_request_template.md`, `.github/CODEOWNERS`, `.github/workflows/sdlc-gates.yml`, `.github/scripts/sdlc-gates.ps1`, `docs/features/README.md`, `docs/adr/README.md`. List anything missing and stop until the user copies it.

## 2. Check Git and GitHub

- `main` and `develop` exist on the remote; `develop` is the default branch for feature PRs. If `develop` is missing, ask before creating it from `main`.
- The labels exist. Ask before creating them:

  ```powershell
  gh label create "sdlc:intent-approved" --color 0E8A16 --description "Product owner approved intent.md" --force
  gh label create "sdlc:spec-approved"   --color 1D76DB --description "Product owner and tech lead approved spec.md" --force
  gh label create "sdlc:plan-approved"   --color 5319E7 --description "Plan approved; implementation may start" --force
  gh label create "sdlc:high-risk"       --color D93F0B --description "Tech lead or architect approves every gate" --force
  gh label create "sdlc:track-s"         --color C5DEF5 --description "Small change: no intent, spec or plan needed" --force
  ```

- Remind the user to configure branch protection on `main` and `develop` (required review from code owners, required status checks including `SDLC gates`, no force-push). The agent cannot and must not change these settings.

## 3. Check the standards

- If `.dotnet-instructions.lock.json` is missing, tell the user to install the organization's .NET standards for both assistants:

  ```powershell
  dotnet-instructions init --provider copilot,claude
  ```

  Brownfield repositories pick static-analysis strictness `adopt`; greenfield picks `strict`.
- The tool writes its index between `<!-- dotnet-instructions:begin -->` and `<!-- dotnet-instructions:end -->` in `.github/copilot-instructions.md` and `CLAUDE.md`. Leave the `@AGENTS.md` line in `CLAUDE.md` above that block.

## 4. Fill in AGENTS.md

Replace every `<...>` placeholder. Keep the file under one page.

- **Commands:** find the solution file and test projects, then run each command and keep only the ones that work. Record the healthy output (for example "0 Warning(s), 0 Error(s)").
- **Architecture:** one line per project or top-level folder - what it owns and what it may depend on. Derive it from the solution, project references and folder layout, not from guesses.
- **Standards:** confirm the paths of the installed instruction files.
- **Things agents get wrong:** leave empty; the team adds lines as mistakes repeat.

## 5a. Brownfield: describe what exists

Draft `docs/architecture/overview.md` from evidence only: the solution and project references, `Program.cs` and DI setup, `appsettings*.json` (keys, not values), Dockerfiles, CI workflows, README. Mark anything inferred with "(inferred - confirm)". Do not invent components or decisions.

- Run the build and tests once. Record failing tests and warning counts under "Known issues" in the overview. Do not fix them in this branch.
- Offer to record the two or three most important existing decisions as ADRs with `sdlc-adr` (status `accepted`, context "recorded after the fact"). Only if the team confirms them.

## 5b. Greenfield: decide before building

1. Start with an epic intent: run `/sdlc-intent` (track L) for the product or service.
2. Run `/sdlc-spec` on it. The epic spec lists the features (slices) and the non-functional requirements for the whole product.
3. Record the foundational decisions with `/sdlc-adr`: architecture style (clean architecture, modular monolith or microservices, as the standards allow), data store and access, authentication, hosting, messaging.
4. Run `dotnet-instructions init` with answers that match those ADRs.
5. Write `docs/architecture/overview.md` from the epic spec and the ADRs.
6. The first feature is a walking skeleton: solution, projects, one health endpoint, test projects, CI. It gets its own feature folder and goes through `/sdlc-plan` and `/sdlc-implement` like any other feature.

## 6. Commit and open the PR

```powershell
git add AGENTS.md CLAUDE.md docs/
git commit -m "docs: set up AI-native SDLC" -m "Refs #<id>"
```

Ask before pushing and opening the PR (label `sdlc:track-s`). The tech leads (code owners of `AGENTS.md`) review it.
