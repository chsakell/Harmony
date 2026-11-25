---
title: Use .NET Aspire for local orchestration and observability, keeping Docker Compose and Kubernetes for deployment
status: accepted
date: 2026-11-25
deciders: chsakell
work-item: "#1"
supersedes:
superseded-by:
---

# Use .NET Aspire for local orchestration and observability, keeping Docker Compose and Kubernetes for deployment

## Context

Recorded after the fact. Aspire was added in November 2026 with the move to .NET 10 (commits "aspire integration" and "handle aspire dependencies between applications"). Before that, local development meant either the manual setup (install SQL Server, MongoDB, RabbitMQ and Redis, set connection strings, start each service) or Docker Compose for the whole stack.

[2026-11-25-microservices-grpc-and-rabbitmq.md](2026-11-25-microservices-grpc-and-rabbitmq.md) means seven .NET processes and four or five infrastructure containers. They have startup dependencies: the Api must migrate the database before Automations and Notifications start, and the gateway needs every route target.

## Decision

`src/Aspire/Harmony.AppHost` (Aspire 13.6.5) is the recommended way to run Harmony locally (`aspire run` or `dotnet run --project src/Aspire/Harmony.AppHost`).

- It starts SQL Server or PostgreSQL, MongoDB, RabbitMQ, Redis and, optionally, Elasticsearch with Kibana as persistent containers on named volumes (`harmony-*-data`).
- It generates the passwords once and keeps them in the AppHost user secrets.
- It injects configuration into each service as environment variables, in the shape the services already read (`ConnectionStrings__*`, `BrokerConfiguration__*`, `MongoDB__ConnectionURI`, ...). Service code therefore does not depend on Aspire client integrations.
- It starts the services in dependency order, using `WaitFor` on each service's `/healthz/ready`.
- Each service keeps its launchSettings HTTPS port, so `ocelot.json` and the client's `gatewayUrl` work unchanged.
- `Harmony.ServiceDefaults` adds OpenTelemetry (OTLP to the Aspire dashboard), service discovery, HTTP resilience and default health checks to every service.

Docker Compose and the Kustomize manifests in `.k8s/` stay the deployment path. Aspire is not used to generate deployment artifacts (inferred - confirm).

## Alternatives considered

| Option | Pros | Cons | Why not chosen |
|---|---|---|---|
| Docker Compose only for local development | Already exists; same images as deployment | Rebuilds images for every code change; no F5 debugging across services; no integrated traces | Still available; Aspire gives a faster inner loop with the debugger and the dashboard |
| Manual setup (local installs, multiple startup projects) | No container runtime needed | Many manual steps; connection strings and start order must be right by hand; no shared telemetry | Error-prone onboarding; kept in the README as a fallback |
| Aspire client integrations (`AddSqlServerDbContext`, `AddRabbitMQClient`, ...) inside the services | Less configuration code; built-in health checks and telemetry per resource | Changes every service's startup and couples it to Aspire, including under Compose and Kubernetes | Environment-variable injection kept the services unchanged and deployable without Aspire |

## Consequences

- **Positive:**
  - One command, or F5, starts the whole system with the right start order.
  - Distributed traces, metrics and logs appear in one dashboard.
  - Data survives restarts.
  - Switching the database is one setting (`Harmony:DatabaseProvider`: `SqlServer`, `PostgreSQL` or `LocalSqlServer`).
- **Negative:**
  - Local development requires Docker and the Aspire workload or CLI.
  - Two run configurations, Aspire and Compose/Kubernetes, must be kept in step when a service, port or setting changes.
  - The generated passwords are tied to the persisted volumes: losing the user secrets means recreating the volumes.
- **Follow-up work:** consider whether Aspire should also generate the Compose or Kubernetes manifests, so there is only one place to change.
