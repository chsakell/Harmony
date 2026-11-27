---
id: 25
title: Authenticated-by-default API
artifact: intent
status: draft            # draft | approved | superseded
owner: chsakell
approved-by:             # <name>, <YYYY-MM-DD> - filled in when the sdlc:intent-approved label is added
created: 2026-11-27
source: "#25 (roadmap R-07)"
---

# Intent: Authenticated-by-default API

> One page at most. Business language only: say what is wrong and what "better" looks like, not how to build it.

## Problem

Anyone who can reach a Harmony server can use most of its core API without logging in.

- **Open by default.** The core API has no "login required unless stated" rule. Of its 106 endpoints, 6 are public on purpose and only a few others ask for a login. The authorization matrix from R-04 records **94 endpoints that let an anonymous caller in** today.
- **Administration is open to everyone.** An anonymous caller can list all users, create and delete roles, change role permissions and change a user's roles, which includes making themselves an administrator. A self-hosted server reachable from the internet can be taken over with no account.
- **Other users' data by ID.** Anyone can read another user's account details or notification settings if they know the user's ID. The client never needs this: it always asks for the logged-in user's own data.
- **Leftovers that widen the attack surface.** A test endpoint that sends a hard-coded email notification is open to anyone. The API explorer, which lists every endpoint, is served in every environment, production included.

Evidence: roadmap R-07; `tests/Harmony.Api.IntegrationTests/Authorization/authorization-matrix.md` (94 `gap R-07` marks); `docs/architecture/overview.md` (Known issues).

## Who is affected

- **Self-hosting teams:** their server stops accepting anonymous changes to users, roles and data. They lose the API explorer outside development.
- **Administrators:** still manage users and roles. No client screen calls the administration endpoints today, so nothing visible changes.
- **End users:** no visible change. Login, registration, email confirmation, password reset and token refresh work as before. Their own account page and settings keep working.
- **Scripts or integrations that call the API anonymously:** they must log in first. This is intended.
- **Maintainers and reviewers:** every new endpoint needs a login unless someone deliberately marks it public, and the matrix proves that.

## Desired outcome

- Every core API endpoint requires a logged-in user unless it is deliberately public. Today that means login, token refresh, registration, email confirmation, forgot password and reset password.
- Role and user administration require an administrator. A logged-in user without that right is refused.
- A user can read only their own account and notification settings. The server takes the user from the login, not from an ID the caller sends.
- The test endpoint is gone, and the API explorer is available only in development.
- Health probes and the internal calls the other Harmony services make to the core API keep working as they do today.

### Success measures

- The matrix's anonymous known-gap count drops from 94 to 0, and the Api tests pass.
- A new **admin rule** in the matrix proves that a logged-in non-administrator is refused on every administration endpoint.
- The full application still starts with `dotnet run --project src/Aspire/Harmony.AppHost`, and a user can log in, open a board and see their account page.
- Build, unit, integration and Api tests pass, with no code warnings above the baseline.

## Scope

- **In scope:** the login-required default for the core API's REST endpoints; the public-endpoint list; the administrator requirement for role and user administration; own-data-only account and notification reads; removing the test endpoint; limiting the API explorer to development; the matrix, `docs/architecture/overview.md`, `docs/roadmap.md` and the GitBook pages that describe these.
- **Out of scope:**
  - Board, card, list and workspace membership checks and board roles: R-08.
  - Authentication between Harmony services, and the core API's internal gRPC calls: R-09.
  - Login on the Automations, SourceControl and SignalR services, and the gateway: R-10.
  - Token lifetimes, password rules and the seeded administrator account: R-11.
  - File upload safety (R-13) and the shape of error responses (R-24).

## Constraints

- **High-risk:** security change; the tech lead approves every gate (`sdlc:high-risk`).
- **Do not break the internal calls.** The other services call the core API without a user login, and health checks are anonymous. Both must keep working until R-09 secures the internal calls.
- **Contract change:** removing the user ID from the two "own data" endpoints changes the public API. The client changes in the same pull request.
- Prove every change with a failing matrix row first, and never skip a test. Build at 0 errors with no warnings above the baseline, and formatting passes for changed files.

## Affected systems

- The core API: its startup, identity and administration controllers, and the user notifications endpoint.
- The Blazor client: the account page and main layout that read the current user's data.
- The Api integration tests and the authorization matrix.
- Docs: `docs/architecture/overview.md`, `docs/roadmap.md`, and the `harmony-docs` GitBook if it documents the API explorer or user administration.
- Not changed: the gateway, the other services, the data model.

## Open questions

- [ ] What counts as "administrator": the Administrator role, or the existing per-area permissions for users, roles and role claims that the client already checks? - chsakell, in the spec
- [ ] Proposed approach to evaluate in the spec: drop the user ID from the account and notification read endpoints and take the user from the login's name-identifier claim (both client callers already pass their own ID, and the notifications handler already ignores it). - chsakell, in the spec
- [ ] How do the internal gRPC services and health probes stay reachable under a login-required default? Explicitly public for now, with R-09 closing it? - chsakell, in the spec
- [ ] Do self-hosting teams need a setting to turn the API explorer on outside development, or is development-only enough? - chsakell
