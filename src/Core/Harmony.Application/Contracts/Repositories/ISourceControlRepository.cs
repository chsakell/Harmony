
using Harmony.Domain.SourceControl;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ISourceControlRepository
    {
        Task CreateBranch(Branch branch);
        Task<bool> DeleteBranch(string name);
    }
}
