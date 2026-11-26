# Architecture overview: Harmony

> The current state of the system. Update it in the same pull request as any change that alters what is written here. Marked "(inferred - confirm)" where it was derived from code and not yet confirmed by the team.

## Purpose and context

Harmony is a free, self-hosted project management tool: Kanban boards, Scrum projects with sprints and retrospectives, automations, and a GitHub integration, with instant updates across connected clients (README). Users work in a Blazor WebAssembly client; everything server-side is a set of .NET 10 services behind an Ocelot API gateway.

```mermaid
flowchart LR
    user([Team member]) --> client[Harmony.Client<br/>Blazor WASM]
    github([GitHub]) -- webhooks --> gw
    client --> gw[Harmony.ApiGateway<br/>Ocelot]
    gw -- /core --> api[Harmony.Api]
    gw -- /automations --> auto[Harmony.Automations]
    gw -- /sourcecontrol --> sc[Harmony.Integrations.SourceControl]
    gw -- /signalRHub --> sr[Harmony.SignalR]
    api <-- gRPC --> auto
    api <-- gRPC --> sc
    notif[Harmony.Notifications] -- gRPC --> api
    api --> sql[(SQL Server or PostgreSQL<br/>Harmony)]
    auto --> mongo[(MongoDB)]
    sc --> mongo
    auto --> jobsA[(Automations.Jobs DB<br/>Hangfire)]
    notif --> jobsN[(Notifications.Jobs DB<br/>Hangfire)]
    api -- publish --> mq{{RabbitMQ}}
    mq --> auto
    mq --> notif
    mq --> sr
    api --> redis[(Redis)]
    sr --> redis
    notif --> brevo([Brevo email])
    notif --> algolia([Algolia search])
```

## Building blocks

Dependencies are project references taken from the `.csproj` files.

| Project | Responsibility | Depends on |
|---|---|---|
| `Core/Harmony.Domain` | Entities and enums | - |
| `Core/Harmony.Application` | CQRS commands/queries (MediatR), FluentValidation, AutoMapper, gRPC protos, broker constants, contracts | Domain, Shared |
| `Core/Harmony.Application.SourceControl` | Source-control features (branches, pushes, pull requests) | Application |
| `Shared/Harmony.Shared` | Constants, localization, helpers shared with the client | - |
| `Infrastructure/Harmony.Persistence` | EF Core `HarmonyContext`, ASP.NET Identity, JWT bearer | Application, Domain |
| `Infrastructure/Harmony.Persistence.Migrations.SqlServer` / `.PostgreSql` | EF Core migrations per database provider | Persistence |
| `Infrastructure/Harmony.Infrastructure` | Repositories (EF Core, Dapper, MongoDB driver), Algolia, gRPC clients | Application, Persistence |
| `Infrastructure/Harmony.Messaging` | RabbitMQ publisher (`notifications` topic exchange), RabbitMQ health check | Application |
| `Infrastructure/Harmony.Caching` | EasyCaching, in-memory or Redis (`CacheProvider`) | Application |
| `Infrastructure/Harmony.Logging` | Serilog configuration, optional Elasticsearch sink | - |
| `Services/Harmony.Api` | Core REST API (controllers) and gRPC server (Card, UserCard, Board, Workspace, User, UserNotification services); owns the Harmony database; migrates and seeds it at startup | ServiceDefaults, Application, Caching, Infrastructure, Logging, Messaging, Persistence (+ migrations) |
| `Services/Harmony.Automations` | Automation rules in MongoDB, Hangfire jobs, gRPC `AutomationService`; consumes `notifications.automation` | ServiceDefaults, Application, Caching, Infrastructure, Logging, Messaging, migrations, Shared |
| `Services/Harmony.Notifications` | Email notifications (Brevo) and search-index updates via Hangfire; consumes `email_notifications` and `search_index_notifications` | ServiceDefaults, Application, Infrastructure, Logging, Messaging, Persistence (+ migrations) |
| `Services/Harmony.SignalR` | SignalR hub for real-time updates; Redis backplane; consumes `notifications.signalr` | ServiceDefaults, Application, Application.SourceControl, Logging, Messaging |
| `Integrations/Harmony.Integrations.SourceControl` | GitHub webhook endpoint (`GithubController`), source-control activity in MongoDB, gRPC `SourceControlService` | ServiceDefaults, Application(.SourceControl), Infrastructure, Logging, Messaging, Persistence |
| `ApiGateways/Harmony.ApiGateway` | Ocelot routing (`ocelot.json`, `ocelot.docker.json`) and aggregated Swagger | ServiceDefaults |
| `Web/Harmony.Client` + `Harmony.Client.Infrastructure` | Blazor WebAssembly UI (MudBlazor), JWT auth state in local storage, SignalR client; calls only `gatewayUrl` | Application, Application.SourceControl, Shared |
| `Aspire/Harmony.AppHost` | Local orchestration of containers and services, startup order | all hosts |
| `Aspire/Harmony.ServiceDefaults` | OpenTelemetry, service discovery, HTTP resilience, default health checks | - |

