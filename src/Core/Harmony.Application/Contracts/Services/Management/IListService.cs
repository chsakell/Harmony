using Harmony.Domain.Entities;
using static Harmony.Application.Events.BoardListArchivedEvent;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service for lists operations
    /// </summary>
    public interface IListService
	{
		Task<List<BoardListOrder>> ReorderAfterArchive(BoardList list);
	}
}
