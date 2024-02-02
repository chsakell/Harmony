using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly HarmonyContext _context;

        public BoardRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<Board> Entities => _context.Set<Board>();

        public async Task<Board?> GetAsync(Guid boardId)
        {
            return await _context.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == boardId);
        }

        public async Task AddAsync(Board Board)
        {
            await _context.Boards.AddAsync(Board);
        }

        public async Task<int> CreateAsync(Board Board)
        {
            await _context.Boards.AddAsync(Board);

            return await _context.SaveChangesAsync();
        }

        public async Task LoadWorkspaceEntryAsync(Board board)
        {
            await _context.Entry(board).Reference(board => board.Workspace).LoadAsync();
        }

        public async Task LoadBoardListEntryAsync(Board board)
        {
            await _context.Entry(board).Collection(board => board.Lists).LoadAsync();
        }

        public async Task<bool> Exists(Guid boardId)
        {
            return await _context.Boards
                .Where(b => b.Id == boardId)
                .CountAsync() > 0;
        }

        public async Task<bool> Exists(Guid workspaceId, string key)
        {
            return await _context.Boards
                .Where(b => b.Key == key && b.WorkspaceId == workspaceId)
                .CountAsync() > 0;
        }

        public async Task<Board> LoadBoard(Guid boardId, int maxCardsPerList)
        {
            return await _context.Boards
                .Include(b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                    .ThenInclude(l => l.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active).Take(maxCardsPerList))
                        .ThenInclude(c => c.CheckLists)
                            .ThenInclude(cl => cl.Items)
                .Include(b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                    .ThenInclude(l => l.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active).Take(maxCardsPerList))
                    .ThenInclude(c => c.Attachments)
                .Include(b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                    .ThenInclude(l => l.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active).Take(maxCardsPerList))
                    .ThenInclude(c => c.Members)
                .Include(b => b.Users)
                .Include(b => b.Labels)
                    .ThenInclude(l => l.Labels)
                .FirstAsync(board => board.Id == boardId);
        }

        public async Task<Board> LoadBoardList(Guid boardId, Guid listId, int page, int maxCardsPerList)
        {
            return await _context.Boards
                .Include(b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                    .ThenInclude(l => l.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active && c.BoardListId == listId).Skip((page -1) * maxCardsPerList).Take(maxCardsPerList))
                        .ThenInclude(c => c.CheckLists)
                            .ThenInclude(cl => cl.Items)
                .Include(b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                    .ThenInclude(l => l.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active && c.BoardListId == listId).Skip((page - 1) * maxCardsPerList).Take(maxCardsPerList))
                    .ThenInclude(c => c.Attachments)
                .Include(b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                    .ThenInclude(l => l.Cards.Where(c => c.Status == Domain.Enums.CardStatus.Active && c.BoardListId == listId).Skip((page - 1) * maxCardsPerList).Take(maxCardsPerList))
                    .ThenInclude(c => c.Members)
                .Include(b => b.Users)
                .Include(b => b.Labels)
                    .ThenInclude(l => l.Labels)
                .FirstAsync(board => board.Id == boardId);


        }

        public async Task<Board?> GetBoardWithLists(Guid boardId)
		{
            return await _context.Boards
                .Include (b => b.Lists.Where(l => l.Status == Domain.Enums.BoardListStatus.Active))
                .FirstOrDefaultAsync(board => board.Id == boardId);
		}

		public async Task<int> Update(Board board)
		{
			_context.Boards.Update(board);

            return await _context.SaveChangesAsync();
		}
    }
}
