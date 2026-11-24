// .NET Aspire AppHost for Harmony.
//
//   aspire run            (or: dotnet run --project src/Aspire/Harmony.AppHost, or F5 with Harmony.AppHost as startup project)
//
// Starts the infrastructure as containers (SQL Server or PostgreSQL, MongoDB, RabbitMQ, Redis, optionally Elasticsearch
// + Kibana) and every Harmony service, injects their configuration as environment variables in the exact shape the
// services already read (ConnectionStrings__*, BrokerConfiguration__*, MongoDB__ConnectionURI, ...), and collects
// traces, metrics and logs in the Aspire dashboard.
//
// Data survives restarts: containers are Persistent and use named Docker volumes (harmony-*-data). Passwords are
// generated once and stored in this project's user secrets (Parameters:*) - keep them, the volumes are initialized with them.
//
// Configuration (appsettings.json, user secrets, or environment variables like Harmony__DatabaseProvider):
//   Harmony:DatabaseProvider    SqlServer (default) | PostgreSQL | LocalSqlServer
//   Harmony:EnableElasticsearch false (default) | true

using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var databaseProvider = builder.Configuration["Harmony:DatabaseProvider"] ?? "SqlServer";
var enableElasticsearch = builder.Configuration.GetValue("Harmony:EnableElasticsearch", false);

// ------------------------------------------------------------------------------------------------------------------
// Relational databases: Harmony (Api), Harmony.Automations.Jobs and Harmony.Notifications.Jobs (Hangfire + EF contexts)
// ------------------------------------------------------------------------------------------------------------------
IResourceBuilder<IResourceWithConnectionString> harmonyDb;
IResourceBuilder<IResourceWithConnectionString> automationsJobsDb;
IResourceBuilder<IResourceWithConnectionString> notificationsJobsDb;
string servicesDatabaseProvider;

switch (databaseProvider.Trim().ToLowerInvariant())
{
    case "postgresql":
        {
            var postgres = builder.AddPostgres("postgres", password: builder.AddGeneratedSecret("postgres-password"), port: 54333)
                .WithDataVolume("harmony-postgres-data")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithPgAdmin(pgAdmin => pgAdmin.WithLifetime(ContainerLifetime.Persistent));

            harmonyDb = postgres.AddDatabase("harmony", "Harmony");
            automationsJobsDb = postgres.AddDatabase("automations-jobs", "Harmony.Automations.Jobs");
            notificationsJobsDb = postgres.AddDatabase("notifications-jobs", "Harmony.Notifications.Jobs");
            servicesDatabaseProvider = "PostgreSQL";
            break;
        }
    case "localsqlserver":
        {
            // Your own SQL Server instance - connection strings come from ConnectionStrings:* in appsettings / user secrets
            harmonyDb = builder.AddConnectionString("harmony-local");
            automationsJobsDb = builder.AddConnectionString("automations-jobs-local");
            notificationsJobsDb = builder.AddConnectionString("notifications-jobs-local");
            servicesDatabaseProvider = "SqlServer";
            break;
        }
    case "sqlserver":
        {
            var sql = builder.AddSqlServer("sql", password: builder.AddGeneratedSecret("sql-password"), port: 14333)
                .WithDataVolume("harmony-sqlserver-data")
                .WithLifetime(ContainerLifetime.Persistent);

            harmonyDb = sql.AddDatabase("harmony", "Harmony");
            automationsJobsDb = sql.AddDatabase("automations-jobs", "Harmony.Automations.Jobs");
            notificationsJobsDb = sql.AddDatabase("notifications-jobs", "Harmony.Notifications.Jobs");
            servicesDatabaseProvider = "SqlServer";
            break;
        }
    default:
        throw new InvalidOperationException(
            $"Unknown Harmony:DatabaseProvider '{databaseProvider}'. Use SqlServer, PostgreSQL or LocalSqlServer.");
}

