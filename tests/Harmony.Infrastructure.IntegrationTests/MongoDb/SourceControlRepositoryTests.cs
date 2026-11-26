using Harmony.Domain.Enums.SourceControl;
using Harmony.Domain.SourceControl;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Harmony.Infrastructure.Repositories;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.MongoDb
{
    /// <summary>
    /// The GitHub activity store. Repositories and branches are built as the source-control command handlers
    /// build them: a new GUID as the ID and empty commit, pull request and tag lists.
    /// </summary>
    [Collection(MongoDbCollection.Name)]
    [Trait("Requirement", "FR-051")]
    public class SourceControlRepositoryTests(MongoDbFixture mongo) : IAsyncLifetime
    {
        private readonly SourceControlRepository repository = new(mongo.Client);

        public async ValueTask InitializeAsync() => await mongo.DropDatabasesAsync();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        [Fact]
        public async Task GetRepositoryAndBranch_Recorded_ReadBack()
        {
            var repo = await RepositoryAsync();
            var main = await BranchAsync(repo, "main");
            var feature = await BranchAsync(repo, "feature/harbour-lights");

            (await repository.GetRepository(repo.RepositoryId)).ShouldNotBeNull().Name.ShouldBe(repo.Name);
            (await repository.GetBranch(main.Name, repo.RepositoryId)).ShouldNotBeNull().Id.ShouldBe(main.Id);
            (await repository.GetBranch(feature.Name, repo.RepositoryId)).ShouldNotBeNull().Id.ShouldBe(feature.Id);
            (await repository.BranchExists(main.Name, repo.RepositoryId)).ShouldBeTrue();
            (await repository.BranchExists(feature.Name, repo.RepositoryId)).ShouldBeTrue();
            (await repository.BranchExists("no-such-branch", repo.RepositoryId)).ShouldBeFalse();
        }

        [Fact]
        public async Task SearchBranches_Term_SearchAndTotalAgree()
        {
            var repo = await RepositoryAsync();
            var match = await BranchAsync(repo, "feature/harbour-lights");
            await BranchAsync(repo, "bugfix/login");

            var found = await repository.SearchBranches("harbour");
            var total = await repository.GetTotalBranches("harbour");

            found.Select(branch => branch.Id).ShouldBe([match.Id]);
            total.ShouldBe(1);
        }

        [Fact]
        public async Task CreatePush_Commit_BranchFoundByCommitId()
        {
            var repo = await RepositoryAsync();
            var branch = await BranchAsync(repo, "feature/harbour-lights");
            var first = Commit("Add the lights");
            var head = Commit("Wire the lights");

            await repository.CreatePush(repo.RepositoryId, branch.Name, [first, head], head.Id);

            (await repository.FindBranchByCommit(repo.RepositoryId, first.Id)).ShouldNotBeNull().Id.ShouldBe(branch.Id);
            (await repository.FindBranchByCommit(repo.RepositoryId, head.Id)).ShouldNotBeNull().Id.ShouldBe(branch.Id);
        }

        [Fact]
        public async Task AddTagToBranch_StoresTag()
        {
            var repo = await RepositoryAsync();
            var branch = await BranchAsync(repo, "main");

            await repository.AddTagToBranch(branch.Id, repo.RepositoryId, "v1.0.0");

            (await repository.GetBranch(branch.Name, repo.RepositoryId)).ShouldNotBeNull().Tags.ShouldBe(["v1.0.0"]);
        }

        [Fact]
        public async Task AddOrUpdatePullRequest_SameIdTwice_UpdatesNotDuplicates()
        {
            var repo = await RepositoryAsync();
            var branch = await BranchAsync(repo, "feature/harbour-lights");

            await repository.AddOrUpdatePullRequest(repo.RepositoryId, PullRequest(branch, "Harbour lights"));
            await repository.AddOrUpdatePullRequest(repo.RepositoryId, PullRequest(branch, "Harbour lights, reviewed"));

            var stored = (await repository.GetBranch(branch.Name, repo.RepositoryId)).ShouldNotBeNull();
            stored.PullRequests.ShouldHaveSingleItem().Title.ShouldBe("Harbour lights, reviewed");
        }

        [Fact]
        public async Task DeleteBranch_RemovesIt()
        {
            var repo = await RepositoryAsync();
            var branch = await BranchAsync(repo, "feature/harbour-lights");

            (await repository.DeleteBranch(branch.Name)).ShouldBeTrue();

            (await repository.GetBranch(branch.Name, repo.RepositoryId)).ShouldBeNull();
            (await repository.BranchExists(branch.Name, repo.RepositoryId)).ShouldBeFalse();
        }

        private async Task<Repository> RepositoryAsync()
        {
            var repositoryId = Guid.NewGuid().ToString("N")[..9];
            var repo = new Repository
            {
                Id = Guid.NewGuid().ToString(),
                RepositoryId = repositoryId,
                Name = $"repo-{repositoryId}",
                FullName = $"harmony-tests/repo-{repositoryId}",
                Url = $"https://github.example.test/harmony-tests/repo-{repositoryId}",
                Provider = SourceControlProvider.GitHub,
            };
            await repository.CreateRepository(repo);
            return repo;
        }

        private async Task<Branch> BranchAsync(Repository repo, string name)
        {
            var branch = new Branch
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                RepositoryId = repo.RepositoryId,
                Creator = new RepositoryUser("octo-tester", "https://avatars.example.test/1", "https://github.example.test/octo-tester"),
                Commits = [],
                PullRequests = [],
                Tags = [],
            };
            await repository.CreateBranch(branch);
            return branch;
        }

        private static Commit Commit(string message) => new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Message = message,
            Timestamp = DateTime.UtcNow,
            Url = "https://github.example.test/commit",
            Author = new Author { Name = "Octo Tester", Email = "octo@example.test", Username = "octo-tester" },
            Added = [],
            Removed = [],
            Modified = [],
        };

        private static PullRequest PullRequest(Branch branch, string title) => new()
        {
            Id = "pr-1",
            Number = 1,
            Title = title,
            State = PullRequestState.Opened,
            SourceBranch = branch.Name,
            TargetBranch = "main",
            HtmlUrl = "https://github.example.test/pull/1",
            Assignees = [],
            Reviewers = [],
        };
    }
}