## Key flows

1. **A user changes a card.** The client calls the gateway (`/core/...`), which routes to `Harmony.Api`. A MediatR command handler in `Harmony.Application` updates the Harmony database and publishes a message to the `notifications` topic exchange in RabbitMQ.
2. **Fan-out of that change.** `Harmony.SignalR` consumes `notifications.signalr` and pushes the update to connected clients (Redis backplane). `Harmony.Automations` consumes `notifications.automation` and runs matching automations (for example sum up story points, sync parent and child issues), calling back into the Api over gRPC. `Harmony.Notifications` consumes `email_notifications` (emails via Brevo) and `search_index_notifications` (Algolia index).
3. **GitHub activity.** GitHub posts webhooks to `Harmony.Integrations.SourceControl` (through the gateway, `/sourcecontrol/...`) (inferred - confirm). It stores branches, pushes and pull requests in MongoDB and publishes broker events; the Api reads source-control data over gRPC.

## Data

- **Harmony database** (SQL Server or PostgreSQL, chosen by `DatabaseProvider`): owned by `Harmony.Api` through EF Core `HarmonyContext` and ASP.NET Identity. Migrations live in the two `Persistence.Migrations.*` projects and are applied at Api startup (`Database.Migrate()`), followed by seeding (default users).
- **Harmony.Automations.Jobs** and **Harmony.Notifications.Jobs** (same provider): Hangfire storage plus the services' own EF contexts (`AutomationContext`, `NotificationContext`), migrated at each service's startup.
- **MongoDB**: automation rules (Automations) and source-control activity (SourceControl).
- **Redis**: distributed cache (`CacheProvider=Redis`) and SignalR backplane.
- **Files**: card attachments are served from the Api's `Files/` folder (static files) (inferred - confirm).
- Retention: not defined in code.

## Integrations

| Direction | Counterpart | Mechanism | Owner |
|---|---|---|---|
| Inbound | Blazor client | REST + SignalR through the Ocelot gateway | Api, SignalR |
| Inbound | GitHub | Webhooks (`GithubController`) | Integrations.SourceControl |
| Internal | Api <-> Automations, Api <-> SourceControl, Notifications -> Api | gRPC (HTTPS, server certificate validation disabled) | each service |
| Internal | Api, SourceControl -> Automations, Notifications, SignalR | RabbitMQ: topic exchange `notifications`; queues `notifications.automation`, `notifications.signalr`, `email_notifications`, `search_index_notifications` | Messaging |
| Outbound | Brevo | Transactional email API (`BrevoSettings`); `GmailSettings` is also bound | Notifications |
| Outbound | Algolia | Search index (`AlgoliaConfiguration`); the Api itself searches the database (`SearchEngine.Database`) | Notifications |

## Cross-cutting concerns

- **Authentication and authorization:** ASP.NET Identity users in the Harmony database; the Api issues and validates JWT bearer tokens (`AppConfiguration:Secret`). The gateway has no `AuthenticationOptions` on its routes, so it forwards without validating tokens, and Automations, Notifications, SignalR and SourceControl do not configure authentication (inferred - confirm).
- **Observability:** `Harmony.ServiceDefaults` adds OpenTelemetry traces, metrics and logs (OTLP, the Aspire dashboard locally); Serilog writes to the console and optionally Elasticsearch (`ElasticConfiguration:Uri`, empty disables it). Every service exposes `/healthz/ready` (database, RabbitMQ or MongoDB checks) and `/healthz/live`.
- **Resilience:** Polly retry policies (`AddRetryPolicies`), `Microsoft.Extensions.Http.Resilience` from ServiceDefaults; the RabbitMQ publisher connects in the background with wait-and-retry.
- **Configuration and secrets:** `appsettings.json` / `appsettings.Production.json` per service with sections `ConnectionStrings`, `DatabaseProvider`, `BrokerConfiguration`, `MongoDB`, `RedisConnectionString`, `CacheProvider`, `AppEndpointConfiguration`, `AppConfiguration`, `AlgoliaConfiguration`, `BrevoSettings`, `GmailSettings`, `ElasticConfiguration`. Under Aspire, the AppHost injects these as environment variables and keeps generated passwords in its user secrets (`Parameters:*`). Docker Compose reads secrets from `.env` (see `.env.example`).
- **Localization:** resource files (`Resources/`) on the Api and SourceControl; the client picks the culture from stored preferences.

