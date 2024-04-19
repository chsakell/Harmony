using Algolia.Search.Models.Analytics;
using Bogus.DataSets;
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
using static Harmony.Shared.Constants.Permission.Permissions;

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

        public async Task CreateRepository(Repository repo)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var repositoriesCollection = database
                .GetCollection<Repository>(MongoDbConstants.RepositoriesCollection);

            await repositoriesCollection.InsertOneAsync(repo);
        }

        public async Task<Repository> GetRepository(string repositoryId)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var repositoriesCollection = database
                .GetCollection<Repository>(MongoDbConstants.RepositoriesCollection);

            var filter = Builders<Repository>.Filter
                    .Eq(repo => repo.Id, repositoryId);

            return await repositoriesCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateBranch(Branch branch)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var branchesCollection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            await branchesCollection.InsertOneAsync(branch);
        }

        public async Task<Branch> GetBranch(string name, string repositoryId)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var branchesCollection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var repoFilter = Builders<Branch>.Filter
                .Eq(repo => repo.RepositoryId, repositoryId);

            var nameFilter = Builders<Branch>.Filter
                    .Eq(branch => branch.Name, name);

            var filter = Builders<Branch>.Filter
                .And(repoFilter, nameFilter);


            return await branchesCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Branch>> SearchBranches(string term)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var branchesCollection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var filter = Builders<Branch>.Filter
                .Regex(branch => branch.Name,
                new BsonRegularExpression($"/{term}/i"));

            return await branchesCollection.Find(filter).ToListAsync();
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

        public async Task CreatePush(Push push)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var commitsCollection = database
                .GetCollection<Push>(MongoDbConstants.CommitsCollection);

            await commitsCollection.InsertOneAsync(push);
        }

        public async Task<List<Repository>> GetRepositories(List<string> repositories)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var repositoriesCollection = database
                .GetCollection<Repository>(MongoDbConstants.RepositoriesCollection);

            var filter = Builders<Repository>.Filter
                    .In(repo => repo.RepositoryId, repositories);

            return await repositoriesCollection.Find(filter).ToListAsync();
        }
    }
}
