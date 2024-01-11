using Harmony.Domain.Automation;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Persistence.Configurations.MongoDb
{
    public class MongoDbConfiguration
    {
        public static void RegisterEntities()
        {
            BsonClassMap.RegisterClassMap<AutomationTemplate>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }
    }
}
