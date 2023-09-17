namespace Harmony.Application.Contracts.Persistence
{
    public interface IDatabaseSeeder
    {
        int Order { get; }
        Task Initialize();
    }
}
