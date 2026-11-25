# Feature folders

Each feature (GitHub issue) that is not a small change has one folder here:

```text
docs/features/<issue>-<slug>/
  intent.md   why, and what outcome      - product owner approves   (label sdlc:intent-approved)
  spec.md     what exactly, and the tests - product owner + tech lead (label sdlc:spec-approved)
  plan.md     how, step by step          - engineer, or tech lead if high risk (label sdlc:plan-approved)
```

- `<issue>` is the GitHub issue number, so parallel branches never collide. `<slug>` is 2-5 lowercase words.
- The folder is created on the branch `feature/<issue>-<slug>` and merged to `develop` with the code, in the same pull request.
- `status:` in each file's front matter is `draft`, `approved` or `superseded`. The PR label is the approval record; the front matter mirrors it so agents and hooks can read it offline.
- After the merge the folder is a record of the change. The current state of the system is described in [../architecture/overview.md](../architecture/overview.md) and in the code.
- Small changes (label `sdlc:track-s`) have no folder.

The full process is described in the organization's `dotnet-sdlc.md`.
