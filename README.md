# Harmony

The source code of the [Harmony](https://docs.harmony-teams.com/) project management tool.

> Manage your team projects with this free to use, simple yet powerfull management tool.

<a href="https://docs.harmony-teams.com/configuration/dependencies" target="_blank"><img src="https://4051864592-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F9FS3EgJIfGPiZJAR9LaG%2Fuploads%2FIo7zVQE4xKC1af3GtQyy%2Fharmony-architecture.gif?alt=media&token=0fc3a580-b675-494f-b772-c6a544bfe55" alt="Harmony Architecture" width="600"></a>

## Features
- [Kanban](https://docs.harmony-teams.com/guide/kanban) boards
- [Scrum](https://docs.harmony-teams.com/guide/scrum) projects with sprints & retrospectives
- [Automations](https://docs.harmony-teams.com/guide/automations)
- GitHub [integration](https://docs.harmony-teams.com/integrations/github)
- Instant updates across all connected clients
- **Docker** & **Kubernetes** support :ship: :rocket:

### Stack
| **Databases** 	| **Server** 	| **Front** 	|
|---------------	|------------	|-----------	|
| SQL Server or PostgreSQL 	| **.NET 8.0**   	| Blazor    	|
| MongoDB       	| SignalR    	| MudBlazor 	|
| Redis         	| gRPC       	|           	|

| **Data access** 	| **Patterns**       	| **Messaging** 	|
|-----------------	|--------------------	|---------------	|
| EF Core         	| Clean **Microservice** Architecture 	| RabbitMQ      	|
|                 	| CQRS MediatR        	|               	|

## Documentation
Docs are maintained at [docs.harmony-teams.com](https://docs.harmony-teams.com/)

## Installation
- For a detailed walkthrough on how to install and configure the dependencies, go through the [dependencies](https://docs.harmony-teams.com/configuration/dependencies) guide of the docs.
- In a nutchel, all you need to do is have **SQL Server** or **PostgreSQL**, **MongoDB** & **RabbitMQ** installed on your machine and configure all connection strings to point to these instances.
- By default, the connection strings use the default ports for all above installations.
- Redis is optional and should be used in production environment in order to configure the SignalR Redis backplane & the Redis caching.
- Configure the startup projects as explained on the [before-running](https://docs.harmony-teams.com/configuration/before-running) guide.

## Contribute
You are more than welcomed to contribute on Harmony's evolvement. Harmony will continue to grow in both technical & product directions so feel free to join this amazing journey. Read the [CONTRIBUTING](https://github.com/chsakell/Harmony/blob/main/CONTRIBUTING.md) section for more details.

## Support me
You can support me :wave: using a [Sponsorship](https://github.com/sponsors/chsakell) or simply buy me a coffee :coffee: :pray:

<a href="https://www.buymeacoffee.com/chsakell" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>
