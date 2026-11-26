# Harmony

Open-source project management tool (Kanban boards, Scrum sprints, automations, GitHub integration) built as .NET 10 microservices behind an Ocelot gateway with a Blazor WebAssembly client, for teams that self-host it.

## Commands

Run these from the repository root. All must pass before any task is reported complete; paste their output.

- Build: `dotnet build Harmony.slnx` - healthy output ends with `0 Error(s)`. The baseline is 669 warnings (see Known issues in the overview); do not add new ones.
- Test: `dotnet test --solution Harmony.slnx` - all tests pass; never skip or delete a failing test. `global.json` opts into Microsoft.Testing.Platform (xUnit v3); test projects live in `tests/`. Run one requirement's tests with `dotnet test --solution Harmony.slnx --filter-trait "Requirement=FR-006"`. Every run writes a Cobertura coverage report to `TestResults/<project>/coverage.cobertura.<timestamp>.xml` (git-ignored) with no extra options. To write results elsewhere, pass `-p:HarmonyTestResultsDirectory=<dir>`. Do not use `--results-directory`: `tests/Directory.Build.props` already passes it, so a second one fails with exit code 5 and runs no tests.
- Format: `dotnet format Harmony.slnx --verify-no-changes --include <changed files>` - exit code 0 for the files you changed. The whole solution does not pass yet (1,017 whitespace diagnostics in 220 files); do not reformat files you did not change.
- Run: `dotnet run --project src/Aspire/Harmony.AppHost` (needs Docker running) - ready when the log shows `Distributed application started` and `https://localhost:7108/healthz/ready` (gateway) returns 200; the client is at https://localhost:7096. Each service has `/healthz/ready` on its launchSettings https port.

If a test fails, fix the code, not the test.

## Architecture

Clean architecture per service; dependencies point inwards (Domain <- Application <- Infrastructure/Persistence <- hosts).

- `src/Core/Harmony.Domain` - entities and enums; no dependencies.
- `src/Core/Harmony.Application` - CQRS commands/queries (MediatR), validation, contracts, gRPC protos, broker constants; depends on Domain and Shared.
- `src/Core/Harmony.Application.SourceControl` - source-control (GitHub) features; depends on Application.
- `src/Shared/Harmony.Shared` - constants, localization and helpers shared by server and client; no project dependencies.
- `src/Infrastructure/Harmony.Persistence` - EF Core `HarmonyContext`, ASP.NET Identity, JWT; depends on Application and Domain.
- `src/Infrastructure/Harmony.Persistence.Migrations.SqlServer` / `.PostgreSql` - EF Core migrations per provider; depend on Persistence.
- `src/Infrastructure/Harmony.Infrastructure` - repositories (EF Core, Dapper, MongoDB), Algolia search, gRPC clients; depends on Application and Persistence.
- `src/Infrastructure/Harmony.Messaging` - RabbitMQ publisher and health check; depends on Application.
- `src/Infrastructure/Harmony.Caching` - EasyCaching (in-memory or Redis); depends on Application.
- `src/Infrastructure/Harmony.Logging` - Serilog setup with optional Elasticsearch sink; no project dependencies.
- `src/Services/Harmony.Api` - core REST + gRPC API, owns the Harmony database; references Application and all Infrastructure projects.
- `src/Services/Harmony.Automations` - automation rules (MongoDB) and Hangfire jobs, consumes broker events.
- `src/Services/Harmony.Notifications` - email (Brevo) and search-index notifications via Hangfire, consumes broker events.
- `src/Services/Harmony.SignalR` - real-time push hub with Redis backplane, consumes broker events.
- `src/Integrations/Harmony.Integrations.SourceControl` - GitHub webhooks and source-control activity (MongoDB), gRPC server.
- `src/ApiGateways/Harmony.ApiGateway` - Ocelot gateway: `/core`, `/automations`, `/sourcecontrol`, `/signalRHub`.
- `src/Web/Harmony.Client` (+ `Harmony.Client.Infrastructure`) - Blazor WebAssembly UI (MudBlazor); talks only to the gateway.
- `src/Aspire/Harmony.AppHost` / `Harmony.ServiceDefaults` - local orchestration and shared OpenTelemetry, health and resilience defaults.
- `.k8s/`, `docker-compose*.yml` - container and Kubernetes deployment.

How the system works today: [docs/architecture/overview.md](docs/architecture/overview.md). Decisions and their reasons: [docs/adr/](docs/adr/).

## Standards

The organization's .NET standards (`dotnet-instructions`) are not installed in this repository yet: there is no `.dotnet-instructions.lock.json`, `.github/instructions/` or `.claude/rules/`. Until they are, follow the conventions of the surrounding code.

## How we work

This repository follows the AI-native .NET SDLC (`docs/dotnet-sdlc.md`, kept locally and git-ignored). Work items are GitHub issues, not Azure DevOps.

- Backlog: [docs/roadmap.md](docs/roadmap.md). When you start an item, open a GitHub issue and fill in the item's Issue and Status columns in the same pull request.
- Feature work lives in `docs/features/<issue>-<slug>/`: `intent.md`, then `spec.md`, then `plan.md`. Use `/sdlc-intent`, `/sdlc-spec`, `/sdlc-plan`, `/sdlc-implement`, `/sdlc-verify`, `/sdlc-review`, `/sdlc-pr`.
- Do not change code under `src/` or `tests/` for a feature until its `plan.md` has `status: approved`.
- Keep `plan.md` in sync: tick tasks and log deviations in the same commit as the code.
- Tag every test with the requirement it proves: `[Trait("Requirement", "FR-001")]`.
- Commits follow Conventional Commits with the issue in the footer: `Refs #<id>` (a footer line starting with `#` is dropped by git as a comment). PR titles start with `#<id>`; PR bodies say `Fixes #<id>`.
- Branches follow GitFlow: `feature/<id>-<slug>` and `bugfix/<id>-<slug>` from `develop`, `hotfix/<id>-<slug>` from `main`. Never push to `main` or `develop` directly.

## Things agents get wrong

<!-- When an agent makes the same mistake twice, add a one-line rule here. Keep this file under one page. -->