## Deployment

- **Local:** `aspire run` (or `dotnet run --project src/Aspire/Harmony.AppHost`) starts SQL Server or PostgreSQL, MongoDB, RabbitMQ, Redis and optionally Elasticsearch + Kibana as persistent containers with named volumes (`harmony-*-data`), then every service in dependency order (`WaitFor`). `Harmony:DatabaseProvider` selects `SqlServer` (default), `PostgreSQL` or `LocalSqlServer`. The client runs at https://localhost:7096.
- **Containers:** each host has a `Dockerfile`; `docker-compose.yml` (SQL Server) and `docker-compose.postgres.yml` (PostgreSQL override) run the full stack with an Aspire dashboard container.
- **Kubernetes:** Kustomize base in `.k8s/base` (apps and infra) with `sqlserver` and `postgres` overlays (`kubectl apply -k .k8s/overlays/<provider>`).
- **CI/CD:** no build or release pipeline in the repository. `.github/workflows/` holds only the SDLC workflows (`sdlc-gates.yml`, `copilot-setup-steps.yml`). Agent evals are not run in CI; run them locally with `./evals/run-evals.ps1`. How releases reach production and how they are rolled back is not described in the repository (inferred - confirm).

## Known issues and technical debt

Recorded from one run on 2026-11-25 (`dotnet build Harmony.slnx`, SDK 10.0.404); not fixed in the setup branch.

- **Unit tests cover only part of the Application layer** (added 2026-11-26, #5). `tests/Harmony.Application.UnitTests` (xUnit v3 on Microsoft.Testing.Platform, see the testing-strategy ADR) covers:
  - the card helpers, the issue-type filter and the query-filter builder;
  - the six AutoMapper mappings with custom rules, plus a ratchet on the 28 unmapped members;
  - one reference handler test (`UpdateCardTitleCommandHandler`).

  Still missing: a CI build that runs them (R-02), integration tests against the databases and the broker, and tests for the other services and the Blazor client.
- **669 build warnings, 0 errors.** Mostly nullable reference types (CS8618 x262, CS8602 x136, CS8632, CS8604, CS8625, CS8600, CS8603, CS8601), plus unused fields (CS0169, CS0414), CA2022 (inexact `Stream.ReadAsync` in `Account.razor.cs` and `EditCardModal.razor.cs`) and MUD0002 (attribute casing in `Board.razor`).
- **Vulnerable package:** `AutoMapper` 14.0.0 raises NU1903 for three high-severity denial-of-service advisories: [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x), [GHSA-6vhm-j9h7-f9gh](https://github.com/advisories/GHSA-6vhm-j9h7-f9gh) and [GHSA-gpwp-5797-mjff](https://github.com/advisories/GHSA-gpwp-5797-mjff). The fixes are only in the commercially licensed 15.1.x and 16.x releases.
  - Replacing AutoMapper is R-14.
  - `tests/Directory.Build.props` suppresses the three advisories for test projects only, so `TreatWarningsAsErrors` can stay on there. `src/` projects still report them.
- **Formatting:** `dotnet format --verify-no-changes` fails with 1,017 whitespace diagnostics in 220 files; there is no `.editorconfig`.
- **gRPC clients accept any server certificate** (`DangerousAcceptAnyServerCertificateValidator`) in the Api and Automations.
- **CORS allows any origin with credentials** (`SetIsOriginAllowed(_ => true)`) in the Api, SignalR and the gateway.
- **Unused SQL Server-only code:** `BoardService.LoadBoardOld` / `LoadBoardListCardsOld` (stored procedures through `SqlConnection`) and `AddHarmonyDatabase` in `Harmony.Notifications` are never called; they would fail on PostgreSQL.
- **No CI build** for the application itself.

## Decision index

| ADR | Decision | Status |
|---|---|---|
| [2026-11-25-microservices-grpc-and-rabbitmq](../adr/2026-11-25-microservices-grpc-and-rabbitmq.md) | Microservices behind an Ocelot gateway; gRPC for queries, RabbitMQ for events | accepted |
| [2026-11-25-sql-server-and-postgresql](../adr/2026-11-25-sql-server-and-postgresql.md) | Support SQL Server and PostgreSQL with one migrations project per provider | accepted |
| [2026-11-25-aspire-local-orchestration](../adr/2026-11-25-aspire-local-orchestration.md) | .NET Aspire for local orchestration and observability; Compose and Kubernetes for deployment | accepted |
| [2026-11-26-testing-strategy](../adr/2026-11-26-testing-strategy.md) | xUnit v3 on Microsoft.Testing.Platform with NSubstitute, Shouldly and coverlet.MTP; tests in a separate `tests/` tree | accepted |
