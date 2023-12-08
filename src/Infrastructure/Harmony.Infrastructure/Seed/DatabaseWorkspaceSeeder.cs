using Bogus;
using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Application;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Seed
{
    /// <summary>
    /// Seeder for mock workspace/board (optional)
    /// </summary>
    public class DatabaseWorkspaceSeeder : IDatabaseSeeder
    {
        private readonly HarmonyContext _context;
        private readonly ISender _sender;
        private readonly IWorkspaceRepository _workspaceRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IBoardLabelRepository _boardLabelRepository;
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICheckListRepository _checkListRepository;
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly ICardRepository _cardRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly UserManager<HarmonyUser> _userManager;

        private HarmonyUser _admin;
        private List<string> _boardUsers = new List<string>();
        private List<IssueType> _issueTypes;
        public int Order => 3;
        private static int SerialNumber;
        private bool _active = false;

        public DatabaseWorkspaceSeeder(HarmonyContext context,
            ISender sender,
            IWorkspaceRepository workspaceRepository,
            IUserWorkspaceRepository userWorkspaceRepository,
            IBoardRepository boardRepository,
            IBoardLabelRepository boardLabelRepository,
            IUserBoardRepository userBoardRepository,
            ICheckListRepository checkListRepository,
            ICheckListItemRepository checkListItemRepository,
            IBoardListRepository boardListRepository,
            ICardRepository cardRepository,
            ISprintRepository sprintRepository,
            UserManager<HarmonyUser> userManager)
        {
            _context = context;
            _sender = sender;
            _workspaceRepository = workspaceRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _boardRepository = boardRepository;
            _boardLabelRepository = boardLabelRepository;
            _userBoardRepository = userBoardRepository;
            _checkListRepository = checkListRepository;
            _checkListItemRepository = checkListItemRepository;
            _boardListRepository = boardListRepository;
            _cardRepository = cardRepository;
            _sprintRepository = sprintRepository;
            _userManager = userManager;
        }

        public async Task Initialize()
        {
            if(!_active)
            {
                return;
            }

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

            await CreateBoard(engineering.Id, BoardType.Kanban);
            await CreateBoard(engineering.Id, BoardType.Scrum);
        }

        private async Task CreateBoard(Guid workspaceId, BoardType type)
        {
            var labels = new List<Label>();
            foreach (var colour in LabelColorsConstants.GetDefaultColors())
            {
                labels.Add(new Label()
                {
                    Colour = colour,
                });
            }

            var issueTypes = new List<IssueType>();
            foreach (var issueType in IssueTypesConstants.GetDefaultIssueTypes())
            {
                issueTypes.Add(new IssueType()
                {
                    Summary = issueType
                });
            }

            var board = new Board()
            {
                WorkspaceId = workspaceId,
                Title = type == BoardType.Kanban ? "Harmony" : "HR System",
                Description = type == BoardType.Kanban ? "The best project management tool ever" : "CMS for HR operations" ,
                Visibility = BoardVisibility.Workspace,
                UserId = _admin.Id,
                Labels = labels,
                Type = type,
                IssueTypes = issueTypes,
                Key = type == BoardType.Kanban ? "HARM" : "HRS"
            };

            await _boardRepository.CreateAsync(board);

            _issueTypes = board.IssueTypes;

            var userBoard = new UserBoard()
            {
                UserId = _admin.Id,
                BoardId = board.Id,
            };
            
            await _userBoardRepository.CreateAsync(userBoard);


            await AddBoardMembers(workspaceId, board.Id);

            await CreateList(board, "TODO", 0);
            await CreateList(board, "IN PROGRESS", 1);
            await CreateList(board, "TESTING", 2);
            await CreateList(board, "DONE", 3, BoardListCardStatus.DONE);
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

                var userWorkspace = await _userWorkspaceRepository.GetUserWorkspace(workspaceId, user);

                if(userWorkspace == null)
                {
                    userWorkspace = new UserWorkspace()
                    {
                        UserId = user,
                        WorkspaceId = workspaceId
                    };

                    await _userWorkspaceRepository.CreateAsync(userWorkspace);
                }

                await _userBoardRepository.CreateAsync(userBoard);
            }
        }

        private async Task CreateList(Board board, string title, short order, BoardListCardStatus? status = null)
        {
            var faker = new Faker(locale: "en");

            var todoList = new BoardList()
            {
                Title = title,
                UserId = faker.PickRandom(_boardUsers),
                BoardId = board.Id,
                Status = BoardListStatus.Active,
                Position = order,
                CardStatus = status
            };

            await _boardListRepository.CreateAsync(todoList);

            await GenerateCards(board, todoList.Id);
        }

        private async Task GenerateCards(Board board, Guid boardListId)
        {
            var labels = await _boardLabelRepository.GetLabels(board.Id);
            Sprint sprint = null;

            if (board.Type == BoardType.Scrum)
            {
                sprint = new Sprint()
                {
                    Name = "Iteration One",
                    Goal = "Build home screen",
                    BoardId = board.Id,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(14),
                    Status = SprintStatus.Active
                };

                await _sprintRepository.CreateAsync(sprint);
            }

            for (var i = 0; i< 5; i++ )
            {
                var lorem = new Bogus.DataSets.Lorem(locale: "en");
                var faker = new Faker(locale: "en");

                var randomMembers = faker.PickRandom(_boardUsers, 3);
                var issueType = faker.PickRandom(_issueTypes);

                var members = new List<UserCard>();

                foreach (var member in randomMembers)
                {
                    members.Add(new UserCard()
                    {
                        UserId = member
                    });
                }

                var randomLabels = faker.PickRandom(labels, 2);
                var cardLabels = new List<CardLabel>();

                foreach (var label in randomLabels)
                {
                    var cardLabel = new CardLabel()
                    {
                        LabelId = label.Id
                    };

                    cardLabels.Add(cardLabel);
                }

                var card = new Card()
                {
                    BoardListId = boardListId,
                    UserId= faker.PickRandom(_boardUsers),
                    Title = string.Join(" ", lorem.Words(3)),
                    Description = lorem.Sentences(),
                    StartDate = faker.Date.Between(DateTime.Now.AddDays(-100), DateTime.Now),
                    DueDate = faker.Date.Between(DateTime.Now, DateTime.Now.AddDays(30)),
                    Position = (short)i,
                    Status = Domain.Enums.CardStatus.Active,
                    Members = members,
                    Labels = cardLabels,
                    IssueType = issueType,
                    SprintId = board.Type == BoardType.Kanban ? null : sprint.Id,
                    SerialNumber = SerialNumber
                };

                SerialNumber += 1;

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
