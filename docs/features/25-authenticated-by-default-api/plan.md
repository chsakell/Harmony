---
id: 25
title: Authenticated-by-default API
artifact: plan
status: approved         # draft | approved | superseded
owner: chsakell
approved-by: chsakell, 2026-11-27   # <name>, <YYYY-MM-DD> - filled in when the sdlc:plan-approved label is added
created: 2026-11-27
spec: ./spec.md
---

# Plan: Authenticated-by-default API

## Approach

The Api gets an authorization **fallback policy** that requires an authenticated user, registered in `AddJwtAuthentication` next to the existing per-permission policies ([ADR 2026-11-27-api-authorization-model](../../adr/2026-11-27-api-authorization-model.md), rules 1-2). The explicitly anonymous list is marked where each item is declared or mapped. The two token actions get `[AllowAnonymous]`, the `/Error` page model gets `[AllowAnonymous]`, and the two health checks and six gRPC services get `.AllowAnonymous()` in `Program.cs`. Administration actions get `[Authorize(Policy = Permissions.X.Y)]` with the policies that already exist (ADR rule 3). The role-rename rule checks `Permissions.Roles.Edit` with `IAuthorizationService` and returns `Forbid()`, so its 403 has the same body. The two "own data" reads take the user from the login through the existing MediatR queries (ADR rule 4), and the client changes in the same pull request. Swagger moves ahead of routing and then to Development only, because a fallback policy also covers requests that no endpoint matches. Every change is proven in the R-04 Api test project ([spec #22](../22-api-authorization-matrix/spec.md), [plan #22](../22-api-authorization-matrix/plan.md)): the matrix gains a `Non-admin` column and a `permission=` binding, and new Api tests cover the rest. Tests come first, following the [testing strategy ADR](../../adr/2026-11-26-testing-strategy.md) and [traceability ADR](../../adr/2026-11-26-test-traceability-tags.md).

### Decisions on the spec's open points

| Point | Decision |
|---|---|
| Q5: the matrix format for the admin rule | A new **`Non-admin`** column between `Non-member` and `Bindings`. It reads `refused` on `administration` lines and `-` on all others. A known-gap mark there is an error, because an administration endpoint ships closed (FR-113). Each administration line names its required permission as a **`permission=<value>`** binding, for example `permission=Permissions.Roles.View`. The parser stores it in `MatrixLine.Permission`, not in `Bindings`, so the request builder never sees it. The header becomes `\| Route \| Method \| Scope \| Anonymous \| Non-member \| Non-admin \| Bindings \|` (7 cells). No baseline line is added, because the admin rule has no known gaps. This matches the spec's example ("a `Non-admin` column and a `Permission` binding"). |
| FR-105: the gRPC method per service | `BoardService/GetBoard`, `CardService/GetCard`, `UserCardService/GetUserCard`, `WorkspaceService/GetWorkspace`, `UserService/GetUser`, `UserNotificationService/GetUsersForNotificationType`, all with new random IDs. The calls go through `Grpc.Net.Client` (already a direct package of `Harmony.Api`, so it flows to the test project) over the test server's handler. Each call is a hand-built `Method<TRequest, TResponse>` that uses the Api's public generated message types (`Harmony.Api.Protos.*.Parser`). No stubs are generated in the test project: they would duplicate the Api's `Harmony.Api.Protos` types and add CS0436 warnings. |
| The query behind `GET api/identity/account` | Reuse `GetUserQuery`/`GetUserHandler`, which nothing uses today and which already reads `ICurrentUserService.UserId`. Its unused `UserId` property is removed, so the query cannot carry an ID (ADR rule 4). `GetUserNotificationsQuery` loses `UserId` and its constructor parameter. |
| Where the client's "my account" call lives | `IAccountManager.GetAsync()` → `AccountEndpoints.Get` (`{CoreApiPrefix}/identity/account`), next to the other account calls. `IUserManager.GetAsync(string)` and `UserEndpoints.Get(string)` are removed. All three managers are injected globally in `_Imports.razor`, so no injection changes. |
| The `/Error` marker | `[AllowAnonymous]` on `ErrorModel`, not `MapRazorPages().AllowAnonymous()`, so that any Razor page added later needs a login. |
| Swagger placement | T4 moves `app.ConfigureSwagger()` before `app.UseRouting()`, still in every environment, so that the fallback policy does not hide it. T7 then wraps it in the existing `IsDevelopment()` branch. Each story stays red-green on its own. |
| Users with chosen permissions (admin rule, FR-114) | `PermissionUsers` creates, once per run and per permission set, a custom role (`matrix-<suffix>`) with those permission claims and a user in that role only. It caches the result per host, the way `MatrixRows.Discovery` does. The plain user is the existing `api.NonMember` (`Basic`, no permissions). |
| FR-107 and FR-113 | Proven by committed pure tests on synthetic matrix text (`MatrixCheckTests`, `MatrixFileTests`) as well as the local edits the spec lists for verification. This adds proof and does not change scope. |
| The flagged concerns that were still open at spec approval (2-7, 9, 11) | Recorded in `spec.md` as accepted as proposed when the spec approval is written down (see Hand-over). The plan carries out each proposal: concern 6 → T7, concern 9 → T8, concern 11 → the task order, concerns 2-5 and 7 → T8 docs and the PR's upgrade notes. |

## Files that change

| File | Change | Why |
|---|---|---|
| `src/Services/Harmony.Api/Extensions/ServiceCollectionExtensions.cs` | modify | `AddJwtAuthentication`: `options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()` (FR-102) |
| `src/Services/Harmony.Api/Program.cs` | modify | `.AllowAnonymous()` on both `MapHealthChecks` and the six `MapGrpcService` calls (FR-104, FR-105). `ConfigureSwagger()` moves before `UseRouting()` (T4), then into the Development branch (T7, FR-131, FR-132). |
| `src/Services/Harmony.Api/Pages/Error.cshtml.cs` | modify | `[AllowAnonymous]` (FR-106) |
| `src/Services/Harmony.Api/Controllers/Identity/TokenController.cs` | modify | `[AllowAnonymous]` on `Get` and `Refresh` (FR-103; spec #22 FR-022) |
| `src/Services/Harmony.Api/Controllers/Identity/RoleController.cs` | modify | Policies on its 5 actions. Injects `IAuthorizationService`; `Post` with a non-empty `Id` also needs `Permissions.Roles.Edit`, otherwise `Forbid()` (FR-110 to FR-112, FR-114) |
| `src/Services/Harmony.Api/Controllers/Identity/UserController.cs` | modify | Policies on `GetAll`, `GetRolesAsync` and `UpdateRolesAsync`; `GetById` removed (FR-110 to FR-112, FR-122) |
| `src/Services/Harmony.Api/Controllers/Identity/AccountController.cs` | modify | New `[HttpGet] Get()` that sends `GetUserQuery` (FR-120) |
| `src/Services/Harmony.Api/Controllers/Management/UserNotificationsController.cs` | modify | `[HttpGet("{userId}")] Get(string)` becomes `[HttpGet] Get()` (FR-121, FR-122) |
| `src/Services/Harmony.Api/Controllers/TestController.cs` | delete | FR-130 |
| `src/Core/Harmony.Application/Features/Users/Queries/GetUser/GetUserQuery.cs` | modify | Remove the unused `UserId` (ADR rule 4) |
| `src/Core/Harmony.Application/Features/Users/Queries/GetUserNotifications/GetUserNotificationsQuery.cs` | modify | Remove `UserId` and its constructor parameter (spec, Components) |
| `src/Web/Harmony.Client.Infrastructure/Routes/AccountEndpoints.cs` | modify | Add `Get` |
| `src/Web/Harmony.Client.Infrastructure/Routes/UserEndpoints.cs` | modify | Remove `Get(string)` |
| `src/Web/Harmony.Client.Infrastructure/Routes/UserNotificationsEndpoints.cs` | modify | Remove `User(string)`; GET uses `Index` |
| `src/Web/Harmony.Client.Infrastructure/Managers/Identity/Account/IAccountManager.cs`, `AccountManager.cs` | modify | Add `Task<IResult<UserResponse>> GetAsync()` |
| `src/Web/Harmony.Client.Infrastructure/Managers/Identity/Users/IUserManager.cs`, `UserManager.cs` | modify | Remove `GetAsync(string)` |
| `src/Web/Harmony.Client.Infrastructure/Managers/Project/IUserNotificationManager.cs`, `UserNotificationManager.cs` | modify | `GetNotificationsAsync()` without a user ID |
| `src/Web/Harmony.Client/Pages/Identity/Account.razor.cs` | modify | Calls `_accountManager.GetAsync()` and `GetNotificationsAsync()`, and no longer reads the ID claim |
| `src/Web/Harmony.Client/Shared/MainBody.razor.cs` | modify | Calls `_accountManager.GetAsync()`; `CurrentUserId` is still read from the claims for its other uses |
| `tests/Harmony.Api.IntegrationTests/Authorization/authorization-matrix.md` | modify | `Non-admin` column and `permission=` bindings (T2); every anonymous mark `refused`, baseline 0 (T4); the user-scope lines replaced (T6); `api/Test` removed (T7); legend |
| `tests/Harmony.Api.IntegrationTests/Authorization/MatrixFile.cs` | modify | 7-cell header, `NonAdmin` mark, `Permission` binding, the FR-113 rules |
| `tests/Harmony.Api.IntegrationTests/Authorization/MatrixCheck.cs` | modify | FR-107: the public lines are exactly the six public endpoints |
| `tests/Harmony.Api.IntegrationTests/Authorization/RowVerdict.cs` | modify | `Rule.Admin`, `AdminCaller`, `JudgeAdmin` |
| `tests/Harmony.Api.IntegrationTests/Authorization/MatrixRows.cs` | modify | Three admin row sources and their display names |
| `tests/Harmony.Api.IntegrationTests/Authorization/AnonymousMatrixTests.cs` | modify | FR-101 and FR-103 tags (NFR-104) |
| `tests/Harmony.Api.IntegrationTests/Authorization/AdminMatrixTests.cs` | new | The admin rule: 3 theories × 8 lines (FR-110 to FR-112) |
| `tests/Harmony.Api.IntegrationTests/Authorization/LoginRequiredTests.cs` | new | FR-101 (takeover), FR-102, FR-103 (login works), FR-108, FR-109, NFR-106 (401 bodies) |
| `tests/Harmony.Api.IntegrationTests/Authorization/ExplicitlyAnonymousTests.cs` | new | FR-104, FR-106 |
| `tests/Harmony.Api.IntegrationTests/Authorization/GrpcAnonymousTests.cs` | new | FR-105 |
| `tests/Harmony.Api.IntegrationTests/Authorization/AdministrationTests.cs` | new | FR-112 (an administrator logs in through the endpoint), FR-114, NFR-106 (403 body) |
| `tests/Harmony.Api.IntegrationTests/Authorization/AttackSurfaceTests.cs` | new | FR-130, FR-131 |
| `tests/Harmony.Api.IntegrationTests/Identity/OwnDataTests.cs` | new | FR-120 to FR-122 |
| `tests/Harmony.Api.IntegrationTests/Authorization/MatrixFileTests.cs`, `MatrixCheckTests.cs`, `RowVerdictTests.cs`, `MatrixRowsTests.cs` | modify | New pure tests. The format change and the gap count of 0 change existing ones (*Existing tests that change*). |
| `tests/Harmony.Api.IntegrationTests/Harness/TestUsers.cs` | modify | `TestUser` gains `Password` and `Permissions`. `CreateAsync(services, label, params string[] roles)` defaults to `Basic` and reads the roles' permission claims through `RoleManager`. |
| `tests/Harmony.Api.IntegrationTests/Harness/TestTokens.cs` | modify | Adds one `ApplicationClaimTypes.Permission` claim per permission (FR-140), and an optional `expires` for FR-108 |
| `tests/Harmony.Api.IntegrationTests/Harness/PermissionUsers.cs` | new | Users whose only role holds a given permission set, cached per host |
| `tests/Harmony.Api.IntegrationTests/Harness/ApiTestHost.cs` | modify | `CreateGrpcChannel()` over `factory.Server.CreateHandler()` (FR-105) |
| `tests/Harmony.Api.IntegrationTests/Harness/TestTokenTests.cs` | modify | FR-140 tests |
| `tests/Harmony.Api.IntegrationTests/Harness/StandInTests.cs` | modify | The `POST api/Test` publisher test moves to `PUT api/Cards/{id}/dates` (Flagged concern 6) |
| `docs/features/22-api-authorization-matrix/spec.md` | modify | FR-006 scenario: dated note on the moved endpoint (Flagged concern 6) |
| `docs/architecture/overview.md` | modify | NFR-107 and Flagged concerns 1-5 |
| `docs/roadmap.md` | modify | R-07 done and notes; R-06 (concern 9), R-09 (gRPC markers), R-11 (concern 4) notes; a "Later" item for Compose and Kubernetes as Production (concerns 1, 2) |
| `AGENTS.md` | modify | The Api test command's filter note names the admin rule's IDs (FR-110 to FR-112) |

**Shared hot spots:** `Program.cs` and `ServiceCollectionExtensions.cs` (R-08 to R-11 will change startup too); `authorization-matrix.md`, where R-08 changes the `Non-member` marks on the same lines, so merge the new column before R-08 starts; `docs/roadmap.md`; `AGENTS.md`. No `Directory.Packages.props` change, no migration, no contract under `Protos/`.

## Tasks

Tasks run in order. `[P]` marks a task that shares no file with another `[P]` task: T1, T2 and T3 can run in parallel. Each task starts with a failing test, or, for T3, with guard tests that T4 turns red. Its commit ticks its box here. Commits: `test:`/`feat:`/`refactor:`/`docs:` with footer `Refs #25`.

- [x] **T1** [P] Harness users and tokens carry permissions
  - Covers: FR-140 (and the harness that FR-108, FR-110 to FR-112 and FR-114 need).
  - Files: `Harness/TestUsers.cs`, `Harness/TestTokens.cs`, `Harness/PermissionUsers.cs` (new), `Harness/TestTokenTests.cs`.
  - Red first: `TestTokenTests.Tokens_AdministratorUser_HarnessAndLoginCarrySamePermissionClaims`. It creates a user in `Administrator` with a known password, logs in through `POST api/identity/token`, and compares the permission claim values of both tokens (`JwtSecurityTokenHandler.ReadJwtToken`). It fails today: the harness token has none.
  - Proof:
    - `TestTokenTests.Tokens_AdministratorUser_HarnessAndLoginCarrySamePermissionClaims`
    - `TestTokenTests.Tokens_PermissionUser_CarriesExactlyItsRolePermissions`: a `PermissionUsers` user with `Permissions.Roles.View` only gets exactly that claim value.
    - The existing `TestTokenTests` and every matrix test stay green (tokens for `Basic` users gain no claims).
- [x] **T2** [P] Matrix format for the admin rule, and the public-set check
  - Covers: FR-107, FR-113 (except the rule on known gaps on administration lines, which lands in T4), and the pure parts of FR-110 to FR-112.
  - Files: `Authorization/MatrixFile.cs`, `MatrixCheck.cs`, `RowVerdict.cs`, `MatrixRows.cs`, `authorization-matrix.md`, `MatrixFileTests.cs`, `MatrixCheckTests.cs`, `RowVerdictTests.cs`, `MatrixRowsTests.cs`.
  - Change: the format in *Decisions*, Q5. In the file, every line gains a `Non-admin` cell (`refused` on the 8 administration lines, `-` elsewhere), the administration lines gain their `permission=` binding from the spec's *Administration permissions* table, and the legend explains both. Anonymous marks are unchanged. `RowVerdict.JudgeAdmin(AdminCaller, RowResponse, row)` treats a row as inconclusive when it did not reach the endpoint or got 405 or 401 (the login was not accepted). `PlainUser` and `AllButRequired` pass only on 403. `OnlyRequired` passes on any other status and fails on 403, naming the permission. `MatrixRows.AdminPlainUser()`, `AdminAllButRequired()` and `AdminOnlyRequired()` give one row per administration line, named `Admin (plain user) GET api/identity/role` and so on. `MatrixCheck` adds `PublicEndpoints`, the six from spec #22, and fails both ways (FR-107).
  - Proof (all pure, no Docker):
    - `MatrixFileTests.Parse_AdministrationLine_ReadsPermissionOutsideBindings`
    - `MatrixFileTests.Parse_AdministrationLineWithoutPermission_FailsNamingLine`
    - `MatrixFileTests.Parse_UnknownPermission_FailsNamingLine`
    - `MatrixFileTests.Parse_PermissionOnOtherScope_FailsNamingLine`
    - `MatrixFileTests.Parse_NonAdminKnownGap_FailsNamingLine`
    - `MatrixFileTests.Parse_NonAdminMarkOnOtherScope_FailsNamingLine` (FR-113)
    - `MatrixCheckTests.Check_PublicLineOutsideTheSix_FailsNamingLine`, `Check_OneOfTheSixNotPublic_FailsNamingLine` (FR-107)
    - `RowVerdictTests.Admin_Refusing403_PassesForPlainAndAllBut`, `Admin_LetIn_FailsForPlainAndAllBut`, `Admin_OnlyRequired403_FailsNamingPermission`, `Admin_OnlyRequiredOtherStatus_IsLetIn` (200, 400, 500), `Admin_401Or405OrNotReached_IsInconclusive` (FR-110 to FR-112)
    - `MatrixRowsTests.AdminRows_OneRowPerAdministrationLinePerCaller`
    - `MatrixCheckTests.TestHostAndMatrixFile_AreConsistent` stays green with the reformatted file.
- [x] **T3** [P] Guards for the explicitly anonymous list
  - Covers: FR-104, FR-105, FR-106.
  - Files: `Harness/ApiTestHost.cs`, `Authorization/ExplicitlyAnonymousTests.cs` (new), `Authorization/GrpcAnonymousTests.cs` (new).
  - These pass on today's code, because nothing requires a login yet. T4 proves they catch the regression (see T4, *Early check*).
  - Proof:
    - `ExplicitlyAnonymousTests.Health_Anonymous_Returns200` (`/healthz/ready`, `/healthz/live`)
    - `ExplicitlyAnonymousTests.ErrorPage_AnonymousRequestThatThrows_Returns500WithErrorPage`: `GET api/identity/user/confirm-email?userId=<random>&code=x` gets 500, and the body contains `An error occurred while processing your request.`
    - `GrpcAnonymousTests.GrpcService_AnonymousCall_IsNotUnauthenticated`: one row per service, using the methods in *Decisions*. Any `RpcException` status other than `Unauthenticated` passes; grpc-dotnet maps HTTP 401 to `Unauthenticated`.
- [ ] **T4** Login-required default (US1)
  - Covers: FR-101, FR-102, FR-103, FR-108, FR-109, FR-113 (no known gap on an administration line), NFR-106 (401 bodies). Keeps FR-104 to FR-106 green.
  - Files: `ServiceCollectionExtensions.cs`, `Program.cs`, `Pages/Error.cshtml.cs`, `TokenController.cs`, `authorization-matrix.md`, `MatrixFile.cs`, `MatrixFileTests.cs`, `AnonymousMatrixTests.cs`, `MatrixRowsTests.cs`, `Authorization/LoginRequiredTests.cs` (new).
  - Red first:
    - In the matrix file, all 94 `gap R-07` become `refused` and the anonymous baseline becomes 0. The 94 rows of `AnonymousRequest_ProtectedEndpoint_Returns401` fail.
    - Add `[Trait("Requirement", "FR-101")]` to that theory and `FR-103` to `AnonymousRequest_PublicEndpoint_IsNot401`.
    - New `LoginRequiredTests`:
      - `FallbackPolicy_TestHost_RequiresAuthenticatedUser`: `IAuthorizationPolicyProvider.GetFallbackPolicyAsync()` holds a `DenyAnonymousAuthorizationRequirement` (FR-102).
      - `AnonymousDeleteRole_ExistingRole_Returns401AndRoleRemains` and `AnonymousUpdateUserRoles_ExistingUser_Returns401AndRolesUnchanged` (FR-101, the takeover path, on a real role and user).
      - `AnonymousRequest_ProtectedEndpoint_ReturnsNotAuthorizedBody`: `GET api/Workspace` → 401 and `["You are not Authorized."]` (NFR-106).
      - `ExpiredLogin_ProtectedEndpoint_Returns401WithExpiredBody` on `api/Board/userboards` and `api/Workspace`, with a harness token that expired one minute ago → 401 and `["The Token is expired."]` (FR-108, NFR-106). `api/Workspace` has no attribute, so it is the red row.
      - `UnknownRoute_Anonymous_Returns401` (GET and POST `api/nothing-here`, red) and `UnknownRoute_Owner_Returns404` (guard) (FR-109).
      - `Token_HarnessUserCredentials_ReturnsToken` (FR-103, guard).
    - `MatrixFileTests.Parse_AdministrationLineWithKnownGap_FailsNamingLine` (FR-113) with its parser rule.
  - Code: the fallback policy; `.AllowAnonymous()` on the health checks and the six gRPC services; `[AllowAnonymous]` on `ErrorModel` and the two token actions; `ConfigureSwagger()` moves before `UseRouting()`.
  - **Early check (the riskiest step):** add only the fallback policy first, and run T3's guards and `ApiTestHostTests.HealthReady_HostStarted_Returns200`. Expected: health, error page and gRPC fail with 401 or `Unauthenticated`. Then add the markers and see them pass. If a guard stays green without its marker, it is not testing what it claims; fix the guard before continuing.
  - Existing test changed (beyond NFR-104's list): `MatrixRowsTests.AnonymousRows_CoverEveryLineOnceWithDisplayName` stops counting the known-gap placeholder row. With 0 gaps that theory yields its single placeholder, as spec #22 FR-040 designed.
  - Proof: the Api test command with `--filter-trait "Requirement=FR-101"`, then FR-102, FR-103, FR-104, FR-105, FR-106, FR-108, FR-109 and NFR-106, all green. `TestHostAndMatrixFile_AreConsistent` reports anonymous baseline 0.
- [ ] **T5** Administration needs its permission (US2)
  - Covers: FR-110, FR-111, FR-112, FR-114, NFR-106 (403 body).
  - Files: `RoleController.cs`, `UserController.cs`, `Authorization/AdminMatrixTests.cs` (new), `Authorization/AdministrationTests.cs` (new).
  - Red first:
    - `AdminMatrixTests.AdminRequest_PlainUser_Returns403` (FR-110) and `AdminRequest_AllButRequiredPermission_Returns403` (FR-111) are 8 rows each, all red. `AdminRequest_OnlyRequiredPermission_IsLetIn` (FR-112) is 8 rows that pass today and guard against over-restriction. Each row sends `RequestBuilder.Anonymous(endpoint)` (random IDs, `{}` body; spec, *Administration permissions*) with the caller's harness token. A theory asserts first that its line's `Non-admin` mark is `refused`.
    - `AdministrationTests.ForbiddenBody_PlainUser_IsResultEnvelope`: `GET api/identity/role` → `["You are not authorized to access this resource."]` (NFR-106).
    - `AdministrationTests.RoleRename_OnlyCreateOrOnlyEdit_Returns403AndKeepsName` (two rows) and `RoleRename_CreateAndEdit_RenamesRole` on a custom role, checked through `RoleManager` (FR-114).
    - `AdministrationTests.Administrator_LoginThroughTokenEndpoint_ListsUsers` (FR-112, guard).
  - Code: `[Authorize(Policy = ...)]` per the spec's table. In `RoleController.Post`, when `request.Id` is not empty and `AuthorizeAsync(User, Permissions.Roles.Edit)` fails, return `Forbid()`.
  - Proof: the Api test command filtered to FR-110, FR-111, FR-112, FR-114 and NFR-106: 24 admin rows plus the four tests, all green.
- [ ] **T6** A user reads only their own account and settings (US3)
  - Covers: FR-120, FR-121, FR-122.
  - Files: `AccountController.cs`, `UserController.cs`, `UserNotificationsController.cs`, `GetUserQuery.cs`, `GetUserNotificationsQuery.cs`, the client files in *Files that change* (routes, three managers, `Account.razor.cs`, `MainBody.razor.cs`), `authorization-matrix.md`, `Identity/OwnDataTests.cs` (new).
  - Red first:
    - `OwnDataTests.Account_LoggedInUser_ReturnsOwnDetailsOnly`: users A and B; A's response has A's ID, email, first and last name and no B value (FR-120).
    - `OwnDataTests.Notifications_LoggedInUser_ReturnsOwnSettingsOnly`: A and B set different lists through `POST api/UserNotifications`; A reads A's (FR-121).
    - `OwnDataTests.ReadById_OtherUsersId_Returns404WithoutTheirData` on both removed routes (FR-122).
    - `OwnDataTests.MatrixFile_HasNoUserScopeLine` (FR-122).
    - In the matrix file, the two `user` lines are replaced by `| api/UserNotifications | GET | self | refused | - | - |  |` and `| api/identity/account | GET | self | refused | - | - |  |` in sort order. `TestHostAndMatrixFile_AreConsistent` fails until the code lands.
  - Code: per *Files that change*. `IUserService.GetAsync` stays, because `GetUserHandler` uses it. The discovered action count stays 106 (two removed, two added).
  - Proof: the Api test command filtered to FR-120, FR-121, FR-122, plus `TestHostAndMatrixFile_AreConsistent`. `dotnet build Harmony.slnx` compiles the client, which proves no caller still uses the removed members.
- [ ] **T7** Leftovers removed (US4)
  - Covers: FR-130, FR-131 (FR-132 in T9), Flagged concern 6.
  - Files: `StandInTests.cs`, `Authorization/AttackSurfaceTests.cs` (new), `TestController.cs` (delete), `Program.cs`, `authorization-matrix.md`, `MatrixCheckTests.cs`, `docs/features/22-api-authorization-matrix/spec.md`.
  - First, still green on today's code: replace `StandInTests.TestEndpoint_OwnerPost_RecordsEmailNotification` with `StandInTests.UpdateCardDates_OwnerPut_RecordsEmailNotification` (FR-006). The owner sends `PUT api/Cards/{fixture.CardId}/dates` with a due date. Expected: 200, and a `CardDueTimeUpdatedNotification` whose `Id` is the fixture's card. Add a dated note to spec #22's FR-006 scenario.
  - Red first:
    - `AttackSurfaceTests.TestEndpoint_Owner_Returns404AndPublishesNothing`: 404, and no `CardDueTimeUpdatedNotification` for the hard-coded card `8D7FAE06-D1E3-4418-FD89-08DBF00B9F44` (FR-130). Matching on that ID keeps the test safe while other tests run in parallel.
    - `AttackSurfaceTests.ApiExplorer_NotDevelopment_IsNotServed`: 4 rows, `/swagger/index.html` and `/swagger/v1/swagger.json`, each anonymous and as the owner. None is 2xx or contains `"openapi"` (FR-131).
    - The `api/Test` line is removed from the matrix file, and `Discovery_TestHost_FindsOneEndpointPerControllerAction` expects 105.
  - Code: delete `TestController.cs`; move `ConfigureSwagger()` into the `IsDevelopment()` branch (still before routing).
  - Proof: the Api test command filtered to FR-130, FR-131 and FR-006, plus `TestHostAndMatrixFile_AreConsistent`.
- [ ] **T8** Documentation
  - Covers: NFR-107, NFR-101 (the middleware list), Flagged concerns 1-5 and 9.
  - Files: `docs/architecture/overview.md`, `docs/roadmap.md`, `AGENTS.md`, `authorization-matrix.md` (final legend pass), `spec.md` (*Expected sizes*, if any count differs).
  - `overview.md`:
    - Cross-cutting, authentication: the login-required default, the explicitly anonymous list, administration by permission, and the middleware that runs before routing (CORS preflight, static files and `/Files`, the API explorer in Development).
    - Known issues: the 94 gaps are closed. The gRPC services stay anonymous until R-09 (concern 3). The API explorer is Development-only, but Compose and Kubernetes run as Development (concern 1), and the gateway's aggregated Swagger loses the core API outside Development (concern 2). A permission change takes effect only when the token is refreshed (concern 4). A user with `Users.Edit` can grant themselves a non-administrator role (concern 5).
  - `roadmap.md`:
    - R-07: status `done`, with notes.
    - R-06: rewrite its rule (concern 9).
    - R-09: "remove the six `.AllowAnonymous()` gRPC markers".
    - R-11: concern 4.
    - "Later": "Run Compose and Kubernetes deployments as Production (Development-only API explorer, exception handler, HSTS; gateway Swagger)".
  - `AGENTS.md`: the Api test command's filter note adds `"Requirement=FR-110"` to `FR-112` (admin rule).
  - Proof: the links resolve (`docs/` relative paths checked by reading), and `harmony-docs` needs no change (spec, NFR-107).
- [ ] **T9** Verification and smoke run (`/sdlc-verify`)
  - Covers: NFR-102, NFR-103, NFR-104, NFR-105, and the verification-only parts of FR-102, FR-107, FR-113 and FR-132.
  - Local, not committed, recorded in the PR:
    - FR-102: a throwaway attribute-less action and a `MapGet` both get 401 anonymously.
    - FR-107: change `POST api/identity/token` to `self`, then make a seventh line `public`. Each time the matrix fails naming the line.
    - FR-113: remove one `permission=`, then misspell one. Each time the matrix fails naming the line.
  - Smoke run (NFR-102, FR-132): `dotnet run --project src/Aspire/Harmony.AppHost`.
    - `https://localhost:7108/healthz/ready` returns 200.
    - `https://localhost:7097/swagger/v1/swagger.json` returns 200 anonymously.
    - `john@harmony.com` logs in, opens a board, opens the account page (name and email shown), changes a notification setting, reloads, and sees it kept.
    - A card created on the board appears in a second tab without a reload.
    - The Aspire dashboard shows no gRPC call from Automations or Notifications ending in `Unauthenticated` or 401.
  - NFR-105: the CI `Integration tests` job's Api step takes at most 5 minutes; record its duration from the PR run.

### Existing tests that change

NFR-104 lists the intended changes. The rest are recorded here, as NFR-104 requires, and copied to the Deviations log when made.

| Test | Change | Listed in NFR-104 |
|---|---|---|
| `AnonymousMatrixTests` (all rows) | Known-gap rows become refused rows; FR-101 and FR-103 tags; rows of removed endpoints go with their lines | yes |
| `StandInTests.TestEndpoint_OwnerPost_RecordsEmailNotification` | Replaced by `UpdateCardDates_OwnerPut_RecordsEmailNotification` | yes (concern 6) |
| `MatrixFileTests` (header, `Document` helper, synthetic lines; "6 cells" → "7 cells") and `MatrixCheckTests` (`Document` helper, synthetic lines) | One more cell per line, for the `Non-admin` column (Q5) | no: a format change the spec delegated to the plan |
| `MatrixRowsTests.AnonymousRows_CoverEveryLineOnceWithDisplayName` | Excludes the placeholder row from the count | no: the count of 0 gaps exposes it |
| `MatrixCheckTests.Discovery_TestHost_FindsOneEndpointPerControllerAction` | 106 → 105 | no: the test itself says "update it with the matrix file" |

### Coverage

| Requirement | Task(s) | Test(s) |
|---|---|---|
| FR-101 | T4 | `AnonymousMatrixTests.AnonymousRequest_ProtectedEndpoint_Returns401` (99 rows); `LoginRequiredTests.AnonymousDeleteRole_*`, `AnonymousUpdateUserRoles_*` |
| FR-102 | T4, T9 | `LoginRequiredTests.FallbackPolicy_TestHost_RequiresAuthenticatedUser`; local check in T9 |
| FR-103 | T4 | `AnonymousMatrixTests.AnonymousRequest_PublicEndpoint_IsNot401` (6 rows); `LoginRequiredTests.Token_HarnessUserCredentials_ReturnsToken` |
| FR-104 | T3, T4 | `ExplicitlyAnonymousTests.Health_Anonymous_Returns200` |
| FR-105 | T3, T4 | `GrpcAnonymousTests.GrpcService_AnonymousCall_IsNotUnauthenticated` (6 rows) |
| FR-106 | T3, T4 | `ExplicitlyAnonymousTests.ErrorPage_AnonymousRequestThatThrows_Returns500WithErrorPage` |
| FR-107 | T2, T9 | `MatrixCheckTests.Check_PublicLineOutsideTheSix_*`, `Check_OneOfTheSixNotPublic_*`; local check in T9 |
| FR-108 | T1, T4 | `LoginRequiredTests.ExpiredLogin_ProtectedEndpoint_Returns401WithExpiredBody` |
| FR-109 | T4 | `LoginRequiredTests.UnknownRoute_Anonymous_Returns401`, `UnknownRoute_Owner_Returns404` |
| FR-110 | T2, T5 | `AdminMatrixTests.AdminRequest_PlainUser_Returns403` (8 rows); `RowVerdictTests.Admin_*` |
| FR-111 | T2, T5 | `AdminMatrixTests.AdminRequest_AllButRequiredPermission_Returns403` (8 rows) |
| FR-112 | T2, T5 | `AdminMatrixTests.AdminRequest_OnlyRequiredPermission_IsLetIn` (8 rows); `AdministrationTests.Administrator_LoginThroughTokenEndpoint_ListsUsers` |
| FR-113 | T2, T4, T9 | `MatrixFileTests.Parse_AdministrationLine*`, `Parse_UnknownPermission_*`, `Parse_PermissionOnOtherScope_*`, `Parse_NonAdmin*`; local check in T9 |
| FR-114 | T5 | `AdministrationTests.RoleRename_OnlyCreateOrOnlyEdit_Returns403AndKeepsName`, `RoleRename_CreateAndEdit_RenamesRole` |
| FR-120 | T6 | `OwnDataTests.Account_LoggedInUser_ReturnsOwnDetailsOnly`; anonymous row `GET api/identity/account` |
| FR-121 | T6 | `OwnDataTests.Notifications_LoggedInUser_ReturnsOwnSettingsOnly` |
| FR-122 | T6 | `OwnDataTests.ReadById_OtherUsersId_Returns404WithoutTheirData`, `MatrixFile_HasNoUserScopeLine` |
| FR-130 | T7 | `AttackSurfaceTests.TestEndpoint_Owner_Returns404AndPublishesNothing` |
| FR-131 | T7 | `AttackSurfaceTests.ApiExplorer_NotDevelopment_IsNotServed` (4 rows) |
| FR-132 | T9 | Smoke run, `swagger.json` 200 in Development (recorded in PR) |
| FR-140 | T1 | `TestTokenTests.Tokens_AdministratorUser_HarnessAndLoginCarrySamePermissionClaims`, `Tokens_PermissionUser_CarriesExactlyItsRolePermissions` |
| NFR-101 | T4, T8 | Matrix (FR-101, FR-103) + `TestHostAndMatrixFile_AreConsistent` (spec #22 FR-013) + T3 guards; overview list |
| NFR-102 | T9 | Smoke run (recorded in PR) |
| NFR-103 | T9 | Build and format commands |
| NFR-104 | T9 | All three test commands |
| NFR-105 | T9 | CI Api test step duration |
| NFR-106 | T4, T5 | `LoginRequiredTests.AnonymousRequest_ProtectedEndpoint_ReturnsNotAuthorizedBody`, `ExpiredLogin_*`; `AdministrationTests.ForbiddenBody_PlainUser_IsResultEnvelope` |
| NFR-107 | T8 | Review of the changed docs |

## Risks and what could break

| Risk | Likelihood / impact | Mitigation |
|---|---|---|
| **A missing anonymous marker** (health, `/Error`, gRPC). Probes fail and Aspire, Compose and Kubernetes never see the Api ready; Automations and Notifications lose their gRPC calls. | Medium / high | T3's guards are written first. T4's early check applies the fallback alone and watches them go red before the markers. The smoke run checks the dashboard for `Unauthenticated`. |
| **Swagger in Development returns 401**, because the fallback also covers requests that no endpoint matches and Swagger ran after authorization | High if missed / medium | T4 moves it before `UseRouting()`. T9 checks `swagger.json` anonymously in the smoke run (FR-132). |
| The client still calls a removed route (account page, main layout), or a cached old client gets 404 until it reloads | Low / medium | The removed members are deleted, not left in place, so the build finds every caller. The smoke run opens the account page. The PR's upgrade notes cover cached clients (concern 7). |
| An admin row is let in or refused for the wrong reason (for example `{}` gives 400 before authorization) | Low / medium | Endpoint authorization runs in middleware before model binding. `JudgeAdmin` treats 401, 405 and not-reached as inconclusive. The only-required theory proves the permission is enough. |
| The `Non-admin` column conflicts with R-08's edits to the same matrix lines | Medium / low | Merge #25 before R-08 starts editing the file. The column change is mechanical. |
| CORS preflight or `/Files` blocked | Low / medium | Both run before routing and authorization (`UseCors` is first; static files come before `UseRouting`). The smoke run uses the browser client. |
| An expired token on a public endpoint: `OnAuthenticationFailed` writes a 401 and the pipeline continues. This happens today and is unchanged here. | Existing / low | Out of scope (R-11). See *Found while planning*. |
| The Api test run gets longer (24 admin rows, 14 extra users and roles) | Low / low | Users are created once per permission set. NFR-105 is checked on CI in T9. |

## Alternatives considered

| Option | Why not chosen |
|---|---|
| `[Authorize]` on a base controller, or a global MVC `AuthorizeFilter` | It covers MVC only and is still opt-in for minimal APIs, Razor pages and gRPC (ADR, *Alternatives*). |
| Administration by role name (`Roles = "Administrator"`) | Rejected by the ADR. It would ignore the seeded permission model and the role-permission screen. |
| A `Permission` column instead of the `permission=` binding, or no `Non-admin` column | A `Permission` column is `-` on 97 of 105 lines. Without a `Non-admin` column the admin rule has no visible result, unlike the other two rules. The binding keeps the column count at 7. |
| Generating gRPC client stubs in the test project | Duplicates the Api's generated types: CS0436 warnings over the baseline. |
| Raw HTTP/2 gRPC frames for FR-105 | Possible, but reading `grpc-status` trailers through `TestServer` is more fragile than `Grpc.Net.Client`, which is already available. |
| `MapRazorPages().AllowAnonymous()` | It would make every future Razor page anonymous, which goes against ADR rule 2. |
| A new `GetCurrentUserQuery` | `GetUserQuery` already does exactly this and nothing else uses it. |
| Admin-rule users logging in through `POST api/identity/token` | It would make the admin rule depend on a public endpoint under test. Harness tokens with the same claims (FR-140) keep the rule self-contained. One test still covers the real login (FR-112). |
| Swagger behind a login outside Development instead of Development-only | Decided against in the spec (Q4, concern 1). |

## Migration and rollback

- **EF Core migration:** none. The permissions are existing role claims, and `DatabaseRolesSeeder` gives them all to `Administrator` at every startup. Existing data is untouched.
- **Feature flag:** none (spec, *Rollout*).
- **Deploy:** Api and client together, as always. Self-hosters read the upgrade notes in the PR (spec, *Rollout* 3).
- **Rollback:** revert the merge commit and redeploy. Nothing is stored, so nothing needs undoing. Tokens issued in between stay valid on the old code.

## Proof (definition of done)

- Build: `dotnet build Harmony.slnx`. 0 errors, no more than 585 code warnings (`WARNING_BASELINE`), 90 NU1903.
- Test: `dotnet test --solution Harmony.slnx`. All green, none skipped.
- Integration tests: `dotnet test --project tests/Harmony.Infrastructure.IntegrationTests -p:HarmonyIntegrationTests=true`. All green (NFR-104; the data layer is unchanged).
- Api tests: `dotnet test --project tests/Harmony.Api.IntegrationTests -p:HarmonyIntegrationTests=true`. All green. The matrix reports anonymous baseline 0 and non-member baseline 80, and the admin rule has 24 rows.
- Format: `dotnet format Harmony.slnx --verify-no-changes --include <changed files>`. Exit code 0.
- Every FR and NFR ID above has a passing test tagged `[Trait("Requirement", "<ID>")]`, or a recorded verification-only result (FR-132, NFR-102, NFR-103, NFR-105).
- Each of these filters runs more than 0 tests: `--filter-trait "Requirement=FR-101"`, FR-110, FR-114, FR-120 and FR-140.
- T9's local checks and smoke run are recorded in the PR.

## Found while planning (for the product owner, not in scope)

- **An expired token on a public endpoint.** `OnAuthenticationFailed` writes a 401 body for an expired token and does not stop the pipeline. On an anonymous endpoint (for example `POST api/identity/token/refresh` sent with an expired `Authorization` header), the action then runs against a response that has already started. This is inferred from the handler code; confirm under R-11.
- **Unknown gRPC methods.** The `grpcunimplemented` endpoints fall under the fallback policy, so an anonymous call to an unknown gRPC method gets `Unauthenticated` instead of `Unimplemented`. This is harmless, and R-09 decides.

## Deviations log

Update this table in the same commit as any change that departs from the plan.

| Date | Task | What changed and why |
|---|---|---|
| 2026-11-27 | T1 | `TestUser` keeps its six positional parameters. `Password` and `Permissions` are init properties with empty defaults, so `RequestBuilderTests`, which builds a `TestUser` positionally, does not change. The optional `expires` on `TestTokens` moves to T4, next to the FR-108 test that uses it. The login helper is `TestTokenTests.LoginAsync`, public so that T5's FR-112 test can reuse it. `PermissionUsers` is used as the extension `ApiTestHost.UserWithPermissionsAsync(params string[])`. |
| 2026-11-27 | T2 | `RequestBuilderTests` builds a synthetic matrix line too, so its `Line` helper gains the seventh cell, the same format change as `MatrixFileTests` and `MatrixCheckTests` (*Existing tests that change*). `MatrixFileTests.Parse_WrongHeader_Fails` now uses the old 6-cell header as its wrong header. `RowVerdict.JudgeAdmin` also takes the line's permission, so that its messages can name it. `Parse_NonAdminMarkOnOtherScope_FailsNamingLine` also has a row for `-` on an administration line. The FR-011 message's suggested line gains the seventh cell. |
