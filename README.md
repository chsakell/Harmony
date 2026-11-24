# Harmony

The source code of the [Harmony](https://chsakell.gitbook.io/harmony/) project management tool.

> Manage your team projects with this free to use, simple yet powerfull management tool.

<a href="https://chsakell.gitbook.io/harmony/configuration/dependencies" target="_blank"><img src="https://4051864592-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F9FS3EgJIfGPiZJAR9LaG%2Fuploads%2FIo7zVQE4xKC1af3GtQyy%2Fharmony-architecture.gif?alt=media&token=0fc3a580-b675-494f-b772-c6a544bfe55" alt="Harmony Architecture" width="600"></a>

## Features
- [Kanban](https://chsakell.gitbook.io/harmony/guide/kanban) boards
- [Scrum](https://chsakell.gitbook.io/harmony/guide/scrum) projects with sprints & retrospectives
- [Automations](https://chsakell.gitbook.io/harmony/guide/automations)
- GitHub [integration](https://chsakell.gitbook.io/harmony/integrations/github)
- Instant updates across all connected clients
- **.NET Aspire** local orchestration with end-to-end **OpenTelemetry** tracing :telescope:
- **Docker** & **Kubernetes** support :ship: :rocket:

### Stack
| **Databases** 	| **Server** 	| **Front** 	|
|---------------	|------------	|-----------	|
| SQL Server or PostgreSQL 	| **.NET 10**   	| Blazor    	|
| MongoDB       	| SignalR    	| MudBlazor 	|
| Redis         	| gRPC       	|           	|

| **Data access** 	| **Patterns**       	| **Messaging** 	|
|-----------------	|--------------------	|---------------	|
| EF Core         	| Clean **Microservice** Architecture 	| RabbitMQ      	|
|                 	| CQRS MediatR        	|               	|

| **Orchestration & observability** |
|-----------------------------------|
| .NET Aspire, OpenTelemetry, Serilog (+ optional Elasticsearch / Kibana) |

## Documentation
Docs are maintained at [docs.harmony-teams.com](https://chsakell.gitbook.io/harmony/)

## Quick start with .NET Aspire (recommended)
All you need is the **.NET 10 SDK** and **Docker Desktop** running.

```powershell
dotnet dev-certs https --trust
aspire run                                    # or: dotnet run --project src/Aspire/Harmony.AppHost
```

Or open `Harmony.slnx` and run **Harmony.AppHost** as the single startup project (F5).

- The AppHost starts SQL Server (or PostgreSQL), MongoDB, RabbitMQ and Redis as containers, plus every Harmony service, and opens the Aspire dashboard (resources, logs, distributed traces, metrics).
- Database data lives in named Docker volumes (`harmony-*-data`) and survives restarts. Generated passwords are kept in the AppHost user secrets.
- Switch the database with the `https-postgres` / `https-local-sqlserver` launch profiles or `Harmony:DatabaseProvider` (`SqlServer` | `PostgreSQL` | `LocalSqlServer`) in `src/Aspire/Harmony.AppHost/appsettings.json`.
- Open Harmony at https://localhost:7096 and login with default user _administrator_ or _johndoe_ using the password: __Pa$$w0rd!__

See the [.NET Aspire](https://chsakell.gitbook.io/harmony/configuration/aspire) guide for details.

## Docker & Kubernetes
```powershell
docker compose build
docker compose up -d                                                            # SQL Server
docker compose -f docker-compose.yml -f docker-compose.postgres.yml up -d       # PostgreSQL
kubectl apply -k .k8s/overlays/sqlserver                                        # Kubernetes, see .k8s/README.md
```
Harmony runs at http://localhost:7096 and traces are available in the Aspire dashboard at http://localhost:18888. See [docs/docker.md](docs/docker.md).

## Manual installation (without Aspire)
- For a detailed walkthrough on how to install and configure the dependencies, go through the [dependencies](https://chsakell.gitbook.io/harmony/configuration/dependencies) guide of the docs.
- In a nutshell, all you need to do is have **SQL Server** or **PostgreSQL**, **MongoDB** & **RabbitMQ** installed on your machine and configure all connection strings to point to these instances.
- By default, the connection strings use the default ports for all above installations.
- Redis is optional and should be used in production environment in order to configure the SignalR Redis backplane & the Redis caching.
- Configure the startup projects as explained on the [before-running](https://chsakell.gitbook.io/harmony/configuration/before-running) guide.
- Login with default user _administrator_ or _johndoe_ using the password: __Pa$$w0rd!__

## Contribute
You are more than welcomed to contribute on Harmony's evolvement. Harmony will continue to grow in both technical & product directions so feel free to join this amazing journey. Read the [CONTRIBUTING](https://github.com/chsakell/Harmony/blob/main/CONTRIBUTING.md) section for more details.

## Support me
You can support me :wave: using a [Sponsorship](https://github.com/sponsors/chsakell) or simply buy me a coffee :coffee: :pray:

<a href="https://www.buymeacoffee.com/chsakell" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>
