namespace Harmony.Application.Contracts.Services
{
    /// <summary>
    /// Service to access logged in user
    /// </summary>
    public interface ICurrentUserService
    {
        string UserId { get; }
        string FullName { get; }
        bool IsTrustedClientRequest { get; }
        string GetHeader(string name);
    }
}
