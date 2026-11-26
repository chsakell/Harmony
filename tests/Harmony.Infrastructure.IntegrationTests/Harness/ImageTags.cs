namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// The database images the tests run: the ones in docker-compose*.yml and .k8s/ (NFR-006, checked by
    /// ImageTagTests). The SQL Server tag floats (spec, Flagged concern 4).
    /// </summary>
    public static class ImageTags
    {
        public const string SqlServer = "mcr.microsoft.com/mssql/server:2022-latest";
        public const string PostgreSql = "postgres:17-alpine";
        public const string MongoDb = "mongo:8.3";
    }
}
