using Algolia.Search.Models.Analytics;
using Bogus.DataSets;
using Google.Protobuf.WellKnownTypes;
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
            mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(10);
            mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
            mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
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
                    .Eq(repo => repo.RepositoryId, repositoryId);

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

        public async Task<bool> BranchExists(string name, string repositoryId)
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

            return await branchesCollection.Find(filter).CountDocumentsAsync() > 0;
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

        public async Task<long> GetTotalBranches(string term)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var branchesCollection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var filter = Builders<Branch>.Filter
                .Regex(branch => branch.Name,
                new BsonRegularExpression($"/{term}/i"));

            return await branchesCollection.Find(filter).CountAsync();
        }

        //public async Task<long> GetTotalPushes(string term)
        //{
        //    var database = _client
        //        .GetDatabase(MongoDbConstants.SourceControlDatabase);

        //    var collection = database
        //        .GetCollection<Push>(MongoDbConstants.CommitsCollection);

        //    var filter = Builders<Push>.Filter
        //        .Regex(branch => branch.Ref,
        //        new BsonRegularExpression($"/{term}/i"));

        //    return await collection.Find(filter).CountAsync();
        //}

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

        public async Task CreatePush(string repositoryId, string branch, List<Commit> commits)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var collection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var updateDefinition = Builders<Branch>.Update
                .PushEach(b => b.Commits, commits);

            await collection.UpdateOneAsync(
                b => b.Name == branch &&
                     b.RepositoryId == repositoryId, updateDefinition);
        }

        public async Task<Branch> FindBranchByCommit(string repositoryId, string commitId)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var collection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var filter = Builders<Branch>.Filter
                .ElemMatch(b => b.Commits, commit => commit.Id == commitId);

            var branch = await collection
                .Find(filter).FirstOrDefaultAsync();

            return branch;
        }

        public async Task AddTagToBranch(string branchId, string repositoryId, string tag)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var collection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var updateDefinition = Builders<Branch>.Update
                .Push(b => b.Tags, tag);

            await collection.UpdateOneAsync(
                b => b.Id == branchId &&
                     b.RepositoryId == repositoryId, updateDefinition);
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

        public async Task AddOrUpdatePullRequest(string repositoryId, PullRequest pullRequest)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.SourceControlDatabase);

            var collection = database
                .GetCollection<Branch>(MongoDbConstants.BranchesCollection);

            var repoFilter = Builders<Branch>.Filter
                .Eq(repo => repo.RepositoryId, repositoryId);

            var branchFilter = Builders<Branch>.Filter
                    .Eq(branch => branch.Name, pullRequest.SourceBranch);

            var filter = Builders<Branch>.Filter
                .ElemMatch(b => b.PullRequests,
                    pullRequest => pullRequest.Id == pullRequest.Id);

            var branch = await collection.Find(Builders<Branch>.Filter
                .And(repoFilter, branchFilter)).FirstOrDefaultAsync();

            if (branch == null)
            {
                return;
            }

            var pullRequestsExists = branch.PullRequests.Any(p => p.Id == pullRequest.Id);

            if (pullRequestsExists)
            {
                var compinedFilter = Builders<Branch>
                        .Filter.Eq(b => b.Name, pullRequest.SourceBranch)
                    & Builders<Branch>.Filter
                        .ElemMatch(b => b.PullRequests,
                            Builders<PullRequest>.Filter.Eq(p => p.Id, pullRequest.Id));

                var pullDefinition = Builders<Branch>
                    .Update.PullFilter(b => b.PullRequests,
                                p => p.Id == pullRequest.Id);

                var removeResult = await collection
                            .UpdateManyAsync(compinedFilter, pullDefinition);
            }


            var pushDefinition = Builders<Branch>.Update
                    .PushEach(p => p.PullRequests, new List<PullRequest> { pullRequest });

            await collection.UpdateOneAsync(
                b => b.Name == branch.Name &&
                     b.RepositoryId == repositoryId, pushDefinition);
        }

        public async Task<bool> Ping(string database)
        {
            var db = _client
                .GetDatabase(database);

            var result = await db.RunCommandAsync((Command<BsonDocument>)"{ping:1}");

            return result != null;
        }
    }
}
