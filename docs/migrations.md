## Example migrations (add/remove/update)

When running migrations through Visual Studio, open the `Package Manager Console` and set the `Default project` to __src\Infrastructure\Harmony.Persistence__.

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