# Harmony

> Bring harmony to your teams with the `Harmony` __Project management tool__. 

![Kanban](./images/harmony_full_board_12_light.png)

__Always prefer Harmony's [official documentation](https://chsakell.gitbook.io/harmony/) which always contain the latest docs.__

## Features

1. Create __workspaces__ to organize different departements in your organization _(e.g. Engineering, Marketing, etc)_
2. Create __Kanban Boards__ to track your project's progress
3. Create __Scrum Projects__ for agile development
4. Instant updates across all connected members

### KANBAN features
* Create boards
* Add lists to boards _(e.g. TODO, IN PROGRESS, DONE etc)_
* Add cards to lists
* __Reorder__ or move cards between lists
* Add description to card with integrated Text Editor
* Set __due date__ to cards
* Assign team members to cards
* Add __labels__ to cards, predefined or create new ones
* Add __check lists__ to cards
* Add __check list items__ to check lists
* Mark check list item as completed or set a __due date__
* Delete check lists
* Upload __attachments__ to cards, images or files
* Archive cards
* Display card's activity
* View board's activity

### SCRUM features
* Create sprints
* Create backlog
* Move items between backlog & sprints
* Start sprints
* Complete sprints and move pending items to backlog/existing or new sprint
* All Kanban board features mentioned above

### User access features
* User registration and authentication
* Search and add members to workspaces & boards
* Workspace visibility options: Public & Private
* Board visibility options: Private, Workspace, Public

## Setup

1. Setup the __SQL Server__ connection string inside __appsettings.json__ to point to your SQL Server instance.

```javascript
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=Harmony;Integrated Security=True;TrustServerCertificate=True"
  },
```

2. Set the `Harmony.Server` project as the startup project and run the application. This will create the database and run all __migrations__. Alternative you can open the `Package Manager Console`, select  the `src\Infrastructure\Harmony.Persistence` project as the Default project and run the following command:
```
Update-Database -Context HarmonyContext -StartUpProject Harmony.Server -v
```