// ------------------------------------------------------------------------------------------------------------------
// MongoDB (automations & source control), RabbitMQ (integration events), Redis (cache + SignalR backplane)
// ------------------------------------------------------------------------------------------------------------------
var mongodb = builder.AddMongoDB("mongodb", port: 27018, password: builder.AddGeneratedSecret("mongodb-password"))
    .WithDataVolume("harmony-mongodb-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithMongoExpress(mongoExpress => mongoExpress.WithLifetime(ContainerLifetime.Persistent));

var rabbitmqUser = builder.AddParameter("rabbitmq-username", "harmony");
var rabbitmqPassword = builder.AddGeneratedSecret("rabbitmq-password");
var rabbitmq = builder.AddRabbitMQ("rabbitmq", rabbitmqUser, rabbitmqPassword, port: 5673)
    .WithDataVolume("harmony-rabbitmq-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin(port: 15673);

var redis = builder.AddRedis("redis", port: 6380, password: builder.AddGeneratedSecret("redis-password"))
    .WithDataVolume("harmony-redis-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight(insight => insight.WithLifetime(ContainerLifetime.Persistent));

// ------------------------------------------------------------------------------------------------------------------
// Optional: Elasticsearch + Kibana for the Serilog Elasticsearch sink (logs are always in the Aspire dashboard anyway)
// ------------------------------------------------------------------------------------------------------------------
EndpointReference? elasticsearchEndpoint = null;

if (enableElasticsearch)
{
    var elasticsearch = builder.AddContainer("elasticsearch", "docker.elastic.co/elasticsearch/elasticsearch", "7.17.18")
        .WithEnvironment("discovery.type", "single-node")
        .WithEnvironment("xpack.security.enabled", "false")
        .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
        .WithVolume("harmony-elasticsearch-data", "/usr/share/elasticsearch/data")
        .WithHttpEndpoint(port: 9200, targetPort: 9200, name: "http")
        .WithHttpHealthCheck("/_cluster/health", endpointName: "http")
        .WithLifetime(ContainerLifetime.Persistent);

    elasticsearchEndpoint = elasticsearch.GetEndpoint("http");

    builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana", "7.17.18")
        .WithEnvironment("ELASTICSEARCH_HOSTS", elasticsearchEndpoint)
        .WithHttpEndpoint(port: 5601, targetPort: 5601, name: "http")
        .WithLifetime(ContainerLifetime.Persistent)
        .WaitFor(elasticsearch);
}

// ------------------------------------------------------------------------------------------------------------------
// Harmony services. Each keeps its launchSettings "https" port (7097, 7277, ...), so ocelot.json and the Blazor client's
// gatewayUrl keep working unchanged; service-to-service URLs are injected from the endpoints below.
// ------------------------------------------------------------------------------------------------------------------
var api = builder.AddProject<Projects.Harmony_Api>("api", launchProfileName: "https")
    .WithHarmonyDefaults(elasticsearchEndpoint)
    .WithReference(harmonyDb, "HarmonyConnection").WaitFor(harmonyDb)
    .WithEnvironment("DatabaseProvider", servicesDatabaseProvider)
    .WithMessageBroker(rabbitmq, rabbitmqUser, rabbitmqPassword)
    .WithRedisCache(redis);

var sourceControl = builder.AddProject<Projects.Harmony_Integrations_SourceControl>("sourcecontrol", launchProfileName: "https")
    .WithHarmonyDefaults(elasticsearchEndpoint)
    .WithMongoDb(mongodb)
    .WithMessageBroker(rabbitmq, rabbitmqUser, rabbitmqPassword);

var automations = builder.AddProject<Projects.Harmony_Automations>("automations", launchProfileName: "https")
    .WithHarmonyDefaults(elasticsearchEndpoint)
    .WithReference(automationsJobsDb, "HarmonyJobsConnection").WaitFor(automationsJobsDb)
    .WithEnvironment("DatabaseProvider", servicesDatabaseProvider)
    .WithMongoDb(mongodb)
    .WithMessageBroker(rabbitmq, rabbitmqUser, rabbitmqPassword)
    .WithRedisCache(redis)
    .WithEnvironment("AppEndpointConfiguration__HarmonyApiEndpoint", api.GetEndpoint("https"))
    .WaitFor(api);

var notifications = builder.AddProject<Projects.Harmony_Notifications>("notifications", launchProfileName: "https")
    .WithHarmonyDefaults(elasticsearchEndpoint)
    .WithReference(notificationsJobsDb, "HarmonyJobsConnection").WaitFor(notificationsJobsDb)
    .WithEnvironment("DatabaseProvider", servicesDatabaseProvider)
    .WithMessageBroker(rabbitmq, rabbitmqUser, rabbitmqPassword)
    .WithEnvironment("AppEndpointConfiguration__HarmonyApiEndpoint", api.GetEndpoint("https"))
    .WaitFor(api);

var signalr = builder.AddProject<Projects.Harmony_SignalR>("signalr", launchProfileName: "https")
    .WithHarmonyDefaults(elasticsearchEndpoint)
    .WithMessageBroker(rabbitmq, rabbitmqUser, rabbitmqPassword)
    // A Redis connection string turns on the SignalR Redis backplane
    .WithEnvironment("RedisConnectionString", redis.Resource.ConnectionStringExpression)
    .WaitFor(redis);

var gateway = builder.AddProject<Projects.Harmony_ApiGateway>("gateway", launchProfileName: "https")
    .WithHttpHealthCheck("/healthz/ready", endpointName: "https")
    .WaitFor(api)
    .WaitFor(automations)
    .WaitFor(sourceControl)
    .WaitFor(signalr);

var client = builder.AddProject<Projects.Harmony_Client>("client", launchProfileName: "https")
    .WithExternalHttpEndpoints()
    .WaitFor(gateway);

// Endpoints the Api needs from services declared after it
api.WithEnvironment("AppEndpointConfiguration__AutomationEndpoint", automations.GetEndpoint("https"))
    .WithEnvironment("AppEndpointConfiguration__SourceControlEndpoint", sourceControl.GetEndpoint("https"))
    .WithEnvironment("AppEndpointConfiguration__FrontendUrl", client.GetEndpoint("https"));

notifications.WithEnvironment("AppEndpointConfiguration__FrontendUrl", client.GetEndpoint("https"));

builder.Build().Run();

internal static class HarmonyResourceBuilderExtensions
{
    /// <summary>
    /// A secret generated on first run and persisted in the AppHost user secrets (Parameters:{name}).
    /// No special characters, so the value is safe inside URIs and connection strings.
    /// </summary>
    public static IResourceBuilder<ParameterResource> AddGeneratedSecret(this IDistributedApplicationBuilder builder, string name) =>
        builder.AddParameter(name, new GenerateParameterDefault { MinLength = 24, Special = false }, secret: true, persist: true);

    /// <summary>Health check, OTLP service name and Elasticsearch sink settings shared by all Harmony services.</summary>
    public static IResourceBuilder<ProjectResource> WithHarmonyDefaults(
        this IResourceBuilder<ProjectResource> project, EndpointReference? elasticsearchEndpoint)
    {
        project.WithHttpHealthCheck("/healthz/ready", endpointName: "https");

        // An empty value disables the Serilog Elasticsearch sink (see Harmony.Logging.SeriLogger)
        return elasticsearchEndpoint is null
            ? project.WithEnvironment("ElasticConfiguration__Uri", string.Empty)
            : project.WithEnvironment("ElasticConfiguration__Uri", elasticsearchEndpoint);
    }

    /// <summary>Maps the RabbitMQ resource to the BrokerConfiguration section the services bind.</summary>
    public static IResourceBuilder<ProjectResource> WithMessageBroker(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<RabbitMQServerResource> rabbitmq,
        IResourceBuilder<ParameterResource> userName,
        IResourceBuilder<ParameterResource> password)
    {
        var endpoint = rabbitmq.Resource.PrimaryEndpoint;

        return project
            .WithEnvironment("BrokerConfiguration__Host", endpoint.Property(EndpointProperty.Host))
            .WithEnvironment("BrokerConfiguration__Port", endpoint.Property(EndpointProperty.Port))
            .WithEnvironment("BrokerConfiguration__Username", userName)
            .WithEnvironment("BrokerConfiguration__Password", password)
            .WithEnvironment("BrokerConfiguration__VirtualHost", "/")
            .WaitFor(rabbitmq);
    }

    /// <summary>Maps the MongoDB resource to MongoDB:ConnectionURI.</summary>
    public static IResourceBuilder<ProjectResource> WithMongoDb(
        this IResourceBuilder<ProjectResource> project, IResourceBuilder<MongoDBServerResource> mongodb) =>
        project
            .WithEnvironment("MongoDB__ConnectionURI", mongodb.Resource.ConnectionStringExpression)
            .WaitFor(mongodb);

    /// <summary>Switches the EasyCaching provider to Redis.</summary>
    public static IResourceBuilder<ProjectResource> WithRedisCache(
        this IResourceBuilder<ProjectResource> project, IResourceBuilder<RedisResource> redis) =>
        project
            .WithEnvironment("CacheProvider", "Redis")
            .WithEnvironment("RedisConnectionString", redis.Resource.ConnectionStringExpression)
            .WaitFor(redis);
}
