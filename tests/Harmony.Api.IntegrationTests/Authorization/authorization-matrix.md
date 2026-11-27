# Authorization matrix

Which `Harmony.Api` endpoints need a login, and which ones a user who is not a member of a workspace or board can still use. The Api tests check every line against the running Api (`dotnet test --project tests/Harmony.Api.IntegrationTests -p:HarmonyIntegrationTests=true`, see `AGENTS.md`). The rules and the format are in `docs/features/22-api-authorization-matrix/` (spec and plan).

- **Scope** says what the endpoint acts on:
  - `public`: open without a login, on purpose;
  - `self`: the caller's own data;
  - `resource`: a workspace, or a board and anything in it;
  - `administration`: roles and user administration;
  - `user`: another user's account or settings.
- **Anonymous** is the result with no login. `refused` means 401 is expected: every endpoint needs a login unless it is one of the six public endpoints, which have `-`. A known gap would be marked `gap R-07`; R-07 closed the last one, and an `administration` endpoint may never have one.
- **Non-member** is the result for a logged-in user who is not a member. It applies to `resource` endpoints only. `refused` means 403 or 404 is expected, and `gap R-08` means it is open today and R-08 closes it.
- **Non-admin** is the result for a logged-in user who does not hold the endpoint's required permission. It applies to `administration` endpoints only, where it is always `refused`: 403 is expected, and there is no known gap. Other endpoints have `-`. The rules are in `docs/features/25-authenticated-by-default-api/`.
- **Bindings** tell the tests which resource an ID names, when the name alone does not say (`name=kind`, and optionally a fixed `body=<json>` last). An `administration` endpoint names its required permission with `permission=<value>`, for example `permission=Permissions.Roles.View`; it is not sent with the request.

A new endpoint fails the tests until it has a line here. Keep the lines sorted by Route, then Method (ordinal).

## Baselines

<!--
  WARNING: each baseline must equal the number of known-gap marks below, and the tests fail when they differ.
  Lowering a baseline closes a gap. RAISING ONE ADDS A KNOWN GAP: an endpoint that ships open. Review such a
  change as a security decision (spec, Flagged concern 9).
-->

- Anonymous known gaps: 0
- Non-member known gaps: 80

## Endpoints

