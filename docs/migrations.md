## Example migrations (add/remove/update)

```
Add-Migration Initial -Context HarmonyContext -StartUpProject Harmony.Server -v
```

```
Remove-Migration PostCommentsInitial -Context HarmonyContext -StartUpProject Harmony.Server -v
```

```
Update-Database -Context HarmonyContext -StartUpProject Harmony.Server -v
```

```
dotnet ef database update --context HarmonyContext --startup-project "../../Web/Harmony/Server/Harmony.Server.csproj"
```