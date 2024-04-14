using Algolia.Search.Models.Analytics;
using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Enums;
using Harmony.Domain.SourceControl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace Harmony.Infrastructure.Repositories
{
    public class SourceControlRepository : ISourceControlRepository
    {
        private readonly MongoClient _client;

        public SourceControlRepository(IOptions<MongoDbConfiguration> mongoConfiguration,
            ILogger<SourceControlRepository> logger)
        {
            var mongoConnectionUrl = new MongoUrl(mongoConfiguration.Value.ConnectionURI);
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);
            mongoClientSettings.ClusterConfigurator = cb => {
                cb.Subscribe<CommandStartedEvent>(e => {
                    logger.LogInformation($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };

            _client = new MongoClient(mongoClientSettings);
        }

        public async Task CreateBranch(Branch branch)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var branchesCollection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            await branchesCollection.InsertOneAsync(branch);
        }

        public async Task<bool> DeleteBranch(string name)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var branchesCollection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var filter = Builders<Branch>.Filter
                    .Eq(branch => branch.Name, name);

            var deleteResult = await branchesCollection.DeleteOneAsync(filter);

            return deleteResult.DeletedCount > 0;
        }
    }
}
