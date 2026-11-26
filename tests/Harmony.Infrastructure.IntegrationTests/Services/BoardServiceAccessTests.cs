using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Services
{
    public class BoardServiceAccessTests(RelationalDatabaseFixture databases) : IClassFixture<RelationalDatabaseFixture>
    {
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-031")]
        public async Task HasUserAccessToBoard_PrivateBoard_GrantsMemberRefusesWorkspaceMember(RelationalProvider provider)
        {
            var start = await NewBoardAsync(provider);
            var board = await start.Data.BoardAsync(start.Workspace, start.User, BoardVisibility.Private);
            var boardMember = await start.Data.UserAsync();
            await start.Data.BoardMemberAsync(board, boardMember);
            var workspaceMember = await start.Data.UserAsync();
            await start.Data.WorkspaceMemberAsync(start.Workspace, workspaceMember);

            (await HasAccessAsync(start, boardMember.Id, board.Id)).ShouldBeTrue();
            (await HasAccessAsync(start, workspaceMember.Id, board.Id)).ShouldBeFalse();
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-031")]
        public async Task HasUserAccessToBoard_WorkspaceBoard_GrantsWorkspaceMemberRefusesOutsider(RelationalProvider provider)
        {
            var start = await NewBoardAsync(provider);
            var board = await start.Data.BoardAsync(start.Workspace, start.User, BoardVisibility.Workspace);
            var workspaceMember = await start.Data.UserAsync();
            await start.Data.WorkspaceMemberAsync(start.Workspace, workspaceMember);
            var outsider = await start.Data.UserAsync();

            (await HasAccessAsync(start, workspaceMember.Id, board.Id)).ShouldBeTrue();
            (await HasAccessAsync(start, outsider.Id, board.Id)).ShouldBeFalse();
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-031")]
        public async Task HasUserAccessToBoard_PublicBoard_GrantsAnyone(RelationalProvider provider)
        {
            var start = await NewBoardAsync(provider);
            var board = await start.Data.BoardAsync(start.Workspace, start.User, BoardVisibility.Public);
            var anyone = await start.Data.UserAsync();

            (await HasAccessAsync(start, anyone.Id, board.Id)).ShouldBeTrue();
        }

        /// <summary>
        /// Known-wrong (spec, Flagged concern 5b; roadmap R-08): FR-031 says access to a board that does not exist is
        /// refused, but BoardRepository.GetAsync returns null and HasUserAccessToBoard then reads its Visibility.
        /// </summary>
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-031")]
        public async Task HasUserAccessToBoard_UnknownBoard_Throws_KnownIssue(RelationalProvider provider)
        {
            var start = await NewBoardAsync(provider);

            await Should.ThrowAsync<NullReferenceException>(() => HasAccessAsync(start, start.User.Id, Guid.NewGuid()));
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-032")]
        public async Task GetUserBoards_MemberOfPrivateAndPublic_ReturnsEachOnce(RelationalProvider provider)
        {
            var start = await NewBoardAsync(provider);
            var user = await start.Data.UserAsync();
            await start.Data.WorkspaceMemberAsync(start.Workspace, user);
            var privateBoard = await start.Data.BoardAsync(start.Workspace, start.User, BoardVisibility.Private);
            var publicBoard = await start.Data.BoardAsync(start.Workspace, start.User, BoardVisibility.Public);
            await start.Data.BoardMemberAsync(privateBoard, user);
            await start.Data.BoardMemberAsync(publicBoard, user);
            await start.Data.BoardAsync(start.Workspace, start.User, BoardVisibility.Private);

            var boards = await GetUserBoardsAsync(start, null, user.Id);

            boards.ShouldBe([start.Board.Id, privateBoard.Id, publicBoard.Id], ignoreOrder: true);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-032")]
        public async Task GetUserBoards_WorkspaceGiven_ReturnsOnlyThatWorkspace(RelationalProvider provider)
        {
            var start = await NewBoardAsync(provider);
            var user = await start.Data.UserAsync();
            await start.Data.WorkspaceMemberAsync(start.Workspace, user);
            var otherWorkspace = await start.Data.WorkspaceAsync(start.User);
            await start.Data.WorkspaceMemberAsync(otherWorkspace, user);
            await start.Data.BoardAsync(otherWorkspace, start.User, BoardVisibility.Workspace);

            var boards = await GetUserBoardsAsync(start, start.Workspace.Id, user.Id);

            boards.ShouldBe([start.Board.Id]);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-021")]
        public async Task GetUserBoards_InactiveWorkspace_ExcludesItsBoards(RelationalProvider provider)
        {
            var start = await NewBoardAsync(provider);
            var user = await start.Data.UserAsync();
            await start.Data.WorkspaceMemberAsync(start.Workspace, user);
            var inactive = await start.Data.WorkspaceAsync(start.User, WorkspaceStatus.Archived);
            await start.Data.WorkspaceMemberAsync(inactive, user);
            await start.Data.BoardAsync(inactive, start.User, BoardVisibility.Workspace);
            var memberBoard = await start.Data.BoardAsync(inactive, start.User, BoardVisibility.Private);
            await start.Data.BoardMemberAsync(memberBoard, user);

            var boards = await GetUserBoardsAsync(start, null, user.Id);

            boards.ShouldBe([start.Board.Id]);
        }

        private static async Task<bool> HasAccessAsync(TestBoard start, string userId, Guid boardId)
        {
            await using var services = new ServiceFactory(start.Database);
            return await services.BoardService.HasUserAccessToBoard(userId, boardId);
        }

        private static async Task<List<Guid>> GetUserBoardsAsync(TestBoard start, Guid? workspaceId, string userId)
        {
            await using var services = new ServiceFactory(start.Database);
            List<Board> boards = await services.BoardService.GetUserBoards(workspaceId, userId);
            return boards.Select(board => board.Id).ToList();
        }

        /// <summary>
        /// The start board is workspace-visible, in a workspace that the start user owns but is not a member of.
        /// </summary>
        private async Task<TestBoard> NewBoardAsync(RelationalProvider provider) =>
            await TestBoard.CreateAsync(await databases.GetAsync(provider));
    }
}
