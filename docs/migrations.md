## Example migrations (add/remove/update)

When running migrations through Visual Studio, open the `Package Manager Console` and set the `Default project` to __src\Infrastructure\Harmony.Persistence__.

```
Add-Migration Initial -Context HarmonyContext -StartUpProject Harmony.Api -v
```

```
Add-Migration LoadBoardSp_VX -Context HarmonyContext -StartUpProject Harmony.Api -v
```

```
Add-Migration LoadBoardListCardsSp_VX -Context HarmonyContext -StartUpProject Harmony.Api -v
```

```
Remove-Migration PostCommentsInitial -Context HarmonyContext -StartUpProject Harmony.Api -v
```

```
Update-Database -Context HarmonyContext -StartUpProject Harmony.Api -v
```

```
dotnet ef database update --context HarmonyContext --startup-project "../../Web/Harmony.Api.csproj"
```

```
Add-Migration Initial -Context NotificationContext -StartUpProject Harmony.Notifications -v
```

```
Add-Migration Initial -Context NotificationContext -StartUpProject Harmony.Automations -v
```

```
Update-Database -Context NotificationContext -StartUpProject Harmony.Notifications -v
```

```
dotnet ef database update --context NotificationContext --startup-project "Harmony.Notifications.csproj"
```

## Notes
In case you decide for some reason to clean/remove the migrations folder, make sure to keep the following migrations which add two stored procedures.

1. `20231031152602_LoadBoardSp`
2. `20231104191204_LoadBoardListCardsSp`
