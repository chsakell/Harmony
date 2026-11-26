# Harmony roadmap

> The prioritized backlog for Harmony. Each item is written so it can be fed straight into the SDLC: `/sdlc-intent` for Track M and L, `/sdlc-debug` for Track S bugs.
>
> **Last reviewed:** 2026-11-25. Evidence (file:line) was taken from the code on that date. Re-check it when you start an item.

## How to use this file

1. Pick the next item from [Recommended order](#recommended-order). Its dependencies must be done first.
2. Open a GitHub issue with the item's **Problem** and **Outcome** text: `gh issue create --title "<item>" --label <bug|enhancement|documentation>`.
3. Start the work on the right track:
   - **Track S:** `bugfix/<id>-<slug>` (or `feature/<id>-<slug>` for non-bugs), then `/sdlc-debug` or a direct change, then `/sdlc-verify` and `/sdlc-pr` with the label `sdlc:track-s`.
   - **Track M:** `/sdlc-intent #<id>`, then spec, plan and implement through the gates.
   - **Track L:** an epic intent and spec (`level: epic`) plus ADRs, then one Track M feature per slice.
4. In the item's own pull request, fill in the **Issue** column and set **Status** in the table below. When the PR merges, set it to `done`.
5. Items marked **high-risk** get the label `sdlc:high-risk`.
6. When a change alters behaviour that users see, update the matching page in the `harmony-docs` GitBook repository.

**Legend.**
- **Type:** test, security, bug, improvement, feature.
- **Track:** S (no documents), M (intent, spec, plan), L (epic).
- **+ADR:** the item needs an architecture decision record.
- **Status:** `todo`, `in progress`, `done`.

## Summary

| # | Item | Type | Track | Risk | Depends on | Issue | Status |
|---|---|---|---|---|---|---|---|
| **Phase 0: Safety net** | | | | | | | |
| [R-01](#r-01-test-foundation) | Test foundation | test | M +ADR | | - | [#5](https://github.com/chsakell/Harmony/issues/5) | done |
| [R-02](#r-02-ci-build-workflow) | CI build workflow | improvement | S | | R-01 | [#9](https://github.com/chsakell/Harmony/issues/9) | done |
| [R-03](#r-03-integration-test-harness) | Integration test harness (SQL Server + PostgreSQL) | test | M | | R-01 | [#19](https://github.com/chsakell/Harmony/issues/19) | in progress |
| [R-04](#r-04-api-test-host-and-authorization-matrix) | Api test host and authorization matrix | test | M | | R-01 | | todo |
| [R-05](#r-05-characterization-tests-for-scrum-and-automations) | Characterization tests for Scrum and automations | test | S | | R-01 | | todo |
| [R-06](#r-06-architecture-tests) | Architecture tests | test | S | | R-01 | | todo |
| **Phase 1: Security** | | | | | | | |
| [R-07](#r-07-authenticated-by-default-api) | Authenticated-by-default API | security | M | high | R-04 | | todo |
| [R-08](#r-08-board-card-and-workspace-authorization) | Board, card and workspace authorization | security | M | high | R-04, R-07 | | todo |
| [R-09](#r-09-service-to-service-authentication) | Service-to-service authentication | security | M +ADR | high | R-07 | | todo |
| [R-10](#r-10-authenticate-automations-sourcecontrol-and-signalr) | Authenticate Automations, SourceControl and SignalR | security | M | high | R-07 | | todo |
| [R-11](#r-11-identity-and-token-hardening) | Identity and token hardening | security | M | high | R-04 | | todo |
| [R-12](#r-12-github-webhook-signature-and-locking) | GitHub webhook signature and locking | security | S | high | R-01 | | todo |
| [R-13](#r-13-safe-file-uploads) | Safe file uploads | security | M | high | R-04 | | todo |
| [R-14](#r-14-replace-automapper) | Replace AutoMapper | security | M +ADR | | R-01 | | todo |
| **Phase 2: Make what exists correct** | | | | | | | |
| [R-15](#r-15-backlog-reorder-moves-cards-on-every-board) | Backlog reorder moves cards on every board | bug | S | | R-01 | [#11](https://github.com/chsakell/Harmony/issues/11) | done |
| [R-16](#r-16-card-search-returns-nothing-without-an-excluded-card) | Card search returns nothing without an excluded card | bug | S | | R-01 | [#17](https://github.com/chsakell/Harmony/issues/17) | done |
| [R-17](#r-17-sprint-lifecycle-rules) | Sprint lifecycle rules | bug | M | | R-05 | | todo |
| [R-18](#r-18-status-categories-and-reliable-completion-dates) | Status categories and reliable completion dates | bug | M | | R-05 | | todo |
| [R-19](#r-19-correct-burndown-chart) | Correct burndown chart | bug | M | | R-17, R-18 | | todo |
| [R-20](#r-20-subtask-integrity) | Subtask integrity | bug | M | | R-05 | | todo |
| [R-21](#r-21-automations-correctness) | Automations correctness | bug | M | | R-05, R-18 | | todo |
| [R-22](#r-22-card-keys-positions-and-paging) | Card keys, positions and paging | bug | M | | R-03 | | todo |
| [R-23](#r-23-reliable-messaging-and-transactions) | Reliable messaging and transactions | improvement | M +ADR | high | R-03 | | todo |
| [R-24](#r-24-validation-and-error-contract) | Validation and error contract | improvement | M | | R-04 | | todo |
| **Phase 3: Agile model done right** | | | | | | | |
| [R-25](#r-25-issue-hierarchy-epic-story-subtask) | Issue hierarchy: Epic, Story, Subtask | feature | L +ADR | | R-20 | | todo |
| [R-26](#r-26-issue-history) | Issue history | feature | M | | R-18 | | todo |
| **Phase 4: Jira-like features** | | | | | | | |
| [R-27](#r-27-priority-assignee-and-resolution) | Priority, assignee and resolution | feature | M | | R-24 | | todo |
| [R-28](#r-28-velocity-chart-and-sprint-report) | Velocity chart and sprint report | feature | M | | R-19, R-26 | | todo |
| [R-29](#r-29-backlog-planning-view) | Backlog planning view | feature | M | | R-15, R-22, R-25 | | todo |
| [R-30](#r-30-watchers-and-mentions) | Watchers and @mentions | feature | M | | R-08, R-23 | | todo |

## Recommended order

Tests come first, so that every later fix is proven by a test and cannot quietly regress.

1. **R-01, R-02.** Test projects and a CI build, so every pull request proves itself.
2. **R-15, R-16.** Two quick bugs: one changes other boards' data, the other breaks card search. Each is fixed with a failing test first.
3. **R-04, R-07, R-08.** Write the authorization matrix, watch it fail, then make it pass.
4. **R-11, R-12.** Tokens, the seeded admin account and the webhook.
5. **R-05, then R-17 to R-21.** Pin down today's Scrum behaviour, then correct it.
6. The rest of Phase 1 and 2, then Phases 3 and 4.

---

## Phase 0: Safety net

### R-01 Test foundation

- **Problem:** The solution has no test projects. `dotnet test Harmony.slnx` runs nothing, so no change can be proven and nothing protects against regressions.
- **Outcome:**
  - A `tests/` folder with a shared `tests/Directory.Build.props`.
  - A first unit test project, `Harmony.Application.UnitTests`, using xUnit v3, NSubstitute, Shouldly and coverlet.
  - Passing tests for the pure helpers (`CardHelper.GetSerialKey`, `DisplayDates`, `IssueTypeExtensions`, `PredicateBuilder`) and an AutoMapper configuration test.
  - Tests carry `[Trait("Requirement", ...)]` and run with `dotnet test Harmony.slnx`.
- **Evidence:** `docs/architecture/overview.md` (Known issues). `global.json` does not opt into Microsoft.Testing.Platform, so `dotnet test --solution` does not work (AGENTS.md).
- **Notes:**
  - ADR "testing strategy": test frameworks, assertion library (FluentAssertions 8 is commercial), whether to opt into Microsoft.Testing.Platform, and the planned project layout (unit, integration, Api, architecture).
  - Handlers depend only on interfaces, so they are easy to unit test. About 15 handlers run EF async operators directly on `IQueryable`; those belong in integration tests (R-03).
  - Update the Test command in AGENTS.md.

### R-02 CI build workflow

- **Problem:** There is no CI build for the application. A pull request can break the build or the tests without anyone noticing until later.
- **Outcome:** `.github/workflows/ci.yml` runs build, test and format (changed files only) on every pull request to `develop` and `main`, and is a required status check next to `SDLC gates`. Build warnings do not exceed the recorded baseline.
- **Evidence:** `.github/workflows/` holds only `sdlc-gates.yml` and `copilot-setup-steps.yml`.
- **Notes:** Cache NuGet packages. Upload test results. Integration tests (R-03) need Docker on the runner; plan for a separate job.

### R-03 Integration test harness

- **Problem:** The data layer supports SQL Server and PostgreSQL, but nothing checks that migrations, queries and query filters work on both. Several bugs (R-16, R-22) live in LINQ queries that unit tests cannot reach.
- **Outcome:**
  - `Harmony.Infrastructure.IntegrationTests` uses Testcontainers for SQL Server and PostgreSQL.
  - Migrations apply cleanly on both providers.
  - Tests cover `CardRepository`, `BoardService.LoadBoard`, `BoardService.HasUserAccessToBoard` and `GetUserBoards`.
  - MongoDB repositories for Automations and SourceControl are covered too.
- **Evidence:** `src/Infrastructure/Harmony.Persistence/Configurations/CardConfiguration.cs:49` has a global query filter that hides every child card, a common source of surprises. `src/Infrastructure/Harmony.Infrastructure/Repositories/*.cs`.
- **Notes:** These tests need Docker. Run them as a separate CI job (R-02). Also cover the backlog reorder query in `CardService.ReorderCardsInBacklog` fixed by R-15 (#11): its unit tests run the query in memory, so its SQL translation is unproven. The same applies to the card search in `CardService.GetSearchWorkspaceQuery` fixed by R-16 (#17). Check there whether the title match is case-insensitive on both providers: SQL Server's default collation is, PostgreSQL is not.

### R-04 Api test host and authorization matrix

- **Problem:** Nobody can tell which endpoints are protected. Today most of them are not (see R-07 and R-08), and nothing would catch a new unprotected endpoint.
- **Outcome:**
  - `Harmony.Api.IntegrationTests` uses `WebApplicationFactory` with a test database, a fake `INotificationsPublisher`, in-memory caching and test JWTs.
  - A reflection-driven **authorization matrix**: every endpoint returns 401 when anonymous unless it is `[AllowAnonymous]`, and a non-member is refused on another user's board, card, list and workspace.
- **Evidence:** `src/Services/Harmony.Api/Controllers/**`.
- **Notes:** The matrix fails today. Land it together with R-07 and R-08 (each fix turns its rows green), or check in the expected-failure list as data that R-07 and R-08 empty. Never skip a failing test.

### R-05 Characterization tests for Scrum and automations

- **Problem:** The Scrum handlers and automations have known bugs (R-17 to R-21), but no test records what they do today. Fixing them without a baseline risks breaking something else.
- **Outcome:**
  - Tests describe the current behaviour of `MoveCardCommandHandler`, `CompleteSprintCommandHandler`, `CreateChildIssueCommandHandler` and `SprintService.GetSprintReports`.
  - They also cover the three automations (sum up story points, sync parent and child, smart auto-assign) with mocked gRPC clients.
  - Known-wrong behaviour is named in the test so that the fixing item changes it on purpose.
- **Evidence:**
  - `src/Core/Harmony.Application/Features/Cards/Commands/MoveCard/MoveCardCommandHandler.cs`
  - `src/Core/Harmony.Application/Features/Sprints/Commands/CompleteSprint/CompleteSprintCommandHandler.cs`
  - `src/Infrastructure/Harmony.Infrastructure/Services/Management/SprintService.cs`
  - `src/Services/Harmony.Automations/Services/*AutomationService.cs`
- **Notes:** Tests only, no behaviour change, so Track S.

### R-06 Architecture tests

- **Problem:** Clean architecture and the "authorize by default" rule are conventions only. Nothing stops a reference from Application to Infrastructure, or a new controller action without authorization.
- **Outcome:** `Harmony.ArchitectureTests` (NetArchTest or ArchUnitNET) checks four rules:
  - Domain has no outward dependencies.
  - Application does not reference Persistence or Infrastructure.
  - Every controller action has `[Authorize]` or `[AllowAnonymous]`.
  - `DangerousAcceptAnyServerCertificateValidator` is not used outside Development.
- **Evidence:** AGENTS.md (Architecture). `docs/architecture/overview.md` (Known issues).
- **Notes:** The authorization rule fails until R-07 is done. Order the work accordingly.

## Phase 1: Security

### R-07 Authenticated-by-default API

- **Problem:** The core API lets anonymous callers do things only logged-in users or administrators should be able to do, including creating and deleting roles, changing permissions, listing all users and changing their roles.
- **Outcome:**
  - The API requires an authenticated user by default (fallback policy).
  - Only login, register, token refresh and other deliberately public endpoints are `[AllowAnonymous]`.
  - Role and user administration require an administrator permission.
  - The anonymous `TestController` is removed, and Swagger is served only in Development.
- **Evidence:**
  - `src/Services/Harmony.Api/Extensions/ServiceCollectionExtensions.cs:234` (`AddAuthorization` without a fallback policy)
  - `src/Services/Harmony.Api/Controllers/Identity/RoleController.cs` and `UserController.cs` (no `[Authorize]`)
  - `src/Services/Harmony.Api/Controllers/TestController.cs`
  - `src/Services/Harmony.Api/Program.cs` (Swagger always on)
- **Notes:** The gateway forwards `/core/{everything}`, so the API itself must enforce this.

### R-08 Board, card and workspace authorization

- **Problem:** Any logged-in user can act on any board, card, list or workspace whose ID they know, and can make themselves Admin of any board. The board roles (Admin, Member, Observer) are never enforced.
- **Outcome:**
  - A MediatR authorization behaviour checks board, card or workspace membership for every request that names one. The board is resolved on the server from the card or list ID, never taken from the client.
  - Managing board members requires board Admin, and Observers are read-only.
  - The cached board-access result is cleared when a member is removed or a board's visibility changes.
- **Evidence:**
  - `src/Core/Harmony.Application/Features/Boards/Commands/AddUserBoard/AddUserBoardCommandHandler.cs:51-81`
  - `UpdateUserBoardAccess` and `RemoveUserBoard` handlers in the same folder
  - `src/Core/Harmony.Application/Features/Cards/Commands/UpdateCardTitle/UpdateCardTitleCommandHandler.cs:54-78`
  - `GetBoardHandler` and `LoadBoardListHandler`: the only handlers that call `HasUserAccessToBoard`
  - `SprintsController` uses IDs from the body instead of the route.
- **Notes:** Proven by the R-04 matrix. Update `harmony-docs/guide/boards/members.md` with the role rules.

### R-09 Service-to-service authentication

- **Problem:** Any caller can claim to be the Automations or Notifications service by sending one HTTP header, and can then add card members or create and delete links. The internal gRPC endpoints have no authentication, and gRPC clients accept any server certificate.
- **Outcome:**
  - The services authenticate to each other with a real mechanism (client-credentials tokens or mTLS), and the `harmony-trusted-client` header is no longer trusted. The gateway strips it from external requests.
  - The gRPC endpoints require service authentication, and certificate validation is on outside Development.
- **Evidence:**
  - `src/Services/Harmony.Api/Services/CurrentUserService.cs:32-46` (`IsTrustedClientRequest`); `GetHeader` ignores its `name` argument.
  - `src/Services/Harmony.Automations/Services/SmartAutoAssignAutomationService.cs` sends the header.
  - `src/Services/Harmony.Api/Services/gRPC/UserService.cs` returns user emails to any caller.
- **Notes:** ADR needed for the mechanism. It must also work under Aspire, Docker Compose and Kubernetes.

### R-10 Authenticate Automations, SourceControl and SignalR

- **Problem:** Anyone can list, toggle or delete any board's automations, read any board's source-control activity, and connect to the SignalR hub to receive every real-time event of any board. CORS allows any origin with credentials.
- **Outcome:**
  - The three services validate the same JWT as the API, through shared configuration in ServiceDefaults.
  - Automations and SourceControl endpoints check board access.
  - The SignalR hub requires a token and checks board access before adding a connection to a board group.
  - CORS allows only the configured client origin.
- **Evidence:**
  - `src/Services/Harmony.SignalR/Hubs/SignalRHub.cs`
  - `src/Services/Harmony.SignalR/Program.cs` (no `UseAuthentication`)
  - `src/Services/Harmony.Automations/Controllers/AutomationsController.cs`
  - `src/Integrations/Harmony.Integrations.SourceControl/Controllers/ActivityController.cs`
  - `SetIsOriginAllowed(_ => true)` in the Api, SignalR and the gateway
- **Notes:** The Blazor client must send the token on the SignalR connection.

### R-11 Identity and token hardening

- **Problem:** Tokens are easier to forge, steal or reuse than they should be, and login is open to brute force.
  - The JWT signing key is committed to the repository and is the same in Production.
  - Issuer and audience are not validated, and access tokens live for two days.
  - Every installation is seeded with `admin@harmony.com` and a known password.
  - Login has no lockout or rate limit, and its error messages reveal which accounts exist.
- **Outcome:**
  - The signing key comes from configuration secrets (startup fails if it is missing), and the committed key is rotated.
  - Issuer and audience are validated, and access tokens are short-lived (refresh tokens already exist).
  - The admin account is seeded only in Development, or from a configured secret.
  - Login uses lockout and one generic error message, and the token endpoint is rate-limited.
  - The Blazor client refreshes tokens once at a time instead of racing.
- **Evidence:**
  - `src/Services/Harmony.Api/appsettings.json`, `appsettings.Production.json` (`AppConfiguration:Secret`)
  - `src/Services/Harmony.Api/Extensions/ServiceCollectionExtensions.cs` (token validation, password rules)
  - `src/Infrastructure/Harmony.Infrastructure/Services/Identity/IdentityService.cs`
  - `src/Infrastructure/Harmony.Infrastructure/Seed/DatabaseRolesSeeder.cs`
  - `src/Web/Harmony.Client.Infrastructure/Managers/Identity/Authentication/AuthenticationManager.cs`
  - `src/Web/Harmony.Client.Infrastructure/Interceptors/HttpInterceptorManager.cs`
- **Notes:** Self-hosters need clear upgrade notes. Update `harmony-docs/configuration/before-running.md`.

### R-12 GitHub webhook signature and locking

- **Problem:** Anyone can post fake GitHub events (pushes, branches, pull requests) to a board. Failures are swallowed and answered with 200, so GitHub never retries. The per-repository lock does not actually lock.
- **Outcome:**
  - Webhooks are accepted only with a valid `X-Hub-Signature-256` for a configured secret, compared in constant time.
  - Failures return a 5xx status so GitHub retries.
  - Concurrent deliveries for the same key are serialized correctly, or are made idempotent.
- **Evidence:** `src/Integrations/Harmony.Integrations.SourceControl/Controllers/GithubController.cs:41-75` (no signature check) and `:262-283` (lock).
- **Notes:** Document the new webhook secret setting in `harmony-docs/integrations/github.md`.

### R-13 Safe file uploads

- **Problem:** Attachments of any type are stored and served publicly from the API's `Files/` folder without authentication. An uploaded `.html` or `.svg` file can run script in the user's browser.
- **Outcome:**
  - Uploads are limited by extension, content type and size, and are received as `IFormFile` instead of base64 JSON.
  - Files are served with `X-Content-Type-Options: nosniff` and `Content-Disposition: attachment`, ideally through an endpoint that checks board access.
- **Evidence:**
  - `src/Infrastructure/Harmony.Infrastructure/Services/UploadService.cs:25-31`
  - `src/Services/Harmony.Api/Program.cs` (static files for `Files/`)
  - `src/Services/Harmony.Api/Controllers/Content/FilesController.cs`
- **Notes:** Changes the upload API contract, so the Blazor client changes too.

### R-14 Replace AutoMapper

- **Problem:** AutoMapper 14.0.0 has a high-severity advisory (NU1903). The fixed versions need a commercial licence.
- **Outcome:** Mapping uses a free, source-generated mapper (Mapperly), AutoMapper is removed from `Directory.Packages.props`, and the build has no NU1903 warning.
- **Evidence:** `Directory.Packages.props`; `docs/architecture/overview.md` (Known issues).
- **Notes:**
  - ADR to close the open licence decision.
  - Keep the R-01 mapping test as the safety net.
  - MediatR is pinned at its last open-source version (12.5.0); note this in the same ADR.

## Phase 2: Make what exists correct

### R-15 Backlog reorder moves cards on every board

- **Problem:** Moving an issue up or down in one project's backlog renumbers backlog issues in *every* project, so other teams' backlog order changes without anyone touching it.
- **Outcome:** Reordering a backlog changes positions only on that board. A test proves that another board's backlog is untouched.
- **Evidence:** `src/Infrastructure/Harmony.Infrastructure/Services/Management/CardService.cs:416` and `:430`. The filter `board.Id == issueType.BoardId` repeats the join condition and is always true; the board ID of the moved card is never applied.
- **Notes:** Track S with `/sdlc-debug`. Called from the backlog page's up and down buttons (`src/Web/Harmony.Client/Pages/Management/Backlog.razor.cs`).

### R-16 Card search returns nothing without an excluded card

- **Problem:** The search used to pick an issue to link returns no cards, and a count of 0, when no card is excluded. When a card is excluded, the result is correct. The client always excludes the source card, so only direct API calls hit the bug. An earlier version of this item said the excluded-card case returned cards from every workspace. The #17 reproduction test shows it does not, so there is no data leak.
- **Outcome:** The search returns only non-archived cards of the current workspace that match the term, and excludes the given card.
- **Evidence:** `src/Infrastructure/Harmony.Infrastructure/Services/Management/CardService.cs:278-281`. `&&` binds tighter than `?:`, so the whole condition becomes `(workspace && term && notArchived && skip.HasValue) ? card.Id != skip : false`, which is always false when no card is skipped. Used by `SearchCardsHandler`.
- **Notes:** Track S with `/sdlc-debug`.

### R-17 Sprint lifecycle rules

- **Problem:** The rules of a sprint exist only in the user interface. Through the API:
  - A completed sprint can be restarted, or started without dates.
  - Completing a sprint brings archived issues back.
  - If no option is chosen, unfinished issues stay in the completed sprint.
  - A failed "create next sprint" step crashes the request.
  - The steps of completing a sprint are not one transaction, so a failure leaves half-moved issues.
- **Outcome:** The server enforces the lifecycle:
  - **Start:** only Idle sprints of that board, with start and end dates.
  - **Complete:**
    - Only Active sprints. Archived issues stay archived.
    - Every unfinished issue goes to the backlog, an Idle sprint of the same board, or a new sprint that has dates.
    - The actual completion time is recorded, and everything happens in one transaction.
  - **Edit:** completed sprints keep their dates.
  - **Delete:** an Idle sprint can be deleted.
- **Evidence:**
  - `src/Core/Harmony.Application/Features/Sprints/Commands/StartSprint/StartSprintCommandHandler.cs:40-44` (no guards)
  - `CompleteSprintCommandHandler.cs:83` (`operationResult.Messages` while `operationResult` is null); `:68-71` (next sprint created without dates); `:114-115` (unordered "first" list)
  - `src/Infrastructure/Harmony.Infrastructure/Repositories/CardRepository.cs:190-195` (`GetPendingSprintCards` has no status filter)
- **Notes:**
  - **Decision:** one active sprint per board, or several? `harmony-docs/guide/scrum/sprints.md` says several are allowed, while the board shows an arbitrary one (`GetBoardHandler.cs:114`).
  - Update `sprints.md` either way.

### R-18 Status categories and reliable completion dates

- **Problem:** Harmony decides "done" from a flag that only the three default lists have. Lists a team adds can never count as done.
  - The completion date is reset to the server's local time on every reorder inside Done, and the "card completed" email is sent again each time.
  - When an automation moves an issue to Done, no completion date is set at all.
  - Every report that uses completion dates is therefore wrong.
- **Outcome:**
  - Every list belongs to a status category: To Do, In Progress or Done. Users can set it.
  - `DateCompleted` is set in UTC only when an issue enters a Done list and is cleared when it leaves, for user moves and automation moves alike.
  - The completed email is sent once per transition.
- **Evidence:**
  - `src/Core/Harmony.Application/Features/Lists/Commands/CreateList/CreateListCommandHandler.cs:53-58` (new lists get no status)
  - `src/Core/Harmony.Application/Features/Cards/Commands/MoveCard/MoveCardCommandHandler.cs:105-116`
  - `src/Services/Harmony.Api/Services/gRPC/CardService.cs` (the move never sets `DateCompleted`)
- **Notes:** A step towards configurable workflows (see [Later](#later-not-in-the-30)). Update `harmony-docs/guide/boards/board-lists.md` and `guide/scrum/create.md`.

### R-19 Correct burndown chart

- **Problem:** The burndown chart misleads the team.
  - It is built from the issues linked to the sprint *now*. Unfinished issues leave the sprint when it completes, so every completed sprint looks 100% burned down.
  - Work added mid-sprint looks as if it was planned from the start, and archived issues are counted.
  - A one-workday sprint divides by zero, and an issue without a type crashes the report.
- **Outcome:**
  - The sprint's scope is snapshotted when it starts, and issues added or removed later are recorded.
  - The burndown shows remaining points per day from that scope, excludes archived issues, and handles short sprints and missing issue types.
  - Completed sprints keep a truthful report.
- **Evidence:** `src/Infrastructure/Harmony.Infrastructure/Services/Management/SprintService.cs:35-37` (current sprint cards), `:53` (division by `totalWorkDays - 1`), `:108` (null issue type).
- **Notes:** Needs R-17 (completion time) and R-18 (reliable completion dates). The basis for R-28. Update `harmony-docs/guide/scrum/sprints.md` (Sprint reports).

### R-20 Subtask integrity

- **Problem:** Subtasks lose their place and their parent.
  - New subtasks all get position 0, and creating one overwrites the parent's search entry.
  - When a parent moves to the backlog, its subtasks stay on the sprint board.
  - Moving a subtask to a sprint quietly detaches it from its parent and gives it a random issue type.
  - Any issue can be changed to any type, including a type from another board.
  - Issues can link to themselves, and story points can be negative.
- **Outcome:**
  - Subtasks keep correct positions and their own search entries.
  - Subtasks follow their parent between backlog and sprints.
  - Type changes respect the rules: a subtask stays a subtask, a parent cannot become a subtask, the type must be from the same board.
  - Self-links and negative story points are rejected.
- **Evidence:**
  - `src/Core/Harmony.Application/Features/Cards/Commands/CreateChildIssue/CreateChildIssueCommandHandler.cs:72,85,99` (positions); `:119-123` (search message uses the parent)
  - `src/Infrastructure/Harmony.Infrastructure/Services/Management/CardService.cs:91-98` (detach and type) and `:197-204` (backlog move ignores children)
  - `UpdateCardIssueTypeCommandHandler.cs:49-54`
  - `CreateLinkCommandHandler.cs:62-84`
- **Notes:** The global query filter on child cards (`CardConfiguration.cs:49`) causes several of these bugs. Prepares R-25.

### R-21 Automations correctness

- **Problem:** Automations behave differently from what their settings say.
  - Disabled automations still run, and a disabled "sum up story points" rule still blocks manual story-point edits.
  - Sum-up ignores children that are created, archived or moved, and counts deleted children.
  - Sync parent and child is blocked by archived children and does not set the completion date.
  - "Auto-assign after 5 minutes" fires after 5 seconds, old scheduled jobs are never cancelled, and it never fires for issues created in the backlog or a sprint.
- **Outcome:** Each automation honours its Enabled flag and its settings.
  - Sum-up recalculates whenever the set of children changes, ignoring archived and deleted children.
  - Sync ignores archived children and sets completion dates (through R-18).
  - Auto-assign waits the configured time, replaces stale jobs and fires for every issue creation.
- **Evidence:**
  - `src/Infrastructure/Harmony.Infrastructure/Repositories/AutomationRepository.cs:89-107` (no `Enabled` filter)
  - `src/Services/Harmony.Automations/Services/AutomationService.cs:61-77`
  - `src/Services/Harmony.Automations/Services/SmartAutoAssignAutomationService.cs:45-46` (`After_5_Minutes` uses `TimeSpan.FromSeconds(5)`)
  - `SyncParentAndChildIssuesAutomationService.cs:51`
  - `CardService.cs:312-325` (sum includes deleted children)
- **Notes:** Update the pages under `harmony-docs/guide/automations/`.

### R-22 Card keys, positions and paging

- **Problem:** Some failures are rare, but they corrupt data or confuse users.
  - Two issues created at the same time can get the same key (for example `HAR-42`).
  - Backlog positions wrap after 255 issues.
  - Several lists show wrong totals or change order between pages.
  - Backlog issues are missing from the Work Items page.
- **Outcome:**
  - Card keys are unique (a unique index plus safe generation), and positions use a type that does not overflow.
  - Paged lists return the real total with a stable order.
  - Work Items can show backlog issues.
- **Evidence:**
  - `src/Infrastructure/Harmony.Infrastructure/Repositories/CardRepository.cs:128-142` (count + 1 keys)
  - `src/Core/Harmony.Application/Features/Cards/Commands/CreateBacklog/CreateBacklogCommandHandler.cs:60` (`(byte)` position)
  - `GetSprintsCardsHandler.cs:67` (page size returned as total)
  - `GetWorkItemsHandler.cs:38,43-59` (count and page disagree; Active only)
- **Notes:** Data model change with a migration on both providers (R-03 proves it).

### R-23 Reliable messaging and transactions

- **Problem:** Real-time updates, emails, search indexing and automations can be lost silently.
  - Consumers acknowledge messages before handling them.
  - Failures are not retried and there is no dead-letter queue.
  - The publisher drops messages while disconnected.
  - Database changes and the messages about them are not saved together, and commands that save several times are not transactional.
- **Outcome:**
  - Commands commit their database changes and outgoing messages atomically (unit of work plus transactional outbox).
  - Consumers acknowledge after successful handling, retry a bounded number of times, send poison messages to a dead-letter queue, and ignore duplicates.
  - The publisher uses confirms.
- **Evidence:**
  - `BasicConsumeAsync(..., true, ...)` (auto-ack) in `src/Services/Harmony.Automations/Services/Hosted/AutomationNotificationsConsumerHostedService.cs:126`, `src/Services/Harmony.Notifications/Services/Hosted/EmailNotificationsConsumerHostedService.cs:181`, `SearchIndexNotificationsConsumerHostedService.cs:142` and `src/Services/Harmony.SignalR/Services/Hosted/PushNotificationsConsumerHostedService.cs:369`
  - `src/Infrastructure/Harmony.Messaging/RabbitMQNotificationPublisher.cs`
  - Every repository calls `SaveChangesAsync` itself.
- **Notes:** ADR: a library with outbox, retry and DLQ (MassTransit, Wolverine) or a hand-rolled one. Amends the 2026-11-25 messaging ADR. Update `overview.md`.

### R-24 Validation and error contract

- **Problem:** Bad input reaches the handlers.
  - FluentValidation is referenced, but there are no validators and no validation step.
  - Several endpoints ignore the ID in the route and use the one in the body.
  - Missing entities cause crashes instead of "not found".
  - Every failure comes back as HTTP 200 with `Succeeded=false`, so clients and monitoring cannot tell errors apart.
- **Outcome:**
  - A validation pipeline step runs validators for every command.
  - Route IDs must match body IDs.
  - Missing entities return 404, invalid input returns 400 with ProblemDetails, and unexpected errors return 500 through one exception handler.
- **Evidence:**
  - `src/Core/Harmony.Application/Harmony.Application.csproj` (FluentValidation referenced, zero validators)
  - `src/Services/Harmony.Api/Controllers/Management/BoardController.cs`, `src/Services/Harmony.Automations/Controllers/AutomationsController.cs` (route ID ignored)
  - `UpdateCardTitleCommandHandler.cs:54-56` (null card)
- **Notes:** Changes the HTTP contract. The Blazor client's `ResultExtensions.ToResult` must handle non-2xx responses.

## Phase 3: Agile model done right

### R-25 Issue hierarchy: Epic, Story, Subtask

- **Problem:** In Harmony an Epic is just a name on an issue. Stories and tasks cannot belong to an epic, so teams cannot group work, see epic progress or plan by epic. The only parent and child link is a subtask.
- **Outcome:**
  - Issue types have a hierarchy level: Epic, then Story/Task/Bug, then Subtask.
  - Stories, tasks and bugs can belong to an epic, and the rules are enforced: an epic cannot be a child, a subtask cannot have children, and there are no cycles.
  - The backlog has an epic panel and an epic filter, and each epic shows its progress (issues and points done).
- **Evidence:**
  - `src/Core/Harmony.Domain/Entities/IssueType.cs` (only a `Summary`)
  - `src/Core/Harmony.Domain/Entities/Card.cs:35-39` (`ParentCardId`)
  - `CreateChildIssueCommandHandler.cs:89-90,101` (children are always SubTask)
- **Notes:**
  - Track L: an epic intent and spec plus an ADR on the hierarchy model (an epic link field or levels on `ParentCardId`), then slices: model and rules, API, backlog UI, epic progress.
  - Update `harmony-docs/guide/scrum/` and `guide/boards/cards/`.

### R-26 Issue history

- **Problem:** Harmony keeps only a thin activity log. Moves between lists, sprint changes, story-point, assignee and type changes are not recorded. Teams cannot see what happened to an issue, and reports cannot compute cycle time or rebuild past states.
- **Outcome:** Every change to an issue's list or status, sprint, story points, assignees, issue type, parent and title is recorded with who and when, shown in the issue's history tab, and queryable for reports.
- **Evidence:** `src/Core/Harmony.Domain/Enums/CardActivityType.cs`.
- **Notes:** The basis for R-28 and future cycle-time and cumulative-flow reports.

## Phase 4: Jira-like features

### R-27 Priority, assignee and resolution

- **Problem:** Issues have no priority, so the team cannot say what matters most. "Assignee" is a list of members with no single owner, separate from the reporter. Done issues have no resolution, so "won't do" and "duplicate" cannot be told apart from "done".
- **Outcome:** Issues have:
  - A priority (Highest to Lowest, with a board default).
  - One assignee, separate from the reporter, with members kept as collaborators.
  - A resolution when they reach Done.
  - All three are shown on cards, editable in the issue view, and filterable on Work Items.
- **Evidence:** `src/Core/Harmony.Domain/Entities/Card.cs` (no priority or resolution; `Members` only).
- **Notes:** Data model and API change. Update `harmony-docs/guide/boards/cards/`.

### R-28 Velocity chart and sprint report

- **Problem:** Teams cannot see how much they usually finish per sprint, or what happened in a sprint: what was completed, what was not, and what was added or removed. Sprint planning is guesswork.
- **Outcome:**
  - A velocity chart shows committed versus completed points for the last N sprints.
  - A sprint report lists completed, not completed, added and removed issues for any completed sprint.
- **Evidence:** `src/Infrastructure/Harmony.Infrastructure/Services/Management/SprintService.cs` (burndown and issue-type donut only).
- **Notes:** Needs R-19 (scope snapshot) and R-26 (history). Update `harmony-docs/guide/scrum/sprints.md`.

### R-29 Backlog planning view

- **Problem:** The backlog is a flat table with up and down buttons. Teams cannot rank many issues quickly, plan several sprints side by side, edit many issues at once, or see whether a sprint is over capacity.
- **Outcome:**
  - The backlog page shows upcoming sprints as panels above the backlog.
  - Issues are ranked and moved by drag and drop, using a rank that does not renumber every issue.
  - Many issues can be edited at once (sprint, assignee, labels, priority).
  - Each sprint panel shows its committed points against the team's average velocity.
- **Evidence:** `src/Web/Harmony.Client/Pages/Management/Backlog.razor:63-73`.
- **Notes:** Builds on R-15 and R-22 (ranking), R-25 (epic panel) and, for capacity, R-28. Update `harmony-docs/guide/scrum/backlog.md`.

### R-30 Watchers and @mentions

- **Problem:** People only hear about an issue if they are assigned to it. Others who care cannot follow it, and a comment cannot ask a specific person for input.
- **Outcome:**
  - Users can watch and unwatch issues; reporters and assignees watch automatically.
  - Typing `@` in a comment or description suggests board members and notifies the mentioned person.
  - Watchers and mentioned users get in-app and email notifications, according to their notification settings.
- **Evidence:** Notifications today are a fixed set of email and in-app types (`src/Services/Harmony.Notifications`).
- **Notes:** Needs R-08 (only board members can be mentioned) and R-23 (notifications must not be lost).

---

## Later (not in the 30)

Recorded so they are not lost. Promote one into the list when its turn comes.

- **Performance and code health:**
  - N+1 queries when loading workspaces and boards (`GetUserWorkspacesHandler`, `BoardService.GetUserBoards`, `AlgoliaSearchService`)
  - Pass `CancellationToken` through handlers and repositories
  - Sensitive-data logging only in Development
  - Nullable warnings and an `.editorconfig` (675-warning baseline)
  - Liveness probes that do not check the database
  - Run migrations as a job instead of at startup in every replica
  - Client `async void` and `ContinueWith` misuse
  - Remove the unused SQL Server-only `LoadBoardOld` code
- **Known defects found by R-01:** each behaviour defect is pinned by the `_KnownIssue` test named on its line in `Harmony.Application.UnitTests`. The fix changes that test on purpose. See [spec #5](features/5-test-foundation/spec.md), Flagged concern 4.
  - Card date display ignores the year when the months match: 2026-01-05 to 2027-01-10 shows `Jan 05 - 10` (`CardHelper.DisplayDates`, FR-008). Pinned by `CardHelperDisplayDatesTests.DisplayDates_SameMonthDifferentYear_OmitsYear_KnownIssue`.
  - Card date display uses the server's culture for month names, so emails from Notifications depend on how the server is configured (`CardHelper.DisplayDates`, FR-008). Pinned by `CardHelperDisplayDatesTests.DisplayDates_GreekCulture_UsesGreekMonthName_KnownIssue`.
  - The card key is not found when followed by `.` or `,`. The GitHub integration reads keys from branch and ref names, so a branch such as `feature/HAR-42.fix` links no card (`CardHelper.GetSerialKey`, FR-007). Related defects in the same routine: #7. Pinned by `CardHelperGetSerialKeyTests.GetSerialKey_KeyFollowedByPeriod_ReturnsNull_KnownIssue`.
  - Card key extraction throws on null text (`CardHelper.GetSerialKey`, FR-007). Pinned by `CardHelperGetSerialKeyTests.GetSerialKey_NullText_Throws_KnownIssue`.
  - The issue-type filter throws on an issue type with no summary (`IssueTypeExtensions.Normal`, FR-012). Pinned by `IssueTypeExtensionsTests.Normal_TypeWithoutSummary_Throws_KnownIssue`.
  - The `CardDto → CardCreatedMessage` mapping is never used, because both handlers construct the message directly. Remove it in R-14. There is no behaviour to pin, so it has no test.
- **Package moves:** EasyCaching to HybridCache, `Serilog.Sinks.Elasticsearch` to `Elastic.Serilog.Sinks`, Newtonsoft.Json to System.Text.Json in the publisher, MediatR replacement.
- **Jira features:** components, versions and releases, time tracking, custom fields, configurable workflows and transitions, JQL-like and saved filters, dashboards, cumulative flow and control charts, auto-create subtasks automation.
- **From the GitBook roadmap:** an Elasticsearch-backed search service, and Gmail and Facebook login.
