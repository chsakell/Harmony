using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Entities;
using static Harmony.Application.Notifications.BoardListArchivedMessage;


namespace Harmony.Infrastructure.Services.Management
{
    public class ListService : IListService
	{
		private readonly IBoardListRepository _listRepository;

		public ListService(IBoardListRepository listRepository)
        {
			_listRepository = listRepository;
		}

        public async Task<List<BoardListOrder>> ReorderAfterArchive(BoardList list)
        {
            var newPositions = new List<BoardListOrder>();

            var currentPosition = list.Position;

            // find all lists with position > currentPosition and decline by 1
            var listsToReOrder = await _listRepository.GetListsInPositionGreaterThan(list.BoardId, currentPosition);

            foreach(var boardList in listsToReOrder)
            {
                boardList.Position = (short)(boardList.Position - 1);
                _listRepository.UpdateEntry(boardList);

                newPositions.Add(new BoardListOrder(boardList.Id, boardList.Position));
            }

            list.Position = -1;

            return newPositions;
        }
    }
}
