﻿using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Infrastructure.Repositories
{
    public class BoardListRepository : IBoardListRepository
    {
        private readonly HarmonyContext _context;

        public BoardListRepository(HarmonyContext context)
        {
            _context = context;
        }

		public async Task<int> CountLists(Guid boardId)
		{
			return await _context.BoardLists.Where(bl => bl.BoardId == boardId).CountAsync();
		}

		public async Task<int> Add(BoardList boardList)
		{
			_context.BoardLists.Add(boardList);

			return await _context.SaveChangesAsync();
		}
	}
}