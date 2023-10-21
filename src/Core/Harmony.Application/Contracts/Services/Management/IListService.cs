using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface IListService
	{
		Task<bool> ReorderAfterArchive(BoardList list);
	}
}
