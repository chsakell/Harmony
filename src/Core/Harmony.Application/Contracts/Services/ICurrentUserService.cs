namespace Harmony.Application.Contracts.Services
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string FullName { get; }
    }
}
