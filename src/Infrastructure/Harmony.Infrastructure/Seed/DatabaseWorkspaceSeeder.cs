using Bogus;
using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Domain.Entities;
using Harmony.Infrastructure.Repositories;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Harmony.Shared.Constants.Permission.Permissions;

namespace Harmony.Infrastructure.Seed
{
    public class DatabaseWorkspaceSeeder : IDatabaseSeeder
    {
        private readonly HarmonyContext _context;
        private readonly ISender _sender;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICardRepository _cardRepository;
        private readonly UserManager<HarmonyUser> _userManager;
        private HarmonyUser _admin;
        public int Order => 3;

        public DatabaseWorkspaceSeeder(HarmonyContext context,
            ISender sender,
            IWorkspaceRepository workspaceRepository,
            IUserWorkspaceRepository userWorkspaceRepository,
            IBoardRepository boardRepository,
            IBoardListRepository boardListRepository,
            ICardRepository cardRepository,
            UserManager<HarmonyUser> userManager)
        {
            _context = context;
            _sender = sender;
            _workspaceRepository = workspaceRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _boardRepository = boardRepository;
            _boardListRepository = boardListRepository;
            _cardRepository = cardRepository;
            _userManager = userManager;
        }

        public async Task Initialize()
        {
            _admin = await _userManager.FindByEmailAsync("admin@harmony.com");

            await CreateWorkspace();
        }

        private async Task CreateWorkspace()
        {
            var workspaceExists = _context.Workspaces.Any(w => w.Name == "Engineering");

            if(workspaceExists)
            {
                return;
            }

            var engineering = new Workspace()
            {
                UserId = _admin.Id,
                Name = "Engineering",
                Description = "Workspace for engineers",
                IsPublic = false,
            };

            await _workspaceRepository.CreateAsync(engineering);

            var userWorkspace = new UserWorkspace()
            {
                UserId = _admin.Id,
                WorkspaceId = engineering.Id
            };

            var dbResult = await _userWorkspaceRepository.CreateAsync(userWorkspace);

            await CreateBoards(engineering.Id);
        }

        private async Task CreateBoards(Guid workspaceId)
        {
            var harmonyBoard = new Board()
            {
                WorkspaceId = workspaceId,
                Title = "Harmony",
                Description = "The best project management tool ever",
                Visibility = Domain.Enums.BoardVisibility.Workspace,
                UserId = _admin.Id
            };

            await _boardRepository.CreateAsync(harmonyBoard);

            await CreateList(harmonyBoard.Id, "TODO");
            await CreateList(harmonyBoard.Id, "IN PROGRESS");
            await CreateList(harmonyBoard.Id, "DOING");
            await CreateList(harmonyBoard.Id, "DONE");
        }

        private async Task CreateList(Guid boardId, string title)
        {
            var todoList = new BoardList()
            {
                Title = title,
                UserId = _admin.Id,
                BoardId = boardId,
                Status = Domain.Enums.BoardListStatus.Active,
                Position = 0
            };

            await _boardListRepository.CreateAsync(todoList);

            await GenerateCards(todoList.Id);
        }

        private async Task GenerateCards(Guid boardListId)
        {
            for (var i = 0; i< 30; i++ )
            {
                var lorem = new Bogus.DataSets.Lorem(locale: "en");
                var faker = new Faker(locale: "en");

                var card = new Card()
                {
                    BoardListId = boardListId,
                    UserId= _admin.Id,
                    Title = string.Join(" ", lorem.Words(15)),
                    Description = lorem.Sentences(),
                    StartDate = faker.Date.Between(DateTime.Now.AddDays(-100), DateTime.Now),
                    DueDate = faker.Date.Between(DateTime.Now, DateTime.Now.AddDays(30)),
                    Position = (short)i,
                    Status = Domain.Enums.CardStatus.Active
                };

                await _cardRepository.CreateAsync(card);
            }
        }
    }
}