| Route | Method | Scope | Anonymous | Non-member | Non-admin | Bindings |
|---|---|---|---|---|---|---|
| api/Board | POST | resource | refused | gap R-08 | - | body={"Title":"Matrix","Description":"Matrix","WorkspaceId":"","Visibility":0,"BoardType":0,"Key":"MTRX"} |
| api/Board/userboards | GET | self | refused | - | - |  |
| api/Board/{boardId:guid}/work-items | GET | resource | refused | gap R-08 | - |  |
| api/Board/{id:guid}/archived-items | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/backlog | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/boardlists | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/info | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/issuetypes | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/lists/{listId:guid} | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/load | POST | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/members | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/members | POST | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/members/search | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/members/{userId} | DELETE | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/members/{userId}/status | PUT | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/movecardstobacklog | PUT | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/movecardstosprint | PUT | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/positions | PUT | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/reactivatecards | PUT | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/sprints | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/sprints | POST | resource | refused | gap R-08 | - | id=board; body={"BoardId":"","SprintId":null,"Name":"Matrix","Goal":"Matrix","StartDate":"2026-01-01T00:00:00","EndDate":"2026-01-15T00:00:00"} |
| api/Board/{id:guid}/sprints/details | GET | resource | refused | gap R-08 | - | id=board |
| api/Board/{id:guid}/sprints/{sprintId:guid}/cards/pending | GET | resource | refused | gap R-08 | - | id=board |
| api/BoardLists/{id:guid}/title | PUT | resource | refused | gap R-08 | - | id=list |
| api/Cards | POST | resource | refused | gap R-08 | - | IssueType.Id=issuetype |
| api/Cards/backlog | POST | resource | refused | gap R-08 | - | IssueType.Id=issuetype |
| api/Cards/backlog/{id:guid} | PUT | resource | refused | gap R-08 | - | id=card; IssueType.Id=issuetype |
| api/Cards/search | GET | resource | refused | gap R-08 | - | skipCardId=card2 |
| api/Cards/{id:guid} | GET | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/activity | GET | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/attachments/{attachmentId:guid} | DELETE | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/childissue | POST | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/dates | PUT | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/description | PUT | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/issuetype | PUT | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/labels | GET | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/labels | PUT | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/links | POST | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/members | GET | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/members | POST | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/members/{userId} | DELETE | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/move | PUT | resource | refused | gap R-08 | - | id=card; UpdateId=new |
| api/Cards/{id:guid}/status | PUT | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/storypoints | PUT | resource | refused | gap R-08 | - | id=card |
| api/Cards/{id:guid}/title | PUT | resource | refused | gap R-08 | - | id=card |
| api/CheckListItems/{id:guid}/checked | PUT | resource | refused | gap R-08 | - | id=checklistitem |
| api/CheckListItems/{id:guid}/description | PUT | resource | refused | gap R-08 | - | id=checklistitem |
| api/CheckListItems/{id:guid}/duedate | PUT | resource | refused | gap R-08 | - | id=checklistitem |
| api/CheckLists | POST | resource | refused | gap R-08 | - |  |
| api/CheckLists/{id:guid} | DELETE | resource | refused | gap R-08 | - | id=checklist |
| api/CheckLists/{id:guid}/items | POST | resource | refused | gap R-08 | - | id=checklist |
| api/CheckLists/{id:guid}/title | PUT | resource | refused | gap R-08 | - | id=checklist; ListId=checklist |
| api/Comments | POST | resource | refused | gap R-08 | - |  |
| api/Comments/{cardId:guid} | GET | resource | refused | gap R-08 | - |  |
| api/Comments/{commentId:guid} | POST | resource | refused | gap R-08 | - |  |
| api/Comments/{commentId:guid} | PUT | resource | refused | gap R-08 | - |  |
| api/Files | POST | resource | refused | gap R-08 | - |  |
| api/Files/profile-picture | POST | self | refused | - | - |  |
| api/Labels | POST | resource | refused | gap R-08 | - |  |
| api/Labels/{id:guid} | DELETE | resource | refused | gap R-08 | - | id=label |
| api/Labels/{id:guid}/title | PUT | resource | refused | gap R-08 | - | id=label |
| api/Links/{id:guid} | DELETE | resource | refused | gap R-08 | - | id=link |
| api/Lists | POST | resource | refused | gap R-08 | - |  |
| api/Lists/{id:guid}/status | PUT | resource | refused | gap R-08 | - | id=list |
| api/Retrospectives | POST | resource | refused | gap R-08 | - |  |
| api/Search | GET | self | refused | - | - |  |
| api/Search/advanced | GET | self | refused | - | - |  |
| api/Search/advanced | POST | resource | refused | gap R-08 | - |  |
| api/Sprints/{id:guid} | GET | resource | refused | gap R-08 | - | id=sprint |
| api/Sprints/{id:guid}/cards | GET | resource | refused | gap R-08 | - | id=sprint |
| api/Sprints/{id:guid}/cards | POST | resource | refused | gap R-08 | - | id=sprint; IssueType.Id=issuetype |
| api/Sprints/{id:guid}/complete | POST | resource | refused | gap R-08 | - | id=sprint |
| api/Sprints/{id:guid}/reports | GET | resource | refused | gap R-08 | - | id=sprint |
| api/Sprints/{id:guid}/start | PUT | resource | refused | gap R-08 | - | id=sprint |
| api/UserNotifications | GET | self | refused | - | - |  |
| api/UserNotifications | POST | self | refused | - | - |  |
| api/Workspace | GET | self | refused | - | - |  |
| api/Workspace | POST | resource | refused | gap R-08 | - |  |
| api/Workspace/{id:guid} | GET | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/boards | GET | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/info | GET | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/members | GET | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/members/add | POST | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/members/remove | POST | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/members/search | GET | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/rename | PUT | resource | refused | gap R-08 | - | id=workspace |
| api/Workspace/{id:guid}/status | PUT | resource | refused | gap R-08 | - | id=workspace |
| api/identity/account | GET | self | refused | - | - |  |
| api/identity/account/ChangePassword | PUT | self | refused | - | - |  |
| api/identity/account/UpdateProfile | PUT | self | refused | - | - |  |
| api/identity/account/profile-picture/{userId} | POST | self | refused | - | - |  |
| api/identity/role | GET | administration | refused | - | refused | permission=Permissions.Roles.View |
| api/identity/role | POST | administration | refused | - | refused | permission=Permissions.Roles.Create |
| api/identity/role/permissions/update | PUT | administration | refused | - | refused | permission=Permissions.RoleClaims.Edit |
| api/identity/role/permissions/{roleId} | GET | administration | refused | - | refused | permission=Permissions.RoleClaims.View |
| api/identity/role/{id} | DELETE | administration | refused | - | refused | permission=Permissions.Roles.Delete |
| api/identity/token | POST | public | - | - | - |  |
| api/identity/token/refresh | POST | public | - | - | - |  |
| api/identity/user | GET | administration | refused | - | refused | permission=Permissions.Users.View |
| api/identity/user | POST | public | - | - | - |  |
| api/identity/user/confirm-email | GET | public | - | - | - |  |
| api/identity/user/forgot-password | POST | public | - | - | - |  |
| api/identity/user/reset-password | POST | public | - | - | - |  |
| api/identity/user/roles/{id} | GET | administration | refused | - | refused | permission=Permissions.Users.View |
| api/identity/user/roles/{id} | PUT | administration | refused | - | refused | permission=Permissions.Users.Edit |
