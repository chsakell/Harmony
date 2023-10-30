using Bogus;
using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Workspaces.Commands.Create;
using Harmony.Domain.Entities;
using Harmony.Infrastructure.Repositories;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Application;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICardRepository _cardRepository;
        private readonly UserManager<HarmonyUser> _userManager;

        private HarmonyUser _admin;
        private List<string> _boardUsers = new List<string>();
        public int Order => 3;

        public DatabaseWorkspaceSeeder(HarmonyContext context,
            ISender sender,
            IWorkspaceRepository workspaceRepository,
            IUserWorkspaceRepository userWorkspaceRepository,
            IBoardRepository boardRepository,
            IUserBoardRepository userBoardRepository,
            ICheckListRepository checkListRepository,
            ICheckListItemRepository checkListItemRepository,
            IBoardListRepository boardListRepository,
            ICardRepository cardRepository,
            UserManager<HarmonyUser> userManager)
        {
            _context = context;
            _sender = sender;
            _workspaceRepository = workspaceRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _boardRepository = boardRepository;
            _userBoardRepository = userBoardRepository;
            _checkListRepository = checkListRepository;
            _checkListItemRepository = checkListItemRepository;
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
            var labels = new List<Label>();
            foreach (var colour in LabelColorsConstants.GetDefaultColors())
            {
                labels.Add(new Label()
                {
                    Colour = colour,
                });
            }

            var harmonyBoard = new Board()
            {
                WorkspaceId = workspaceId,
                Title = "Harmony",
                Description = "The best project management tool ever",
                Visibility = Domain.Enums.BoardVisibility.Workspace,
                UserId = _admin.Id,
                Labels = labels
            };

            await _boardRepository.CreateAsync(harmonyBoard);

            var userBoard = new UserBoard()
            {
                UserId = _admin.Id,
                BoardId = harmonyBoard.Id,
            };
            
            await _userBoardRepository.CreateAsync(userBoard);

            await AddBoardMembers(workspaceId, harmonyBoard.Id);

            await CreateList(harmonyBoard.Id, "TODO", 0);
            await CreateList(harmonyBoard.Id, "IN PROGRESS", 1);
            await CreateList(harmonyBoard.Id, "DOING", 2);
            await CreateList(harmonyBoard.Id, "DONE", 3);
        }

        private async Task AddBoardMembers(Guid workspaceId, Guid boardId)
        {
            _boardUsers = await _userManager.Users
                .Where(u => u.Id != _admin.Id)
                .Select(u => u.Id).Take(15).ToListAsync();

            foreach (var user in _boardUsers)
            {
                var userBoard = new UserBoard()
                {
                    UserId = user,
                    BoardId = boardId,
                };

                var userWorkspace = new UserWorkspace()
                {
                    UserId = user,
                    WorkspaceId = workspaceId
                };

                await _userBoardRepository.CreateAsync(userBoard);
                await _userWorkspaceRepository.CreateAsync(userWorkspace);
            }
        }

        private async Task CreateList(Guid boardId, string title, short order)
        {
            var faker = new Faker(locale: "en");

            var todoList = new BoardList()
            {
                Title = title,
                UserId = faker.PickRandom(_boardUsers),
                BoardId = boardId,
                Status = Domain.Enums.BoardListStatus.Active,
                Position = order
            };

            await _boardListRepository.CreateAsync(todoList);

            await GenerateCards(todoList.Id);
        }

        private async Task GenerateCards(Guid boardListId)
        {
            for (var i = 0; i< 10; i++ )
            {
                var lorem = new Bogus.DataSets.Lorem(locale: "en");
                var faker = new Faker(locale: "en");

                var randomMembers = faker.PickRandom(_boardUsers, 3);
                var members = new List<UserCard>();

                foreach (var member in randomMembers)
                {
                    members.Add(new UserCard()
                    {
                        UserId = member
                    });
                }

                var card = new Card()
                {
                    BoardListId = boardListId,
                    UserId= faker.PickRandom(_boardUsers),
                    Title = string.Join(" ", lorem.Words(15)),
                    Description = lorem.Sentences(),
                    StartDate = faker.Date.Between(DateTime.Now.AddDays(-100), DateTime.Now),
                    DueDate = faker.Date.Between(DateTime.Now, DateTime.Now.AddDays(30)),
                    Position = (short)i,
                    Status = Domain.Enums.CardStatus.Active,
                    Members = members
                };

                await _cardRepository.CreateAsync(card);

                await GenerateCheckLists(card.Id);
            }
        }

        private async Task GenerateCheckLists(Guid cardId)
        {
            for(var i = 0; i < 2; i++ )
            {
                var lorem = new Bogus.DataSets.Lorem(locale: "en");

                var checkList = new CheckList()
                {
                    CardId = cardId,
                    UserId = _admin.Id,
                    Position = (byte)i,
                    Title = string.Join(" ", lorem.Words(5))
                };

                await _checkListRepository.CreateAsync(checkList);

                await GenerateCheckListItems(checkList.Id);
            }
        }

        private async Task GenerateCheckListItems(Guid checklistId)
        {
            for (var i = 0; i < 3; i++)
            {
                var lorem = new Bogus.DataSets.Lorem(locale: "en");
                var faker = new Faker(locale: "en");

                var checkListItem = new CheckListItem()
                {
                    CheckListId = checklistId,
                    Position = (byte)i,
                    Description = lorem.Sentences(3),
                    IsChecked = faker.Random.Bool(),
                    DueDate = faker.Date.Future()
                };

                await _checkListItemRepository.CreateAsync(checkListItem);
            }
        }
    }
}
