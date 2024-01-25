using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Extensions;
using Harmony.Domain.Entities;
using Harmony.Application.Specifications.Boards;

namespace Harmony.Api.Services.gRPC
{
    public class BoardService : Protos.BoardService.BoardServiceBase
    {
        private readonly ILogger<BoardService> _logger;
        private readonly IBoardRepository _boardRepository;

        public BoardService(ILogger<BoardService> logger, IBoardRepository boardRepository)
        {
            _logger = logger;
            _boardRepository = boardRepository;
        }

        public override async Task<Protos.Board> GetBoard(Protos.BoardFilterRequest request,
            ServerCallContext context)
        {
            var includes = new BoardIncludes()
            {
                Workspace = request.Workspace,
                Lists = request.Lists
            };

            var filter = new BoardFilterSpecification(Guid.Parse(request.BoardId), includes);

            var board = await _boardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            return MapToProto(board);
        }

        private Protos.Board MapToProto(Board board)
        {
            if (board == null)
            {
                return null;
            }

            var proto = new Protos.Board()
            {
                Id = board.Id.ToString(),
                Title = board.Title
            };

            if(board.Workspace != null)
            {
                proto.Workspace = new Protos.Workspace()
                {
                    Id = board.Workspace.Id.ToString(),
                    Name = board.Workspace.Name
                };
            }

            return proto;
        }
    }
}
