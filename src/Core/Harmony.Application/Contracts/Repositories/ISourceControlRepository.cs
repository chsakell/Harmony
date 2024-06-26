﻿
using Harmony.Domain.SourceControl;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ISourceControlRepository
    {
        Task CreateRepository(Repository repo);
        Task<Repository> GetRepository(string repositoryId);
        Task<List<Repository>> GetRepositories(List<string> repositories);
        Task CreateBranch(Branch branch);
        Task AddOrUpdatePullRequest(string repositoryId, PullRequest pullRequest);
        Task CreatePush(string repositoryId, string branch, List<Commit> commits, string headCommitId);
        Task<Branch> FindBranchByCommit(string repositoryId, string commitId);
        Task<Branch> GetBranch(string name, string repositoryId);
        Task<bool> BranchExists(string name, string repositoryId);
        Task<long> GetTotalBranches(string term);
        Task<List<Branch>> SearchBranches(string term);
        Task<bool> DeleteBranch(string name);
        Task AddTagToBranch(string branchId, string repositoryId, string tag);
        Task<bool> Ping(string database);
    }
}
