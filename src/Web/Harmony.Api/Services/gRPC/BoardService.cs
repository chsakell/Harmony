using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Extensions;
using Harmony.Domain.Entities;
using Harmony.Application.Specifications.Boards;
using Harmony.Application.Contracts.Services.Management;

namespace Harmony.Api.Services.gRPC
{
    public class BoardService : Protos.BoardService.BoardServiceBase
    {
        private readonly ILogger<BoardService> _logger;
        private readonly IBoardRepository _boardRepository;
        private readonly IBoardService _boardService;

        public BoardService(ILogger<BoardService> logger, IBoardRepository boardRepository,
            IBoardService boardService)
        {
            _logger = logger;
            _boardRepository = boardRepository;
            _boardService = boardService;
        }

        public override async Task<Protos.BoardResponse> GetBoard(Protos.BoardFilterRequest request,
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

        public override async Task<Protos.AccessResponse> HasUserAccessToBoard(Protos.UserBoardAccessRequest request, ServerCallContext context)
        {
            var userHasAccessToBoard = await _boardService
                .HasUserAccessToBoard(request.UserId, Guid.Parse(request.BoardId));

            return new Protos.AccessResponse()
            {
                HasAccess = userHasAccessToBoard
            };
        }

        private Protos.BoardResponse MapToProto(Board board)
        {
            if (board == null)
            {
                return new Protos.BoardResponse()
                {
                    Found = false
                };
            }

            return new Protos.BoardResponse()
            {
                Found = true,
                Board = MapToProtoBoard(board)
            };
        }

        private Protos.Board MapToProtoBoard(Board board)
        {
            var proto = new Protos.Board()
            {
                Id = board.Id.ToString(),
                Title = board.Title
            };

            if (board.Workspace != null)
            {
                proto.Workspace = new Protos.BoardWorkspace()
                {
                    Id = board.Workspace.Id.ToString(),
                    Name = board.Workspace.Name
                };
            }

            return proto;
        }
    }
}
