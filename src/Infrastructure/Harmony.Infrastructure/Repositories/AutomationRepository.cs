using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Automation;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Harmony.Infrastructure.Repositories
{
    public class AutomationRepository : IAutomationRepository
    {
        private readonly MongoClient _client;

        public AutomationRepository(IOptions<MongoDbConfiguration> mongoConfiguration)
        {
            _client = new MongoClient(mongoConfiguration.Value.ConnectionURI);
        }

        public async Task CreateAsync(IAutomationDto automation)
        {
            automation.Id = Guid.NewGuid().ToString();

            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var automationsCollection = database
                .GetCollection<IAutomationDto>(MongoDbConstants.AutomationsCollection);

            await automationsCollection.InsertOneAsync(automation);
        }

        public async Task<bool> ReplaceAsync(IAutomationDto automation)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var automationsCollection = database
                .GetCollection<IAutomationDto>(MongoDbConstants.AutomationsCollection);

            var filter = Builders<IAutomationDto>.Filter
                    .Eq(automation => automation.Id, automation.Id);

             var replaceResult = await automationsCollection.ReplaceOneAsync(filter, automation);

            return replaceResult.MatchedCount == 1 && replaceResult.ModifiedCount == 1;
        }

        public async Task<List<AutomationTemplate>> GetTemplates()
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var templatesCollection = database
                .GetCollection<AutomationTemplate>(MongoDbConstants.TemplatesCollection);

            var filter = Builders<AutomationTemplate>.Filter.Empty;

            var templates = await templatesCollection
                .Find(filter).ToListAsync();

            return templates;
        }

        public async Task<List<AutomationTemplate>> GetTemplates(NotificationType notificationType)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var templatesCollection = database
                .GetCollection<AutomationTemplate>(MongoDbConstants.TemplatesCollection);

            var filter = Builders<AutomationTemplate>.Filter
                .AnyEq(t => t.NotificationTypes, notificationType);

            var templates = await templatesCollection
                .Find(filter).ToListAsync();

            return templates;
        }

        public async Task<IEnumerable<T>> GetAutomations<T>(AutomationType type, Guid boardId)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var automationsCollection = database
                .GetCollection<BsonDocument>(MongoDbConstants.AutomationsCollection);

            var boardIdFilter = Builders<BsonDocument>.Filter
                .Eq("boardId", boardId.ToString());

            var typeFilter = Builders<BsonDocument>.Filter
                .Eq("type", type);

            var automations = await automationsCollection
                .Find(Builders<BsonDocument>.Filter.And(boardIdFilter, typeFilter)).ToListAsync();

            return automations
                .Select(automation => BsonSerializer.Deserialize<T>(automation));
        }

        public async Task<IEnumerable<IAutomationDto>> GetAutomations(List<AutomationType> types, Guid boardId)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var automationsCollection = database
                .GetCollection<BsonDocument>(MongoDbConstants.AutomationsCollection);

            var boardIdFilter = Builders<BsonDocument>.Filter
                .Eq("boardId", boardId.ToString());

            var typeFilter = Builders<BsonDocument>.Filter
                .AnyIn("type", types);

            var automations = await automationsCollection
                .Find(Builders<BsonDocument>.Filter.And(boardIdFilter, typeFilter)).ToListAsync();

            return automations.Select(auto =>
            {
                auto.TryGetValue("", out var type);

                return new SyncParentAndChildIssuesAutomationDto();
            });
        }

        public async Task CreateTemplate(AutomationTemplate template)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var templatesCollection = database
                .GetCollection<AutomationTemplate>(MongoDbConstants.TemplatesCollection);

            await templatesCollection.InsertOneAsync(template);
        }

        public async Task CreateTemplates(List<AutomationTemplate> templates)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var templatesCollection = database
                .GetCollection<AutomationTemplate>(MongoDbConstants.TemplatesCollection);

            await templatesCollection.InsertManyAsync(templates);
        }

        public async Task<bool> ChangeStatusAsync(string automationId, bool enabled)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var automationsCollection = database
                .GetCollection<IAutomationDto>(MongoDbConstants.AutomationsCollection);

            var filter = Builders<IAutomationDto>.Filter
                    .Eq(automation => automation.Id, automationId);

            var update = Builders<IAutomationDto>.Update.Set(automation => automation.Enabled, enabled);

            var updateResult = await automationsCollection.UpdateOneAsync(filter, update);

            return updateResult.MatchedCount == 1 && updateResult.ModifiedCount == 1;
        }

        public async Task<bool> Remove(string automationId)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var automationsCollection = database
                .GetCollection<IAutomationDto>(MongoDbConstants.AutomationsCollection);

            var filter = Builders<IAutomationDto>.Filter
                    .Eq(automation => automation.Id, automationId);

            var deleteResult = await automationsCollection.DeleteOneAsync(filter);

            return deleteResult.DeletedCount == 1;
        }
    }
}
