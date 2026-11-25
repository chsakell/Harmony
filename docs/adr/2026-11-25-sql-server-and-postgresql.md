---
title: Support both SQL Server and PostgreSQL, with one EF Core migrations project per provider
status: accepted
date: 2026-11-25
deciders: chsakell
work-item: "#1"
supersedes:
superseded-by:
---

# Support both SQL Server and PostgreSQL, with one EF Core migrations project per provider

## Context

Recorded after the fact. Harmony started on SQL Server only. PostgreSQL support was added in May 2024 ("add option to use PostgreSQL instead of SQL Server"). Harmony is free and self-hosted, so the people running it choose their own infrastructure. PostgreSQL has no licence cost and runs everywhere, while SQL Server suits teams that already run it (inferred - confirm).

EF Core migrations are specific to a provider: the generated SQL and the model snapshot differ between SQL Server and Npgsql. A single migrations folder cannot serve both.

## Decision

The relational provider is selected at startup by the `DatabaseProvider` setting (`SqlServer`, the default, or `PostgreSQL`), separately for each service that uses a relational database: Api, Automations and Notifications.

- Migrations live in two projects, `Harmony.Persistence.Migrations.SqlServer` and `Harmony.Persistence.Migrations.PostgreSql`. Each `DbContext` registration points `MigrationsAssembly` at the one that matches the provider.
- Every schema change adds a migration to **both** projects.
- Hangfire storage follows the same switch (`Hangfire.SqlServer` or `Hangfire.PostgreSql`).
- With PostgreSQL, `Npgsql.EnableLegacyTimestampBehavior` is turned on so `DateTime` values keep their SQL Server semantics.
- Docker Compose (`docker-compose.postgres.yml`), Kubernetes (`.k8s/overlays/sqlserver`, `.k8s/overlays/postgres`) and the Aspire AppHost (`Harmony:DatabaseProvider`) each offer both options.

## Alternatives considered

| Option | Pros | Cons | Why not chosen |
|---|---|---|---|
| SQL Server only | One migration set, provider-specific features (stored procedures, `[dbo]` T-SQL) usable freely | Licence cost and heavier footprint for self-hosters; no PostgreSQL shops | PostgreSQL support was requested or wanted for self-hosting (inferred - confirm) |
| PostgreSQL only (migrate everyone) | One migration set, free | Breaks existing SQL Server installations; needs a data migration path | Existing SQL Server users would have to migrate data (inferred - confirm) |
| One migrations project with provider-specific code (`#if` or runtime branches in migrations) | Single project | EF tooling generates one snapshot per context, so multi-provider migrations need manual edits; easy to get wrong | Separate assemblies are the approach the EF Core documentation recommends for multiple providers |

## Consequences

- **Positive:** self-hosters choose their database; both run as containers locally and in Kubernetes.
- **Negative:**
  - Every schema change must be generated and tested twice.
  - Raw SQL is tied to a provider: `BoardService.LoadBoardOld` and `LoadBoardListCardsOld` call SQL Server stored procedures through `SqlConnection`. Today nothing calls these methods except the interface that declares them.
  - `Harmony.Notifications` registers a SQL Server-only `HarmonyContext` (`AddHarmonyDatabase`), which is also unused.
  - The legacy timestamp switch postpones moving to `timestamptz`.
- **Follow-up work:**
  - remove the unused SQL Server-only methods, or make them work on both providers;
  - add tests that apply both migration sets (there are no test projects yet);
  - state in [../migrations.md](../migrations.md) that every schema change needs a migration in both projects. It lists the commands for each provider but not that rule.
