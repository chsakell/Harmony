using Harmony.Domain.Automation;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
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
        public static void Configure()
        {
            var pack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("camel case", pack, t => true);

            BsonClassMap.RegisterClassMap<AutomationTemplate>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });

            BsonClassMap.RegisterClassMap<Automation>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }
    }
}
