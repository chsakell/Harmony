using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Entities;

namespace Harmony.Infrastructure.Services.Management
{
    public class ListService : IListService
	{
		private readonly IBoardListRepository _listRepository;

		public ListService(IBoardListRepository listRepository)
        {
			_listRepository = listRepository;
		}

        public Task<bool> ReorderAfterArchive(BoardList list)
        {
            throw new NotImplementedException();
        }
    }
}
