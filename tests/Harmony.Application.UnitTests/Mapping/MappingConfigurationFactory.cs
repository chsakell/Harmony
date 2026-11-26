using AutoMapper;
using Harmony.Application.Mappings.Management;

namespace Harmony.Application.UnitTests.Mapping
{
    /// <summary>
    /// The AutoMapper configuration of the Application layer, built once from the profiles in the
    /// <c>Harmony.Application</c> assembly, the same way <c>AddApplicationLayer</c> registers them.
    /// </summary>
    public static class MappingConfigurationFactory
    {
        private static readonly Lazy<MapperConfiguration> LazyConfiguration =
            new(() => new MapperConfiguration(config => config.AddMaps(typeof(WorkspaceProfile).Assembly)));

        public static MapperConfiguration Configuration => LazyConfiguration.Value;

        public static IMapper CreateMapper() => Configuration.CreateMapper();
    }
}
