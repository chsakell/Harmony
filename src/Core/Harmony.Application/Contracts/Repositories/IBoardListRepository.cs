using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IBoardListRepository
    {
        Task<int> CountLists(Guid boardId);
        Task<int> Add(BoardList boardList);
	}
}
