# Authorization matrix

Which `Harmony.Api` endpoints need a login, and which ones a user who is not a member of a workspace or board can still use. The Api tests check every line against the running Api (`dotnet test --project tests/Harmony.Api.IntegrationTests -p:HarmonyIntegrationTests=true`, see `AGENTS.md`). The rules and the format are in `docs/features/22-api-authorization-matrix/` (spec and plan).

- **Scope** says what the endpoint acts on:
  - `public`: open without a login, on purpose;
  - `self`: the caller's own data;
  - `resource`: a workspace, or a board and anything in it;
  - `administration`: roles and user administration;
  - `user`: another user's account or settings.
- **Anonymous** is the result with no login. `refused` means 401 is expected. `gap R-07` means the endpoint is open today and roadmap item R-07 closes it. Public endpoints have `-`.
- **Non-member** is the result for a logged-in user who is not a member. It applies to `resource` endpoints only. `refused` means 403 or 404 is expected, and `gap R-08` means it is open today and R-08 closes it.
- **Bindings** tell the tests which resource an ID names, when the name alone does not say (`name=kind`, and optionally a fixed `body=<json>` last).

A new endpoint fails the tests until it has a line here. Keep the lines sorted by Route, then Method (ordinal).

## Baselines

<!--
  WARNING: each baseline must equal the number of known-gap marks below, and the tests fail when they differ.
  Lowering a baseline closes a gap. RAISING ONE ADDS A KNOWN GAP: an endpoint that ships open. Review such a
  change as a security decision (spec, Flagged concern 9).
-->

- Anonymous known gaps: 94
- Non-member known gaps: 80

## Endpoints

