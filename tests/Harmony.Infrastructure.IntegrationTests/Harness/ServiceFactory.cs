using AutoMapper;
using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Services.Caching;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Services.Management;
using Harmony.Persistence.DbContext;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// The real repositories and services over one new <see cref="HarmonyContext"/>, like one request scope in the
    /// Api. Their non-database dependencies get stand-ins: a pass-through cache (and the real card summary service
    /// over it), a substitute mediator, the real AutoMapper profiles, and in-memory configuration and options.
    /// Create one per unit of work and dispose it.
    /// </summary>
    public sealed class ServiceFactory : IAsyncDisposable
    {
        private static readonly Lazy<IMapper> SharedMapper = new(() => new ServiceCollection()
            .AddAutoMapper(typeof(Application.Extensions.ServiceCollectionExtensions).Assembly,
                typeof(Infrastructure.Extensions.ServiceCollectionExtensions).Assembly)
            .BuildServiceProvider()
            .GetRequiredService<IMapper>());

        private readonly TestDatabase database;
        private readonly PassThroughCacheService cache = new();

        public ServiceFactory(TestDatabase database)
        {
            this.database = database;
            Context = database.CreateContext<HarmonyContext>();
        }

        public HarmonyContext Context { get; }

        public IMediator Mediator { get; } = Substitute.For<IMediator>();

        public CardRepository CardRepository => new(Context, cache);

        public BoardService BoardService => new(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:HarmonyConnection"] = database.ConnectionString })
                .Build(),
            new BoardRepository(Context),
            new UserBoardRepository(Context),
            new UserWorkspaceRepository(Context),
            new SprintRepository(Context),
            new IssueTypeRepository(Context),
            CardRepository,
            SharedMapper.Value,
            Mediator,
            cache,
            new UserCardRepository(Context),
            new AttachmentRepository(Context),
            new CheckListItemRepository(Context),
            new CommentRepository(Context),
            new CardLabelRepository(Context),
            new CardSummaryService(cache),
            new LinkRepository(Context));

        public CardService CardService => new(
            CardRepository,
            new BoardRepository(Context),
            new IssueTypeRepository(Context),
            new BoardListRepository(Context),
            Options.Create(new AppEndpointConfiguration()),
            Mediator);

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }
}
