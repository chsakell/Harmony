using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Domain.Automation;
using Harmony.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bogus.DataSets.Name;
using static Harmony.Shared.Constants.Permission.Permissions;

namespace Harmony.Infrastructure.Repositories
{
    public class AutomationRepository : IAutomationRepository
    {
        private readonly MongoClient _client;

        public AutomationRepository(IOptions<MongoDbConfiguration> mongoConfiguration)
        {
            _client = new MongoClient(mongoConfiguration.Value.ConnectionURI);
        }

        public Task<int> CreateAsync(CreateAutomationCommand automation)
        {
            throw new NotImplementedException();
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

        public async Task<IEnumerable<T>> GetAutomations<T>(AutomationType type, Guid boardId)
        {
            var database = _client
                .GetDatabase(MongoDbConstants.AutomationsDatabase);

            var automationsCollection = database
                .GetCollection<BsonDocument>(MongoDbConstants.AutomationsCollection);

            var boardIdFilter = Builders<BsonDocument>.Filter
                .Eq("boardId", boardId);

            var typeFilter = Builders<BsonDocument>.Filter
                .Eq("type", type);

            var automations = await automationsCollection
                .Find(Builders<BsonDocument>.Filter.And(boardIdFilter, typeFilter)).ToListAsync();

            return automations
                .Select(automation => BsonSerializer.Deserialize<T>(automation));
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
    }
}
