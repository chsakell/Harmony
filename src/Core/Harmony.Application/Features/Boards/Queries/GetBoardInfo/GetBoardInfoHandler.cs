using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetBoardInfo
{
    /// <summary>
    /// Handler for getting a board's info
    /// </summary>
    public class GetBoardInfoHandler : IRequestHandler<GetBoardInfoQuery, Result<BoardInfo>>
    {
        private readonly IBoardService _boardService;

        public GetBoardInfoHandler(IBoardService boardService)
        {
            _boardService = boardService;
        }

        public async Task<Result<BoardInfo>> Handle(GetBoardInfoQuery request, CancellationToken cancellationToken)
        {
            var boardInfo = await _boardService.GetBoardInfo(request.BoardId);

            return await Result<BoardInfo>.SuccessAsync(boardInfo);
        }
    }
}
