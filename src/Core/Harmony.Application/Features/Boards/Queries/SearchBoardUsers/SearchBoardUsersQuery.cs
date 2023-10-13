using Harmony.Application.DTO;
using Harmony.Application.Requests;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.SearchBoardUsers
{
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