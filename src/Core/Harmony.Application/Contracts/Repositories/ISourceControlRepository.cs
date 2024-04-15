
using Harmony.Domain.SourceControl;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ISourceControlRepository
    {
        Task CreateRepository(Repository repo);
        Task<Repository> GetRepository(string repositoryId);
        Task CreateBranch(Branch branch);
        Task<Branch> GetBranch(string name, string repositoryId);
        Task<bool> DeleteBranch(string name);
    }
}
