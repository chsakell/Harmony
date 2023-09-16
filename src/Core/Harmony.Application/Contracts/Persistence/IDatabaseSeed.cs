namespace Harmony.Application.Contracts.Persistence
{
    public interface IDatabaseSeed
    {
        int Order { get; }
        Task Initialize();
    }
}
