---
title: Split Harmony into microservices behind an Ocelot gateway, using gRPC for queries and RabbitMQ for events
status: accepted
date: 2026-11-25
deciders: chsakell
work-item: "#1"
supersedes:
superseded-by:
---

# Split Harmony into microservices behind an Ocelot gateway, using gRPC for queries and RabbitMQ for events

## Context

Recorded after the fact. Harmony began in September 2023 as a single API with a Blazor client. RabbitMQ was added in November 2023, gRPC between services in January 2024, and the Ocelot gateway in February 2024, when the services moved under `src/Services` ("move microservices to services folder"). The README describes the style as "Clean Microservice Architecture".

Several features are side effects of a change to a card or board rather than part of the request: real-time updates to other clients, automations (sum up story points, sync parent and child issues), email notifications, search indexing, and GitHub activity. Some of them depend on external systems (Brevo, Algolia, GitHub) and on scheduled jobs (Hangfire). Running them in separate services keeps that work, and its failures, off the user's request path (inferred - confirm).

## Decision

Harmony runs as separate deployable services, each built with clean architecture on the shared `Core` and `Infrastructure` projects:

- **Harmony.Api** owns the Harmony relational database and the core REST API.
- **Harmony.Automations**, **Harmony.Notifications**, **Harmony.SignalR** and **Harmony.Integrations.SourceControl** each own one concern.
- **Harmony.ApiGateway** (Ocelot) is the only entry point for the Blazor client: it routes `/core`, `/automations`, `/sourcecontrol` and `/signalRHub`.
- **Asynchronous side effects** go through RabbitMQ: a `notifications` topic exchange with the queues `notifications.automation`, `notifications.signalr`, `email_notifications` and `search_index_notifications`. Each consuming service runs a hosted consumer.
- **Synchronous calls between services** use gRPC: Automations and Notifications call the Api, and the Api calls SourceControl and Automations.

## Alternatives considered

| Option | Pros | Cons | Why not chosen |
|---|---|---|---|
| Modular monolith: one host, in-process events (MediatR notifications) | One deployment, no broker, simpler debugging and transactions | Background work and external calls share the API's process and resources; modules cannot be scaled or deployed on their own | Not chosen; scaling and isolating the side-effect workloads was preferred (inferred - confirm) |
| Microservices with REST between services | One protocol everywhere, easy to inspect | No generated, strongly typed contracts; JSON is heavier on hot paths | gRPC chosen for typed `.proto` contracts and performance (inferred - confirm) |
| A different broker or a managed bus (Azure Service Bus, Kafka), or MassTransit on top of RabbitMQ | Managed operations (Service Bus); replay (Kafka); retries, outbox and sagas built in (MassTransit) | Cloud lock-in or heavier infrastructure for a self-hosted product; another abstraction layer | RabbitMQ runs anywhere as a container, which suits self-hosting (inferred - confirm) |
| YARP instead of Ocelot | Actively developed by Microsoft, high performance | Routing configured in code or JSON in a different way; less built-in Swagger aggregation | Ocelot was chosen in February 2024; MMLib.SwaggerForOcelot aggregates each service's Swagger (inferred - confirm) |

## Consequences

- **Positive:** side effects are decoupled and retried independently of user requests. Each service can be scaled and deployed on its own (Docker Compose, Kubernetes). The `.proto` files in `Harmony.Application` give typed contracts between services.
- **Negative:**
  - Seven processes plus SQL Server or PostgreSQL, MongoDB, RabbitMQ and Redis must run to develop locally (mitigated by [2026-11-25-aspire-local-orchestration.md](2026-11-25-aspire-local-orchestration.md)).
  - The Api and Automations call each other over gRPC, so only one direction can be a startup dependency.
  - Messages are published straight after the database write, with no outbox, so a crash between the two can lose an event (inferred - confirm).
  - The gRPC clients accept any server certificate, and CORS allows any origin.
  - The gateway does not validate tokens; the Api does.
- **Follow-up work:** consider a transactional outbox for broker events; validate gRPC certificates outside Development; decide whether the gateway should authenticate requests. These are listed under Known issues in [../architecture/overview.md](../architecture/overview.md).