| Route | Method | Scope | Anonymous | Non-member | Bindings |
|---|---|---|---|---|---|
| api/Board | POST | resource | gap R-07 | gap R-08 | body={"Title":"Matrix","Description":"Matrix","WorkspaceId":"","Visibility":0,"BoardType":0,"Key":"MTRX"} |
| api/Board/userboards | GET | self | refused | - |  |
| api/Board/{boardId:guid}/work-items | GET | resource | gap R-07 | gap R-08 |  |
| api/Board/{id:guid}/archived-items | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/backlog | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/boardlists | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/info | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/issuetypes | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/lists/{listId:guid} | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/load | POST | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/members | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/members | POST | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/members/search | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/members/{userId} | DELETE | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/members/{userId}/status | PUT | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/movecardstobacklog | PUT | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/movecardstosprint | PUT | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/positions | PUT | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/reactivatecards | PUT | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/sprints | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/sprints | POST | resource | gap R-07 | gap R-08 | id=board; body={"BoardId":"","SprintId":null,"Name":"Matrix","Goal":"Matrix","StartDate":"2026-01-01T00:00:00","EndDate":"2026-01-15T00:00:00"} |
| api/Board/{id:guid}/sprints/details | GET | resource | gap R-07 | gap R-08 | id=board |
| api/Board/{id:guid}/sprints/{sprintId:guid}/cards/pending | GET | resource | gap R-07 | gap R-08 | id=board |
| api/BoardLists/{id:guid}/title | PUT | resource | gap R-07 | gap R-08 | id=list |
| api/Cards | POST | resource | gap R-07 | gap R-08 | IssueType.Id=issuetype |
| api/Cards/backlog | POST | resource | gap R-07 | gap R-08 | IssueType.Id=issuetype |
| api/Cards/backlog/{id:guid} | PUT | resource | gap R-07 | gap R-08 | id=card; IssueType.Id=issuetype |
| api/Cards/search | GET | resource | gap R-07 | gap R-08 | skipCardId=card2 |
| api/Cards/{id:guid} | GET | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/activity | GET | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/attachments/{attachmentId:guid} | DELETE | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/childissue | POST | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/dates | PUT | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/description | PUT | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/issuetype | PUT | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/labels | GET | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/labels | PUT | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/links | POST | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/members | GET | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/members | POST | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/members/{userId} | DELETE | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/move | PUT | resource | gap R-07 | gap R-08 | id=card; UpdateId=new |
| api/Cards/{id:guid}/status | PUT | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/storypoints | PUT | resource | gap R-07 | gap R-08 | id=card |
| api/Cards/{id:guid}/title | PUT | resource | gap R-07 | gap R-08 | id=card |
| api/CheckListItems/{id:guid}/checked | PUT | resource | gap R-07 | gap R-08 | id=checklistitem |
| api/CheckListItems/{id:guid}/description | PUT | resource | gap R-07 | gap R-08 | id=checklistitem |
| api/CheckListItems/{id:guid}/duedate | PUT | resource | gap R-07 | gap R-08 | id=checklistitem |
| api/CheckLists | POST | resource | gap R-07 | gap R-08 |  |
| api/CheckLists/{id:guid} | DELETE | resource | gap R-07 | gap R-08 | id=checklist |
| api/CheckLists/{id:guid}/items | POST | resource | gap R-07 | gap R-08 | id=checklist |
| api/CheckLists/{id:guid}/title | PUT | resource | gap R-07 | gap R-08 | id=checklist; ListId=checklist |
| api/Comments | POST | resource | gap R-07 | gap R-08 |  |
| api/Comments/{cardId:guid} | GET | resource | gap R-07 | gap R-08 |  |
| api/Comments/{commentId:guid} | POST | resource | gap R-07 | gap R-08 |  |
| api/Comments/{commentId:guid} | PUT | resource | gap R-07 | gap R-08 |  |
| api/Files | POST | resource | refused | gap R-08 |  |
| api/Files/profile-picture | POST | self | refused | - |  |
| api/Labels | POST | resource | gap R-07 | gap R-08 |  |
| api/Labels/{id:guid} | DELETE | resource | gap R-07 | gap R-08 | id=label |
| api/Labels/{id:guid}/title | PUT | resource | gap R-07 | gap R-08 | id=label |
| api/Links/{id:guid} | DELETE | resource | gap R-07 | gap R-08 | id=link |
| api/Lists | POST | resource | gap R-07 | gap R-08 |  |
| api/Lists/{id:guid}/status | PUT | resource | gap R-07 | gap R-08 | id=list |
| api/Retrospectives | POST | resource | gap R-07 | gap R-08 |  |
| api/Search | GET | self | gap R-07 | - |  |
| api/Search/advanced | GET | self | gap R-07 | - |  |
| api/Search/advanced | POST | resource | gap R-07 | gap R-08 |  |
| api/Sprints/{id:guid} | GET | resource | gap R-07 | gap R-08 | id=sprint |
| api/Sprints/{id:guid}/cards | GET | resource | gap R-07 | gap R-08 | id=sprint |
| api/Sprints/{id:guid}/cards | POST | resource | gap R-07 | gap R-08 | id=sprint; IssueType.Id=issuetype |
| api/Sprints/{id:guid}/complete | POST | resource | gap R-07 | gap R-08 | id=sprint |
| api/Sprints/{id:guid}/reports | GET | resource | gap R-07 | gap R-08 | id=sprint |
| api/Sprints/{id:guid}/start | PUT | resource | gap R-07 | gap R-08 | id=sprint |
| api/Test | POST | self | gap R-07 | - |  |
| api/UserNotifications | POST | self | gap R-07 | - |  |
| api/UserNotifications/{userId} | GET | user | gap R-07 | - |  |
| api/Workspace | GET | self | gap R-07 | - |  |
| api/Workspace | POST | resource | gap R-07 | gap R-08 |  |
| api/Workspace/{id:guid} | GET | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/boards | GET | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/info | GET | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/members | GET | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/members/add | POST | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/members/remove | POST | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/members/search | GET | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/rename | PUT | resource | gap R-07 | gap R-08 | id=workspace |
| api/Workspace/{id:guid}/status | PUT | resource | gap R-07 | gap R-08 | id=workspace |
| api/identity/account/ChangePassword | PUT | self | refused | - |  |
| api/identity/account/UpdateProfile | PUT | self | refused | - |  |
| api/identity/account/profile-picture/{userId} | POST | self | refused | - |  |
| api/identity/role | GET | administration | gap R-07 | - |  |
| api/identity/role | POST | administration | gap R-07 | - |  |
| api/identity/role/permissions/update | PUT | administration | gap R-07 | - |  |
| api/identity/role/permissions/{roleId} | GET | administration | gap R-07 | - |  |
| api/identity/role/{id} | DELETE | administration | gap R-07 | - |  |
| api/identity/token | POST | public | - | - |  |
| api/identity/token/refresh | POST | public | - | - |  |
| api/identity/user | GET | administration | gap R-07 | - |  |
| api/identity/user | POST | public | - | - |  |
| api/identity/user/confirm-email | GET | public | - | - |  |
| api/identity/user/forgot-password | POST | public | - | - |  |
| api/identity/user/reset-password | POST | public | - | - |  |
| api/identity/user/roles/{id} | GET | administration | gap R-07 | - |  |
| api/identity/user/roles/{id} | PUT | administration | gap R-07 | - |  |
| api/identity/user/{id} | GET | user | gap R-07 | - |  |
