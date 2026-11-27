---
title: The core API requires a login by default, opens endpoints only by explicit marker, and guards administration with permission policies
status: proposed         # proposed | accepted | rejected | superseded | deprecated
date: 2026-11-27
deciders: chsakell
work-item: "#25"
supersedes:
superseded-by:
---

# The core API requires a login by default, opens endpoints only by explicit marker, and guards administration with permission policies

## Context

`Harmony.Api` authenticates JWT bearer tokens but authorizes almost nothing. Only six of its 106 endpoints carry `[Authorize]`, and `AddAuthorization` has no fallback policy. Protection is opt-in, so a new endpoint is open unless someone remembers to close it. The R-04 authorization matrix found 94 endpoints open to anonymous callers. Among them are role and user administration, through which anyone can make themselves an administrator ([spec #22](../features/22-api-authorization-matrix/spec.md), [intent #25](../features/25-authenticated-by-default-api/intent.md)).

The forces:

- **Self-hosted servers face the internet.** The Ocelot gateway forwards `/core/{anything}` without validating tokens, and the Api's port can be reached directly. The Api must enforce access itself ([microservices ADR](2026-11-25-microservices-grpc-and-rabbitmq.md), follow-up "decide whether the gateway should authenticate requests").
- **Internal callers have no login yet.** Automations and Notifications call the Api's six gRPC services without a token, and the health probes send none. Both must keep working until R-09 adds service authentication.
- **A permission model already exists but is not enforced.** `Permissions` defines View, Create, Edit, Delete and other values for Users, Roles and Role Claims. `AddJwtAuthentication` registers one policy per value. Logins carry the permission claims of the user's roles. The roles seeder gives the `Administrator` role every permission. No endpoint uses these policies.
- **R-08 to R-11 build on whatever is chosen here.** R-08 adds board, card and workspace membership checks, R-09 adds service authentication, R-10 adds logins to the other services, and R-11 hardens tokens.

No `dotnet-instructions` standard is installed, so nothing here departs from one.

## Decision

The core API is **authenticated by default**:

1. **Fallback policy.** The authorization fallback policy requires an authenticated user. It applies to every endpoint the Api routes that has no authorization metadata of its own: controller actions, minimal APIs, Razor pages and gRPC services, including endpoints added later.
2. **Anonymous only by explicit marker.** An endpoint is reachable without a login only if the code marks it anonymous where it is declared or mapped. The allowed list is closed and reviewed:
   - the six public identity endpoints (log in, refresh a token, register, confirm an email address, start and finish a password reset);
   - the health checks and the `/Error` page;
   - the six gRPC services, as a recorded gap until R-09 removes the marker.

   The authorization matrix (`tests/Harmony.Api.IntegrationTests/Authorization/authorization-matrix.md`) is the published list of scopes. Adding a public endpoint, or any other anonymous marker, is a security decision reviewed in its pull request.
3. **Administration by permission, not by role name.** Each role and user administration endpoint requires the policy of one value from `Permissions`, for example `Permissions.Roles.Delete`. The `Administrator` role has access because it holds every permission, not because of its name. A rule that depends on the request body (renaming a role needs `Permissions.Roles.Edit` as well) refuses through the framework's forbid result, so every refusal has the same 403 body.
4. **The server decides whose data it is.** A "self" endpoint takes the user from the login's name-identifier claim and never from an ID that the caller sends.
5. **Status codes.** A missing or expired login gets 401, and a missing permission gets 403. Membership refusals added by R-08 use 403 or 404 (spec #22, Flagged concern 4).

The API explorer (Swagger) is middleware, not an endpoint. It is served only in the Development environment.

## Alternatives considered

| Option | Pros | Cons | Why not chosen |
|---|---|---|---|
| `[Authorize]` on every controller, or on a shared base controller | Explicit at each controller; no global behaviour | Still opt-in: a controller that does not derive from the base class, a minimal API or a Razor page ships open. R-06 would need an architecture test to catch it. | Fails open. That is how the 94 gaps arose. |
| A global MVC `AuthorizeFilter` | One line; covers every controller action | Covers MVC only. Minimal APIs, Razor pages and gRPC stay open without any marker, so the gRPC gap would be invisible in code. | Narrower than the fallback policy, and it hides the gaps it does not cover. |
| Enforce authentication at the Ocelot gateway | One place for every service | The Api's port can be reached directly, and gRPC does not pass through the gateway. Per-endpoint permissions still need the Api. Gateway changes are R-10. | Not enough on its own. It may be added later for defence in depth. |
| Administration by role name (`[Authorize(Roles = "Administrator")]`) | Simplest; no mapping table | Ignores the existing permission policies and the role-permission screen, and cannot delegate one area (for example user administration) without granting everything. | It discards a permission model that already exists and is seeded. |
| One coarse "administration" permission for every administration endpoint | Fewer values to manage | `Permissions` already has per-action values, and the client checks them. Merging them would need data changes. | Per-action policies cost nothing more and are already registered. |

## Consequences

- **Positive:**
  - New endpoints of any kind are closed until someone opens them on purpose, and the matrix shows every opening.
  - The anonymous takeover through role and user administration is closed.
  - Administration can be delegated per area by giving a role selected permissions.
  - R-08 to R-11 start from one model with fixed 401 and 403 meanings.
- **Negative:**
  - The six gRPC services stay anonymous behind an explicit marker until R-09. They read users (emails included), boards, cards and workspaces, and they can move cards, sync story points and add card members.
  - Permissions travel inside the login. Removing a permission takes effect only when the token expires or is refreshed (two days today, R-11).
  - A user who can edit users can give themselves any non-administrator role, including one with more permissions. Only the `Administrator` role is protected by a service check.
  - Unknown routes answer 401 to anonymous callers instead of 404.
  - The roadmap's R-06 rule "every controller action has `[Authorize]` or `[AllowAnonymous]`" becomes redundant. R-06 should check instead that the fallback policy exists and that every anonymous marker is on the allowed list.
- **Follow-up work:**
  - R-09 removes the gRPC marker.
  - R-10 decides whether Automations, SourceControl and SignalR adopt the same model.
  - R-11 shortens token lifetimes.
  - R-06's rule is rewritten as above.
  - The roadmap item to run the Compose and Kubernetes deployments as Production (spec #25, Flagged concern 1), so that the Development-only API explorer is also off there.
