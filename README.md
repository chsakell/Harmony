# Harmony

The source code of the Harmony project management tool.

<a href="https://docs.harmony-teams.com/configuration/dependencies" target="_blank"><img src="https://4051864592-files.gitbook.io/~/files/v0/b/gitbook-x-prod.appspot.com/o/spaces%2F9FS3EgJIfGPiZJAR9LaG%2Fuploads%2FIo7zVQE4xKC1af3GtQyy%2Fharmony-architecture.gif?alt=media&token=0fc3a580-b675-494f-b772-c6a544bfe55" alt="Harmony Architecture" width="600"></a>

## Features
- [Kanban](https://docs.harmony-teams.com/guide/kanban) boards
- [Scrum](https://docs.harmony-teams.com/guide/scrum) projects
- Retrospectives
- [Automations](https://docs.harmony-teams.com/guide/automations)
- GitHub [integration](https://docs.harmony-teams.com/integrations/github)

### Stack
| **Databases** 	| **Server** 	| **Front** 	|
|---------------	|------------	|-----------	|
| SQL Server    	| .NET 8.0   	| Blazor    	|
| MongoDB       	| SignalR    	| MudBlazor 	|
| Redis         	| gRPC       	|           	|

| **Data access** 	| **Patterns**       	| **Messaging** 	|
|-----------------	|--------------------	|---------------	|
| EF Core         	| Clean **Microservice** Architecture 	| RabbitMQ      	|
|                 	| CQRS MediaR        	|               	|

**Docker** & **Kubernetes** support :ship: :rocket:

## Documentation
Docs are maintained at [docs.harmony-teams.com](https://docs.harmony-teams.com/)


## Support me
You can support me :wave: using a [Sponsorship](https://github.com/sponsors/chsakell) or simply buy me a coffee :coffee: :pray:

<a href="https://www.buymeacoffee.com/chsakell" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>
