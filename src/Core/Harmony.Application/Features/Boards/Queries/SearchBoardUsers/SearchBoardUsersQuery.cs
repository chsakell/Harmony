using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.SearchBoardUsers
{
    /// <summary>
    /// Query for searching a board's users
    /// </summary>
    public class SearchBoardUsersQuery : IRequest<Result<List<SearchBoardUserResponse>>>
    {
        public Guid BoardId { get; set; }
        public string SearchTerm { get; set; }

        public SearchBoardUsersQuery(Guid boardId, string searchTerm)
        {
            BoardId = boardId;
            SearchTerm = searchTerm;
        }
    }
}