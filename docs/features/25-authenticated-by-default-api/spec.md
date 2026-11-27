---
id: 25
title: Authenticated-by-default API
artifact: spec
status: approved         # draft | approved | superseded
owner: chsakell
approved-by: chsakell, 2026-11-27
created: 2026-11-27
intent: ./intent.md
---

# Spec: Authenticated-by-default API

## Summary

The core API stops trusting anonymous callers. Every endpoint needs a logged-in user unless it is one of the six deliberately public endpoints (login, token refresh, registration, email confirmation, forgot password, reset password). Role and user administration also need the matching administration permission, which the seeded Administrator role holds. The two "read by user ID" endpoints become "read my own" endpoints that take the user from the login. The test endpoint is removed, and the API explorer is served only in Development. The authorization matrix from R-04 proves it: its anonymous known-gap count drops from 94 to 0, and a new admin rule proves that a logged-in user without the permission is refused. Health probes and the other services' gRPC calls keep working without a login until R-09. See [intent.md](./intent.md).

## Definitions

Terms from [spec #22](../22-api-authorization-matrix/spec.md) (Definitions) keep their meaning: **Harmony Api**, **Api test command**, **test host**, **harness**, **endpoint**, **other endpoints**, **matrix file**, **scope**, **row**, **row outcome** (refused, let in, inconclusive), **known gap**, **reaches the endpoint**, **owner** and **non-member**.

| Term | Meaning |
|---|---|
| **Requirement IDs** | This spec numbers from FR-101 and NFR-101, so that every ID is unique inside the Api test project, where spec #22 already uses FR-001 to FR-050 (decided by chsakell, 2026-11-27). FR-0nn in this spec always means spec #22's requirement. |
| **Matrix vocabulary** | A **line** is one endpoint's entry in the matrix file. A **rule** is one check applied to lines: the **anonymous rule** (spec #22 FR-020, every line), the **non-member rule** (spec #22 FR-030, `resource` lines only) and the new **admin rule** (`administration` lines only). A **row** is one rule's check of one line, sent as one **matrix request**. A line's result for a rule is written as `refused`, `-` (rule does not apply) or a **known-gap mark** such as `gap R-07`. A **baseline** is the recorded count of known-gap marks for one rule (spec #22 FR-042). "The matrix shall fail" means a matrix test fails with a message that names the line or endpoint. |
| **Recording publisher** | The harness's stand-in for `INotificationsPublisher`, which records every message the Api publishes (spec #22 FR-006). |
| **Login** | A bearer token that the Harmony Api accepts: issued by `POST api/identity/token` or `POST api/identity/token/refresh`, or by the harness with the same claims (FR-140). **Logged-in user** and **anonymous** (no `Authorization` header) follow from it. The user is identified by the token's `ClaimTypes.NameIdentifier` claim (the **name-identifier claim**). |
| **Rejected login** | A token that is present but not accepted. An expired token gets 401 (FR-108). A token with a wrong signature or a malformed one gets 500 from the authentication-failed handler today, which R-11 owns. |
| **Result envelope** | The JSON body `Result` serializes to, with the fields `Succeeded` (boolean) and `Messages` (list of strings). |
| **Public endpoint** | One of the six endpoints listed in Design, *Public endpoints*. They are the only matrix lines with scope `public`. |
| **Login-required default** | The rule that any endpoint the Harmony Api routes needs a login unless it is explicitly marked anonymous. It also covers endpoints added later, whatever kind they are (controller action, minimal API, Razor page). |
| **Explicitly anonymous** | An endpoint that the code marks as reachable without a login. After this feature that is exactly: the six public endpoints, `/healthz/ready`, `/healthz/live`, `/Error`, and every method of the six gRPC services. |
| **The six gRPC services** | `CardService`, `UserCardService`, `BoardService`, `WorkspaceService`, `UserService` and `UserNotificationService`, defined by the protos in `src/Services/Harmony.Api/Protos/`. Their methods read boards, cards, users and workspaces. Three of them change data: `MoveCardToList`, `SyncParentStoryPoints` and `AddUserCard`. None of them reads or changes roles, permissions or role membership. |
| **Error page** | The Razor page at `/Error`, which the exception handler re-runs outside Development. Its HTML contains `An error occurred while processing your request.` |
| **Permission** | One of the values of `Permissions.GetRegisteredPermissions()` (`src/Shared/Harmony.Shared/Constants/Permission/Permissions.cs`), for example `Permissions.Roles.View`. A user holds a permission when one of their roles has it as a claim of type `ApplicationClaimTypes.Permission` (the **permission claim**). The login carries the permission claims of the user's roles. |
| **Required permission** | The permission that an administration endpoint's matrix line names, as listed in Design, *Administration permissions*. Each line names exactly one. For `POST api/identity/role` it is `Permissions.Roles.Create`, and FR-114 covers the extra permission that a rename needs. |
| **Administrator** | A user in the `Administrator` role. The roles seeder gives that role every permission at each startup, so an administrator holds every required permission. Endpoint access is decided by permissions only. The existing service checks that protect the `Administrator` and `Basic` roles themselves stay (Design, *Administration permissions*). |
| **Plain user** | A logged-in user whose only role is the one a newly registered user gets (`Basic`), which holds no permission. The non-member from spec #22 is a plain user. |
| **Custom role** | A role other than `Administrator` and `Basic`, created by the harness for a test. |
| **Admin rule** | A new matrix rule for endpoints with scope `administration` (FR-110 to FR-113). |
| **API explorer** | The Swagger UI at `/swagger/index.html` (`/swagger` redirects there) and its OpenAPI document at `/swagger/v1/swagger.json`. |
| **Development** | The ASP.NET Core environment name `Development`. The test host runs as `Testing` (spec #22, NFR-003). |
| **Smoke run** | `dotnet run --project src/Aspire/Harmony.AppHost` on a developer machine, with the checks in NFR-102. Its result is recorded in the pull request. The Harmony Api listens there on its launchSettings https port, `https://localhost:7097`. |
| **Seeded basic user** | `john@harmony.com`, created by the roles seeder with the default password in `UserConstants.DefaultPassword` and the `Basic` role. |
| **Trusted-client header** | The `harmony-trusted-client` request header that Automations and Notifications send to claim a service identity (R-09). |

## User stories

### US1 - Anonymous callers are refused everywhere except the public endpoints (P1)

As a self-hosting team, I want every core API endpoint to need a login unless it is deliberately public, so that a server reachable from the internet cannot be read or changed without an account.

- **Why this priority:** it closes 94 open endpoints, including role and user administration (the takeover path), with one rule. On its own it is the useful slice: anonymous takeover is no longer possible.
- **Independent test:** run the Api test command filtered to each of FR-101 to FR-108. Every anonymous row of the matrix is refused, the anonymous baseline is 0, and the health checks and gRPC services still answer without a login.

### US2 - Administration needs an administration permission (P1)

As an administrator, I want role and user administration to need the matching permission, so that a logged-in user without it cannot make themselves an administrator or change other users' roles.

- **Why this priority:** after US1, any registered user can still administer the server. Registration is public, so US1 alone moves the takeover from "no account" to "any account".
- **Independent test:** run the Api test command filtered to each of FR-110 to FR-114 and FR-140. A plain user is refused with 403 on all 8 administration endpoints, and a user holding only the required permission is let in.

### US3 - A user reads only their own account and notification settings (P2)

As an end user, I want my account details and notification settings to be readable only by me, so that someone who knows my user ID cannot read them.

- **Why this priority:** after US1 only logged-in users can reach the two endpoints, so the exposure is smaller than US2's. It changes a public contract and the client.
- **Independent test:** run the Api test command filtered to FR-120 to FR-122, then open the account page in the smoke run.

### US4 - Leftovers are removed (P3)

As a maintainer, I want the test endpoint gone and the API explorer limited to Development, so that production serves no debug endpoint and no map of the API.

- **Why this priority:** after US1 both need a login, so the remaining risk is small. They shrink the attack surface further.
- **Independent test:** run the Api test command filtered to FR-130 and FR-131, and check FR-132 in the smoke run.

## Functional requirements

| ID | Requirement | Story |
|---|---|---|
| FR-101 | When a request with no `Authorization` header is sent to an endpoint whose scope is not public, the Harmony Api shall respond 401. | US1 |
| FR-102 | The Harmony Api shall apply the login-required default to every endpoint it routes that is not explicitly anonymous, including endpoints that carry no authorization attribute. | US1 |
| FR-103 | When a request with no `Authorization` header is sent to one of the six public endpoints, the Harmony Api shall respond with a status other than 401 or 403. | US1 |
| FR-104 | When a request with no `Authorization` header is sent to `/healthz/ready` or `/healthz/live`, the Harmony Api shall respond as it does without the login-required default: 200 while its checks are healthy. | US1 |
| FR-105 | When a call with no `Authorization` header is made to one method of each of the six gRPC services, the Harmony Api shall not refuse it for missing authentication: the call ends neither with gRPC status `Unauthenticated` nor with HTTP 401. | US1 |
| FR-106 | If an unhandled exception occurs while the Harmony Api serves a request with no `Authorization` header outside Development, then the Harmony Api shall respond 500 with the error page, not 401. | US1 |
| FR-107 | If a matrix line has scope `public` and its endpoint is not one of the six public endpoints, or one of the six has a scope other than `public`, then the matrix shall fail and name the line. | US1 |
| FR-108 | If a request carries an expired login, then the Harmony Api shall respond 401 on any endpoint that is not explicitly anonymous. | US1 |
| FR-109 | When a logged-in request is sent to a route that matches no endpoint, the Harmony Api shall respond 404. The Api serves no `index.html`, so its fallback endpoint answers 404 for every HTTP method. | US1 |
| FR-110 | When a plain user sends a request to an endpoint whose scope is administration, the Harmony Api shall respond 403. | US2 |
| FR-111 | When a logged-in user who holds every permission except an administration endpoint's required permission sends a request to it, the Harmony Api shall respond 403. | US2 |
| FR-112 | When a logged-in user who holds only an administration endpoint's required permission sends a request to it, the Harmony Api shall respond with a status other than 401 or 403. A 200 with `Succeeded` false, a 400 or a 5xx counts as let in, because authorization ran first. If the request does not reach the endpoint, then the row shall fail as inconclusive (spec #22, FR-015). | US2 |
| FR-113 | If a matrix line with scope administration names no required permission or a value that is not a permission, if a line with another scope names a required permission, or if an administration line carries a known-gap mark for any rule, then the matrix shall fail and name the line. | US2 |
| FR-114 | If a request to `POST api/identity/role` carries a non-empty role `Id` (a rename) and the caller does not hold both `Permissions.Roles.Create` and `Permissions.Roles.Edit`, then the Harmony Api shall respond 403 with the forbidden body of NFR-106 and leave the role unchanged. | US2 |
| FR-120 | When a logged-in user sends `GET api/identity/account`, the Harmony Api shall return that user's account details, identified by the login's name-identifier claim. | US3 |
| FR-121 | When a logged-in user sends `GET api/UserNotifications`, the Harmony Api shall return that user's notification settings, identified by the login's name-identifier claim. | US3 |
| FR-122 | When a logged-in request is sent to `GET api/identity/user/{id}` or `GET api/UserNotifications/{userId}`, the Harmony Api shall respond 404 and return no user's data. After this feature, the only REST endpoints that take another user's ID are `administration` and `resource` endpoints. | US3 |
| FR-130 | The Harmony Api shall not serve `POST api/Test`. A logged-in request to it gets 404 (FR-109) and publishes no message. | US4 |
| FR-131 | While the environment is not Development, the Harmony Api shall not serve the API explorer: requests to `/swagger/index.html` and `/swagger/v1/swagger.json` get no 2xx response and no OpenAPI document, with or without a login. | US4 |
| FR-132 | While the environment is Development, the Harmony Api shall serve the API explorer at `/swagger` as it does today. | US4 |
| FR-140 | When a test asks for a login for a user it created, the harness shall issue a token that also carries the permission claims of the user's roles, as `POST api/identity/token` does. This extends spec #22, FR-007. | US2 |

### Acceptance scenarios

All scenarios run on the test host with the Api test command unless they say otherwise. "Proven during verification, not committed" means the check is made locally once and recorded in the pull request, as in spec #22.

- **FR-101**
  - Given the matrix file after this feature, when the Api test command runs, then every anonymous row is refused, no line carries `gap R-07`, and the anonymous baseline is 0. These rows carry both spec #22's FR-020 tag and FR-101.
  - Given `DELETE api/identity/role/{id}` and `PUT api/identity/user/roles/{id}`, when they are called with no `Authorization` header, then each gets 401 and no role or user changes.
- **FR-102**
  - Committed: the anonymous rule covers every endpoint the test host routes today (FR-101). A test confirms that the test host's authorization fallback policy requires an authenticated user, so routed endpoints without an attribute are covered. Spec #22's FR-013 makes any new non-controller endpoint fail the matrix until it is reviewed.
  - Proven during verification, not committed: a controller action with no authorization attribute and a minimal API endpoint (`MapGet`), each added locally, get 401 with no `Authorization` header.
- **FR-103** - Given each of the six public endpoints, when it is called with no `Authorization` header, then it gets neither 401 nor 403. `POST api/identity/token` with the credentials of a user the harness created returns a token. Spec #22's FR-021 rows carry FR-103 as well.
- **FR-104** - Given the test host, when `GET /healthz/ready` and `GET /healthz/live` are sent with no `Authorization` header, then both return 200.
- **FR-105** - Given each of the six gRPC services, when one read method is called through a gRPC client over the test host with no `Authorization` header, then the call ends neither with status `Unauthenticated` nor with HTTP 401. Any other status, including `NotFound` for unknown IDs, passes. The plan picks the method per service.
- **FR-106** - Given the test host (environment `Testing`), when `GET api/identity/user/confirm-email?userId=<random>&code=x` is sent with no `Authorization` header for a user that does not exist, then the response is 500 and its body contains the error page's text, not 401. This trigger relies on today's unhandled exception for an unknown user. If R-24 makes that endpoint answer 404, the test moves to another public request that throws, or to a test-only throwing endpoint, and that change is recorded in the R-24 plan.
- **FR-107** - Given the scope of `POST api/identity/token` changed locally to `self`, or a seventh line changed to `public`, then the matrix fails and names the line. Proven during verification, not committed.
- **FR-108** - Given a harness token for the owner that expired one minute ago, when `GET api/board/userboards` is sent with it, then the response is 401 with the 401 body of NFR-106 ("The Token is expired." is acceptable as the message).
- **FR-109** - Given the owner's login, when `GET api/nothing-here` and `POST api/nothing-here` are sent, then both get 404. With no `Authorization` header, both get 401.
- **FR-110** - Given the non-member (a plain user), when they send each of the 8 administration endpoints' matrix request, then each gets 403. Today each is let in. There is no known-gap mark for this rule (FR-113). The non-member rule of spec #22 still applies only to `resource` lines, so each administration line is checked by the anonymous rule and the admin rule only.
- **FR-111** - Given `DELETE api/identity/role/{id}` and a user holding every permission except `Permissions.Roles.Delete`, then the request gets 403. The same holds for each administration endpoint and its required permission.
- **FR-112**
  - Given `GET api/identity/role` and a user holding only `Permissions.Roles.View`, then the request does not get 401 or 403.
  - Given a user the harness puts in the `Administrator` role, when they log in through `POST api/identity/token` (not a harness token) and call `GET api/identity/user`, then the response is 200 with the list of users.
- **FR-113** - Given an administration line with its required permission removed, or with a misspelled permission, locally, then the matrix fails and names the line. Proven during verification, not committed.
- **FR-114** - Given a custom role, when a user holding only `Permissions.Roles.Create`, or only `Permissions.Roles.Edit`, sends `POST api/identity/role` with that role's `Id` and a new name, then the response is 403 with the forbidden body and the role keeps its name. With both permissions, the role is renamed. These requests come on top of the admin rule's 24 rows.
- **FR-120**
  - Given two users A and B, when A sends `GET api/identity/account` with A's login, then the response is 200 and holds A's ID, email, first and last name, and no field of B.
  - Given no `Authorization` header, then `GET api/identity/account` gets 401 (anonymous row).
  - A login whose user was deleted after the token was issued keeps today's behaviour of the underlying query. This feature does not specify it (R-24).
- **FR-121** - Given users A and B with different notification settings, when A sends `GET api/UserNotifications`, then the response holds A's settings only.
- **FR-122**
  - Given A's login and B's user ID, when A sends `GET api/identity/user/<B's ID>` or `GET api/UserNotifications/<B's ID>`, then each gets 404 and the body holds none of B's data.
  - Given the matrix file after this feature, then no line has scope `user`.
- **FR-130**
  - Given the owner's login, when `POST api/Test` is sent, then the response is 404 and the recording publisher holds no message.
  - Given the matrix file after this feature, then it has no line for `POST api/Test`.
- **FR-131** - Given the test host, when `GET /swagger/index.html` and `GET /swagger/v1/swagger.json` are sent with no `Authorization` header and with the owner's login, then none of the four responses is 2xx or contains `"openapi"`.
- **FR-132** - Given the smoke run (Development), when the Harmony Api's `https://localhost:7097/swagger/v1/swagger.json` is requested with no `Authorization` header, then it returns 200 with an OpenAPI document. Proven during verification, not committed: the test host never runs as Development.
- **FR-140** - Given a user the harness puts in the `Administrator` role, then the harness token and a token from `POST api/identity/token` for the same user carry the same set of permission claim values.

## Non-functional requirements

| ID | Category | Requirement (measurable) |
|---|---|---|
| NFR-101 | Security | After this feature, these are the only things that answer a request with no `Authorization` header with a status other than 401: <br>- **Routed endpoints:** the 6 public endpoints, the 2 health checks, `/Error`, and the methods of the 6 gRPC services (until R-09). The matrix (FR-101, FR-103) and spec #22's FR-013 prove this list exactly. <br>- **Middleware before routing:** CORS preflight (`OPTIONS` with an `Origin` header, answered by the CORS middleware); files under `/Files` (R-13) and any file in the Api's web root (static files); and, in Development only, the API explorer (FR-132). The overview records this list. It is not tested exhaustively. |
| NFR-102 | Compatibility | The smoke run reaches `Distributed application started`, and `https://localhost:7108/healthz/ready` (gateway) returns 200. Then, through the client at https://localhost:7096: <br>- the seeded basic user logs in, opens a board, opens the account page and sees their name and email, changes one notification setting, reloads, and sees the change; <br>- creating a card on that board shows up in a second browser tab without a reload (SignalR). <br>Over the whole run, the Aspire dashboard shows no gRPC call from Automations or Notifications to Harmony Api ending in `Unauthenticated` or HTTP 401. This exercises only the gRPC calls that these steps trigger. FR-105 covers all six services automatically. The result is recorded in the pull request. |
| NFR-103 | Build | `dotnet build Harmony.slnx` reports 0 errors and no more than 585 code warnings (`WARNING_BASELINE`). `dotnet format Harmony.slnx --verify-no-changes --include <changed files>` exits 0. |
| NFR-104 | Tests | The test command, the integration test command and the Api test command pass. No test is skipped, and no test is deleted to make a run pass. These existing tests change on purpose: <br>- the anonymous `_KnownIssue` rows become refused rows (spec #22 FR-041 requires it); <br>- anonymous and public rows gain the FR-101 and FR-103 tags; <br>- the rows of the 3 removed endpoints go with their matrix lines; <br>- the publisher test of Flagged concern 6 moves to another endpoint. <br>Any other change to an existing test is a deviation recorded in `plan.md`. |
| NFR-105 | Performance | The Api test command still finishes its run in at most 5 minutes on `ubuntu-latest`, as spec #22 NFR-001 requires, with the admin rule's rows added. |
| NFR-106 | Error contract | Refusals keep today's response bodies, the result envelope with `Succeeded` false: <br>- a 401 has `Messages` `["You are not Authorized."]` from the bearer challenge handler, or `["The Token is expired."]` for an expired login; <br>- a 403 has `["You are not authorized to access this resource."]` from the bearer forbidden handler, also when the action itself refuses (FR-114). <br>R-24 owns any change to that shape. A test asserts each of the three bodies once. |
| NFR-107 | Documentation | Updated in this pull request: <br>**`docs/architecture/overview.md`**: Cross-cutting concerns (authentication: login-required default, explicitly anonymous list, administration permissions); Known issues (94 anonymous gaps closed; the gRPC services stay anonymous until R-09; the API explorer only in Development, with the Compose and Kubernetes caveat of Flagged concern 1). <br>**`docs/roadmap.md`**: R-07 status and notes; R-06 note (Flagged concern 9); the new item from Flagged concern 1. <br>**AGENTS.md**: the Api test command's filter note names the admin rule's requirement IDs. <br>**`harmony-docs` GitBook**: no change. A search on 2026-11-27 found no page about the API explorer, roles or user administration. |

## Design

The authorization model behind this design is recorded in [ADR 2026-11-27-api-authorization-model](../../adr/2026-11-27-api-authorization-model.md) (accepted). It covers the login-required default, anonymous access only by explicit marker, and administration by permission policy.

### Components affected

See the [overview](../../architecture/overview.md).

- **Harmony Api (`src/Services/Harmony.Api`):**
  - **Startup:** the authorization setup gains a fallback policy that requires an authenticated user (the login-required default, FR-102). The health checks, the `/Error` page and the six gRPC services are marked anonymous where they are mapped (FR-104 to FR-106). The `index.html` fallback is not marked, so an anonymous request to an unknown route gets 401. The API explorer middleware runs only in Development (FR-131, FR-132). `AddSwaggerGen` may stay registered.
  - **Internal callers:** Automations and Notifications reach the Harmony Api only through the six gRPC services. A search of `src/` on 2026-11-27 found no REST call from another service to the Api: the Api's named HTTP client calls Automations, not the other way round. So the login-required default does not affect them.
  - **Identity controllers:** the two token actions get an explicit anonymous marker (spec #22, Design, *Public endpoints*, and its FR-022 already requires it for public lines). Role and user administration actions require their permission policy (Design, *Administration permissions*). The policies already exist: one per permission, registered in `AddJwtAuthentication`. `GET api/identity/user/{id}` is removed, and `GET api/identity/account` is added to the account controller, which already has `[Authorize]` and holds the other self actions.
  - **User notifications controller:** `GET api/UserNotifications/{userId}` becomes `GET api/UserNotifications`. Its handler already reads the user from the login and ignores the ID.
  - **Test controller:** removed.
- **Harmony.Application:** the query behind `GET api/identity/account` reads the user from the login. The unused `GetUserQuery` handler already does this and can be reused (plan decides). `GetUserNotificationsQuery` loses its unused `UserId`.
- **Blazor client (`Harmony.Client`, `Harmony.Client.Infrastructure`):** the user and notification managers and their routes call the two new endpoints without an ID. The account page and the main layout stop passing the current user's ID. No visible change (UX notes).
- **Api tests (`tests/Harmony.Api.IntegrationTests`):**
  - **Matrix file:** all anonymous marks become `refused` and the anonymous baseline becomes 0. The lines for `POST api/Test`, `GET api/identity/user/{id}` and `GET api/UserNotifications/{userId}` are removed, and `GET api/identity/account` and `GET api/UserNotifications` are added with scope `self`. Administration lines gain a non-admin result and their required permission. The plan picks the column format, for example a `Non-admin` column and a `Permission` binding, and updates the legend.
  - **Admin rule:** three requests per administration endpoint: a plain user (FR-110), all-but-required (FR-111), and only-required (FR-112). Each is one test result named with the rule, method and route, as in spec #22 FR-014. Users with the needed permissions are created per run through a role the harness defines.
  - **Harness tokens** carry permission claims (FR-140).
  - **Other tests:** gRPC, health, error-page and API explorer checks (FR-104 to FR-106, FR-131), and the own-data and removal checks (FR-120 to FR-122, FR-130).
  - **Changed test:** spec #22's FR-006 publisher test uses `POST api/Test` and moves to another endpoint (Flagged concern 6).
- **Not changed:** the gateway, Automations, Notifications, SignalR, SourceControl, the data model, the gRPC contracts.

### Login-required default and the explicitly anonymous list

| Endpoint | Anonymous after this feature | Why |
|---|---|---|
| The six public endpoints | allowed | Login, refresh, registration, email confirmation and password reset need no account. |
| `/healthz/ready`, `/healthz/live` | allowed | Probes from Aspire, Compose and Kubernetes send no token. |
| `/Error` | allowed | The exception handler re-runs the request at `/Error`. If that page needed a login, an anonymous request that fails would get 401 instead of 500 (FR-106). |
| The six gRPC services | allowed, **R-09 gap** | Automations and Notifications call them without a login. R-09 adds service authentication. The overview records the gap. |
| Static files, `/Files` included | allowed | Served by middleware before authorization. R-13 owns `/Files`. |
| The `index.html` fallback | **login required** | The Api has no `index.html`, so the fallback answers 404 to a logged-in caller (FR-109). Unknown routes no longer answer anonymously. Spec #22's "reaches the endpoint" check does not depend on the status. |
| Everything else, now and later | **login required** | The login-required default. |

The gateway forwards `/core/{anything}` to the Api unchanged, so the same rules apply through `https://localhost:7108/core/...`.

### Public endpoints

Unchanged from spec #22, Design, *Public endpoints*: `POST api/identity/token`, `POST api/identity/token/refresh`, `POST api/identity/user`, `GET api/identity/user/confirm-email`, `POST api/identity/user/forgot-password`, `POST api/identity/user/reset-password`.

### Administration permissions

"Administrator" means holding the area's permission, not the role name (Open questions, Q1). The policies, the role claims and the client's menu checks already use these values.

| Endpoint | Required permission |
|---|---|
| `GET api/identity/role` | `Permissions.Roles.View` |
| `POST api/identity/role` | `Permissions.Roles.Create`; with a non-empty `Id` (rename) also `Permissions.Roles.Edit` (FR-114) |
| `DELETE api/identity/role/{id}` | `Permissions.Roles.Delete` |
| `GET api/identity/role/permissions/{roleId}` | `Permissions.RoleClaims.View` |
| `PUT api/identity/role/permissions/update` | `Permissions.RoleClaims.Edit` |
| `GET api/identity/user` | `Permissions.Users.View` |
| `GET api/identity/user/roles/{id}` | `Permissions.Users.View` |
| `PUT api/identity/user/roles/{id}` | `Permissions.Users.Edit` |

The matrix request carries `{}` as the `POST api/identity/role` body, so its rows test `Permissions.Roles.Create`. FR-114 covers the rename path. The rename check reads the body, so the action refuses through the framework's forbid result, which writes the same 403 body (NFR-106).

The existing service checks stay: only an administrator can add or remove the `Administrator` role, and the `Administrator` and `Basic` roles cannot be renamed. They answer 200 with `Succeeded` false, after authorization. An only-required row that hits one of them is still let in (FR-112). The matrix requests use random IDs and `{}` bodies, so they do not touch the seeded roles.

### Expected sizes

Read from the code on 2026-11-27. The implementation records the exact numbers and updates this table if one differs.

| Scope | Before | After | Change |
|---|---|---|---|
| public | 6 | 6 | - |
| self | 10 | 11 | `POST api/Test` removed; `GET api/identity/account` and `GET api/UserNotifications` added |
| administration | 8 | 8 | - |
| user | 2 | 0 | both replaced by the self endpoints above |
| resource | 80 | 80 | - |
| **Total** | **106** | **105** | |

- **Anonymous known gaps:** 94 before, 0 after. 99 anonymous rows are refused.
- **Non-member known gaps:** 80, unchanged (R-08).
- **Admin rule:** 24 rows (8 endpoints × 3 requests), no known gaps.

### API contract

| Endpoint | Before | After |
|---|---|---|
| `GET api/identity/account` | - | **New.** 200 with the caller's `Result<UserResponse>`, the same shape `GET api/identity/user/{id}` returned. 401 when anonymous. |
| `GET api/identity/user/{id}` | 200 for anyone, any user | **Removed.** 404 for a logged-in caller, 401 when anonymous. |
| `GET api/UserNotifications` | - | **New.** 200 with the caller's `Result<List<EmailNotificationType>>`, the same shape as before. 401 when anonymous. |
| `GET api/UserNotifications/{userId}` | 200 for anyone (the ID was already ignored) | **Removed.** 404 for a logged-in caller, 401 when anonymous. |
| `POST api/Test` | 200 for anyone, publishes an email notification | **Removed.** 404 for a logged-in caller, 401 when anonymous. |
| The 8 administration endpoints | open to anyone | 401 when anonymous; 403 without the required permission; otherwise unchanged. |
| The 6 public endpoints | open | unchanged |
| Every other endpoint | open to anyone, or `[Authorize]` | 401 when anonymous; otherwise unchanged |
| `/swagger`, `/swagger/v1/swagger.json` | served in every environment | served only in Development |

- **Status codes:** 401 for no login or an expired login (FR-108); 403 for a missing permission. Bodies are today's result envelopes (NFR-106). A wrongly signed or malformed token still gets 500 from the authentication-failed handler, which R-11 owns (Definitions, *Rejected login*).
- **Versioning:** none. The API has no versioning, and the client changes in the same pull request. Scripts that call the API anonymously must log in first (intent, *Who is affected*).

### Data model

No change. The permissions are existing role claims. The roles seeder gives every permission to the `Administrator` role at each startup, so existing installations need no migration and no backfill.

### Events and integrations

- **Broker:** no change. `POST api/Test` no longer publishes its hard-coded email notification.
- **gRPC:** no contract change. The six services stay reachable without a login (FR-105) until R-09.
- **Gateway:** no change. Its aggregated Swagger UI can no longer load the core API's document outside Development (Flagged concern 2).

### UX notes

- **No visible change.** The account page, the main layout (name and avatar) and the notification settings page show the same data. They now load it without sending the user's ID.
- **Administration screens:** none exist, as the intent says. The client's navigation menu computes `Permissions.Roles.View` and `Permissions.Users.View` flags but shows no item with them. No page or layout calls the administration endpoints: the role manager is injected into the main layout but never used (read on 2026-11-27). The server now enforces the same permission values that those flags would use.
- **Expired session:** unchanged. A 401 sends the user to the login page as it does today.

## Standards and policies applied

- **No `dotnet-instructions` standard is installed** (AGENTS.md), and no security, data-classification or UX policy skill is available in this session. The spec follows the ADRs, spec #22 and the surrounding code.
- **Roadmap R-07:** a fallback policy, an "administrator permission", `[AllowAnonymous]` on the two token endpoints, the admin rule (spec #22, Flagged concern 5).
- **[Spec #22](../22-api-authorization-matrix/spec.md):** the matrix rules, row outcomes and baselines. Its FR-022 forbids an anonymous marker on a non-public line, and its FR-013 keeps the list of other endpoints fixed. Refusals are status codes, never 200 with `Succeeded=false`.
- **[Testing strategy ADR](../../adr/2026-11-26-testing-strategy.md):** no skipped tests. The anonymous `_KnownIssue` rows become ordinary refused rows on purpose (rule 7).
- **[Traceability ADR](../../adr/2026-11-26-test-traceability-tags.md):** every new test carries a `Requirement` tag. IDs start at FR-101 so they are unique in the Api test project.
- **[Microservices ADR](../../adr/2026-11-25-microservices-grpc-and-rabbitmq.md):** gRPC stays the internal query path and keeps working (FR-105). The gateway forwards, and the Api enforces (roadmap R-07 note).
- **[Aspire ADR](../../adr/2026-11-25-aspire-local-orchestration.md):** the smoke run (NFR-102).
- **AGENTS.md:** warning baseline, format rule, all test commands, "fix the code, not the test" (NFR-103, NFR-104, Flagged concern 6).

## Flagged concerns

| # | Concern | Owner | Resolution |
|---|---|---|---|
| 1 | `docker-compose.yml` and `.k8s/base/kustomization.yaml` set `ASPNETCORE_ENVIRONMENT=Development` for every service. A Development-only API explorer therefore stays on for self-hosters who deploy with Compose or Kubernetes. The same setting also skips the exception handler and HSTS there. | chsakell | Decided by chsakell, 2026-11-27: Development only, no setting. Add a roadmap item, "Run Compose and Kubernetes deployments as Production", in this PR, and note the caveat in the overview. |
| 2 | The gateway's aggregated Swagger (MMLib.SwaggerForOcelot) reads `swagger/v1/swagger.json` from the Api. Outside Development the core API's section will fail to load. The gateway is out of scope. | chsakell | Accepted as proposed at spec approval (chsakell, 2026-11-27): accept, and note it under the new roadmap item from concern 1. |
| 3 | The six gRPC services stay anonymous behind an explicit marker until R-09. Anyone who reaches the Api's own port can call them. They can read users (with emails), boards, cards and workspaces, and they can move a card, sync story points and add a card member. None of them touches roles, permissions or role membership, so the takeover path through administration stays closed (Definitions, *The six gRPC services*). The gateway forwards `/core/{anything}` over HTTP/1.1, which gRPC cannot use, so they can be reached only on the Api's port (inferred - confirm). | chsakell (tech lead) | Accepted as proposed at spec approval (chsakell, 2026-11-27): accept as the intent's constraint. Record it in the overview's Known issues, and R-09 removes the markers. |
| 4 | Permissions travel in the login. A user who loses a permission keeps it until their token expires or is refreshed. Access tokens live two days today. | chsakell | Accepted as proposed at spec approval (chsakell, 2026-11-27): accept. R-11 shortens token lifetimes. Note it in R-11. |
| 5 | A user with `Permissions.Users.Edit` who is not an administrator can give themselves any role except `Administrator`, including a custom role with more permissions. Today only the `Administrator` role holds permissions, so the risk exists only once an administrator delegates. | chsakell | Accepted as proposed at spec approval (chsakell, 2026-11-27): accept, and record it in the overview. A stricter rule (granting only roles whose permissions you hold) would be a new item. |
| 6 | Spec #22's FR-006 scenario and its test `StandInTests.TestEndpoint_OwnerPost_RecordsEmailNotification` use `POST api/Test`, which this feature removes. The test must move to another endpoint that publishes an email notification. This is a deliberate change to an approved spec's scenario, not a fix to make a failing test pass. | chsakell | Accepted as proposed at spec approval (chsakell, 2026-11-27): amend spec #22's FR-006 scenario in this PR with a dated note. The plan picks the endpoint. |
| 7 | Removing `GET api/identity/user/{id}` and `GET api/UserNotifications/{userId}` breaks any outside script that uses them, with no deprecation period. A browser that still runs a cached old client gets 404 on the account page until it reloads. | chsakell (product owner) | Accepted as proposed at spec approval (chsakell, 2026-11-27): accept. The intent marks the contract change as intended, and client and API deploy together. Mention it in the PR's upgrade notes. |
| 8 | "Authenticated by default, anonymous only by explicit marker, administration by permission policy" is a lasting authorization model that R-08 to R-11 build on. No ADR records it. | chsakell (tech lead) | Written: [ADR 2026-11-27-api-authorization-model](../../adr/2026-11-27-api-authorization-model.md), linked from Design. Resolved: the ADR is `status: accepted` (chsakell, 2026-11-27). |
| 9 | R-06 plans an architecture rule, "every controller action has `[Authorize]` or `[AllowAnonymous]`". With the login-required default that rule is redundant, and it would push attributes back onto every action. | chsakell | Accepted as proposed at spec approval (chsakell, 2026-11-27): update R-06's note in this PR. R-06 should check instead that the fallback policy exists and that the only anonymous markers are on the explicitly anonymous list. |
| 10 | Requirement IDs in one test project now come from two specs. | chsakell | Decided by chsakell, 2026-11-27: this spec numbers from FR-101 and NFR-101. No ADR change. |
| 11 | The intent's constraint "prove every change with a failing matrix row first" fits only FR-101 and FR-110 to FR-113. FR-104 to FR-109, FR-114, FR-120 to FR-122, FR-130 and FR-131 are separate Api tests. FR-102, FR-107, FR-113 and FR-132 are partly proven during verification only. | chsakell | Accepted as proposed at spec approval (chsakell, 2026-11-27): each committed requirement gets a failing test before its code. The plan orders tasks that way and lists the verification-only checks for the PR. |

**Before approval:** the ADR of concern 8 must be accepted, and the product owner and the tech lead record a decision on each open row (2 to 7, 9 and 11). Accepting a proposal is a decision.

## Open questions

| Question (from intent or new) | Answer, or "carried forward" | Owner |
|---|---|---|
| Q1. What counts as "administrator": the Administrator role, or the per-area permissions? | **The per-area permissions** (Design, *Administration permissions*). Evidence: roadmap R-07 asks for "an administrator permission". `AddJwtAuthentication` already registers one policy per permission. The roles seeder gives the `Administrator` role every permission, and the client's menus check `Permissions.Roles.View` and `Permissions.Users.View`. Checking the role name would make the role-permission screen meaningless. **Assumed:** `GET api/identity/user/roles/{id}` needs `Permissions.Users.View`, not `Permissions.Roles.View`. | chsakell, at spec review |
| Q2. Drop the user ID from the account and notification reads, and take the user from the login? | **Yes.** The new routes are `GET api/identity/account` (the account controller already holds the self actions and has `[Authorize]`) and `GET api/UserNotifications`. Evidence: both client callers pass the current user's own ID, `GetUserNotificationsHandler` already ignores it, and the unused `GetUserHandler` already reads the user from the login. | - |
| Q3. How do the internal gRPC services and health probes stay reachable under a login-required default? | **Explicit anonymous markers** on the health checks, `/Error` and the six gRPC services (Design, *Login-required default*). The gRPC marker is a recorded R-09 gap (Flagged concern 3). | - |
| Q4. Do self-hosting teams need a setting to turn the API explorer on outside Development? | **No:** Development only, no setting. Decided by chsakell, 2026-11-27, with Flagged concern 1. | chsakell, 2026-11-27 |
| Q5 (new). How does the matrix file show the admin rule and the required permission? | Carried forward: the column format, chosen in the plan. The content is fixed by FR-110 to FR-113. | chsakell, in the plan |

## Out of scope

- Board, card, list and workspace membership and board roles (R-08). The non-member baseline stays 80.
- Authentication between services and on the gRPC services (R-09), and the trusted-client header.
- Login on Automations, SourceControl, SignalR and the gateway (R-10), and the gateway's aggregated Swagger (Flagged concern 2).
- Token lifetimes, signing key, password rules, the seeded administrator account, and the 500 for a wrongly signed token (R-11).
- File upload safety and anonymous `/Files` (R-13); the response shape of errors (R-24).
- The ignored `{userId}` in `POST api/identity/account/profile-picture/{userId}`, and route IDs that differ from body IDs (R-24).
- Changing the environment of the Compose and Kubernetes deployments (new roadmap item, Flagged concern 1).
- New client screens for role or user administration.

## Rollout

1. **One pull request** (this feature) changes the Api, the client, the tests and the docs together. Client and API are always deployed together, so no compatibility period is needed (Flagged concern 7).
2. **No feature flag and no migration.** The permissions already exist as role claims, and the seeder grants them to `Administrator` at every startup.
3. **Upgrade notes** in the pull request description, for self-hosters:
   - every API call except the six public ones needs a login;
   - administration needs the matching permission, which the `Administrator` role has;
   - the two read-by-ID endpoints and `POST api/Test` are gone;
   - the API explorer is served only in Development, which Compose and Kubernetes still use (Flagged concern 1).
4. **Rollback:** revert the pull request. Nothing is stored, so nothing needs undoing.
