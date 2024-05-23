## Example migrations

## dotnet ef

- First you need to have installed [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet). Run the following command to install them:
```
dotnet tool install --global dotnet-ef
```
- Open a terminal and navigate to the **Harmony.Persistence** root folder
- Select a database provider and configure the connection strings in the appsettings.json files for Harmony.Api, Harmony.Automations & Harmony.Notification projects.

**Examples**:

```
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "HarmonyConnection": "Host=localhost;Database=Harmony;Username=postgres;Password=MySuperSecretPassword"
  },
```
```
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "HarmonyConnection": "Server=.;Database=Harmony;Integrated Security=True;TrustServerCertificate=True"
  },
```
- Based on your database provider, apply the alrealdy existing migrations

### SQL Server

### Apply migrations

```
dotnet ef database update --context HarmonyContext --startup-project "../../Services/Harmony.Api/Harmony.Api.csproj" -- --DatabaseProvider SqlServer
```

```
dotnet ef database update --context NotificationContext --startup-project "../../Services/Harmony.Notifications/Harmony.Notifications.csproj" -- --DatabaseProvider SqlServer
```

```
dotnet ef database update --context AutomationContext --startup-project "../../Services/Harmony.Automations/Harmony.Automations.csproj" -- --DatabaseProvider SqlServer
```

### Adding migrations (required only for new development)

```
dotnet ef migrations add Initial -o HarmonyContextMigrations --context HarmonyContext --project ../Harmony.Persistence.Migrations.SqlServer --startup-project "../../Services/Harmony.Api/Harmony.Api.csproj" -- --DatabaseProvider SqlServer
```

```
Add-Migration MyCustomMigrationName -OutputDir HarmonyContextMigrations -Context HarmonyContext -Project Harmony.Persistence.Migrations.SqlServer -StartUpProject Harmony.Api
```

```
dotnet ef migrations add Initial -o AutomationContextMigrations --context AutomationContext --project ../Harmony.Persistence.Migrations.SqlServer --startup-project "../../Services/Harmony.Automations/Harmony.Automations.csproj" -- --DatabaseProvider SqlServer
```

```
Add-Migration MyCustomMigrationName -OutputDir AutomationContextMigrations -Context AutomationContext -Project Harmony.Persistence.Migrations.SqlServer -StartUpProject Harmony.Automations
```

```
dotnet ef migrations add Initial -o NotificationContextMigrations --context NotificationContext --project ../Harmony.Persistence.Migrations.SqlServer --startup-project "../../Services/Harmony.Notifications/Harmony.Notifications.csproj" -- --DatabaseProvider SqlServer
```

```
Add-Migration MyCustomMigrationName -OutputDir NotificationContextMigrations -Context NotificationContext -Project Harmony.Persistence.Migrations.SqlServer -StartUpProject Harmony.Notifications
```

### PostgreSQL

### Apply migrations

```
dotnet ef database update --context HarmonyContext --startup-project "../../Services/Harmony.Api/Harmony.Api.csproj" -- --DatabaseProvider PostgreSQL
```

```
dotnet ef database update --context NotificationContext --startup-project "../../Services/Harmony.Notifications/Harmony.Notifications.csproj" -- --DatabaseProvider PostgreSQL
```

```
dotnet ef database update --context AutomationContext --startup-project "../../Services/Harmony.Automations/Harmony.Automations.csproj" -- --DatabaseProvider PostgreSQL
```

### Adding migrations (required only for new development)

```
dotnet ef migrations add Initial -o HarmonyContextMigrations --context HarmonyContext --project ../Harmony.Persistence.Migrations.PostgreSql --startup-project "../../Services/Harmony.Api/Harmony.Api.csproj" -- --DatabaseProvider PostgreSQL
```

```
Add-Migration MyCustomMigrationName -OutputDir HarmonyContextMigrations -Context HarmonyContext -Project Harmony.Persistence.Migrations.PostgreSql -StartUpProject Harmony.Api
```

```
dotnet ef migrations add Initial -o AutomationContextMigrations --context AutomationContext --project ../Harmony.Persistence.Migrations.PostgreSql --startup-project "../../Services/Harmony.Automations/Harmony.Automations.csproj" -- --DatabaseProvider PostgreSQL
```

```
Add-Migration MyCustomMigrationName -OutputDir AutomationContextMigrations -Context AutomationContext -Project Harmony.Persistence.Migrations.PostgreSql -StartUpProject Harmony.Automations
```

```
dotnet ef migrations add Initial -o NotificationContextMigrations --context NotificationContext --project ../Harmony.Persistence.Migrations.PostgreSql --startup-project "../../Services/Harmony.Notifications/Harmony.Notifications.csproj" -- --DatabaseProvider PostgreSQL
```

```
Add-Migration MyCustomMigrationName -OutputDir NotificationContextMigrations -Context NotificationContext -Project Harmony.Persistence.Migrations.PostgreSql -StartUpProject Harmony.Notifications
```