namespace Harmony.Application.Contracts.Persistence
{
    /// <summary>
    /// Interface for seeding implementations for Mock data
    /// </summary>
    public interface IDatabaseSeeder
    {
        int Order { get; }
        Task Initialize();
    }
}
