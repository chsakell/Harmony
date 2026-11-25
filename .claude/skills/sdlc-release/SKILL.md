---
name: sdlc-release
description: Prepares a GitFlow release - release/x.y.z branch from develop, version number from the merged changes, CHANGELOG.md entry written from the merged features' intents, PR to main, tag and back-merge. Use when the user asks to cut, prepare or finish a release or write release notes.
argument-hint: "[x.y.z]"
disable-model-invocation: true
metadata:
  version: "1.0.0"
  source: "Adapted from github/awesome-copilot github-release (MIT). See THIRD_PARTY_NOTICES.md in the dotnet-sdlc kit."
---

# Prepare a release

The agent prepares the release. A human release manager approves the PR to `main` and authorizes the production deployment. The agent never deploys to production.

## 1. Preconditions

```powershell
git fetch origin --tags
git switch develop
git pull --ff-only
gh run list --branch develop --limit 1        # latest CI run must be green
```

Stop if CI on `develop` is red or the working tree is not clean.

## 2. Find what changed

```powershell
$last = git describe --tags --abbrev=0 origin/main
git log "$last..origin/develop" --merges --oneline
git log "$last..origin/develop" --format="%s%n%b"
```

If there is no tag yet, this is the first release: propose `1.0.0` (or `0.1.0` before go-live) and use the whole history.

For each merged feature, read `docs/features/<id>-*/intent.md` (problem and outcome) so the notes describe value to users, not implementation.

## 3. Choose the version

Follow the commit-message standard installed in the repository (git-message instructions):

- any breaking change (`!` in the subject or `BREAKING CHANGES:` in the footer) -> major,
- any `feat` or `fix` -> minor,
- otherwise -> patch.

Show the proposed version and the reason. The user confirms it. If the user passed `x.y.z`, use it and only warn if it contradicts the rule.

## 4. Create the release branch

```powershell
git switch -c release/<x.y.z> origin/develop
```

- Update `CHANGELOG.md` using [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) sections: Added, Changed, Deprecated, Removed, Fixed, Security. Write each line for users, link the work item (`#<id>`).
- If the repository sets `<Version>` in `Directory.Build.props`, update it.
- Show the changelog entry to the user before committing.

```powershell
git add CHANGELOG.md            # and Directory.Build.props if you changed it
git commit -m "build(release): bump version to <x.y.z>"
git push -u origin release/<x.y.z>
gh pr create --base main --title "release: v<x.y.z>" --body-file <changelog entry>
```

Fixes found during stabilisation go to `bugfix/<id>-<slug>` branches that target `release/<x.y.z>`.

## 5. After the release manager merges

```powershell
git fetch origin
git tag -a v<x.y.z> origin/main -m "v<x.y.z>"
git push origin v<x.y.z>
gh release create v<x.y.z> --notes-file <changelog entry> --title "v<x.y.z>"
gh pr create --base develop --head main --title "chore: back-merge v<x.y.z> into develop" --body "Back-merge after release v<x.y.z>."
```

Ask before each push. Production deployment runs through the pipeline's protected environment, where the release manager approves it.
