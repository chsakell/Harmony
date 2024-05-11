using Dapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Models;
using Harmony.Application.Specifications.Boards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using Harmony.Application.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Harmony.Application.Constants;
using Harmony.Shared.Utilities;
using Harmony.Application.Features.Boards.Queries.GetSprintsSummary;
using Google.Protobuf.Collections;
using AutoMapper;
using MediatR;
using Harmony.Application.Features.Workspaces.Queries.GetIssueTypes;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Specifications.Cards;
using Harmony.Application.DTO.Summaries;

namespace Harmony.Infrastructure.Services.Management
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly IUserCardRepository _userCardRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly ICheckListItemRepository _checkListItemRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ICardLabelRepository _cardLabelRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly string _connectionString;

        public BoardService(IConfiguration configuration, IBoardRepository boardRepository,
            IUserBoardRepository userBoardRepository, IUserWorkspaceRepository userWorkspaceRepository,
            ISprintRepository sprintRepository,
            IIssueTypeRepository issueTypeRepository,
            ICardRepository cardRepository,
            IMapper mapper,
            IMediator mediator,
            ICacheService cacheService,
            IUserCardRepository userCardRepository,
            IAttachmentRepository attachmentRepository,
            ICheckListItemRepository checkListItemRepository,
            ICommentRepository commentRepository,
            ICardLabelRepository cardLabelRepository,
            ILinkRepository linkRepository)
        {
            _connectionString = configuration.GetConnectionString("HarmonyConnection");
            _boardRepository = boardRepository;
            _userBoardRepository = userBoardRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _sprintRepository = sprintRepository;
            _issueTypeRepository = issueTypeRepository;
            _cardRepository = cardRepository;
            _mapper = mapper;
            _mediator = mediator;
            _cacheService = cacheService;
            _userCardRepository = userCardRepository;
            _attachmentRepository = attachmentRepository;
            _checkListItemRepository = checkListItemRepository;
            _commentRepository = commentRepository;
            _cardLabelRepository = cardLabelRepository;
            _linkRepository = linkRepository;
        }

        public async Task<bool> HasUserAccessToBoard(string userId, Guid boardId)
        {
            var board = await _boardRepository.GetAsync(boardId);
            var hasAccess = false;

            switch (board.Visibility)
            {
                case BoardVisibility.Private:
                    var userBoard = await _userBoardRepository.GetBoardAccessMember(boardId, userId);
                    hasAccess = userBoard != null;
                    break;
                case BoardVisibility.Workspace:
                    var userWorkspace = await _userWorkspaceRepository.GetUserWorkspace(board.WorkspaceId, userId);
                    hasAccess = userWorkspace != null;
                    break;
                case BoardVisibility.Public:
                    hasAccess = true;
                    break;
            }

            return hasAccess;
        }

        public async Task<List<Board>> GetUserBoards(Guid? workspaceId, string userId)
        {
            var userWorkspaceBoardsQuery = _userWorkspaceRepository
                    .GetUserWorkspaceBoardsQuery(workspaceId, userId);

            var userBoardsQuery = _userBoardRepository.GetUserBoardsQuery(workspaceId, userId);

            var result = await userWorkspaceBoardsQuery.Union(userBoardsQuery).Distinct().ToListAsync();

            return result;
        }

        public async Task<List<Board>> GetStatusForBoards(List<Guid> boardIds)
        {

            return await _boardRepository.Entities.AsNoTracking()
                            .Include(b => b.Lists)
                                .ThenInclude(l => l.Cards.Where(c => c.Status == CardStatus.Active))
                            .Where(b => boardIds.Contains(b.Id))
                            .ToListAsync();
        }

        public async Task<List<Board>> GetUserBoardsWithLists(Guid? workspaceId, string userId)
        {
            var userWorkspaceBoardsQuery = _userWorkspaceRepository
                    .GetUserWorkspaceBoardsQuery(workspaceId, userId);

            var userBoardsQuery = _userBoardRepository.GetUserBoardsQuery(workspaceId, userId);

            var result = await userWorkspaceBoardsQuery
                    .Union(userBoardsQuery)
                    .Distinct().ToListAsync();

            foreach (var board in result)
            {
                await _boardRepository.LoadBoardListEntryAsync(board);
            }

            return result;
        }

        public async Task<BoardInfo?> GetBoardInfo(Guid boardId)
        {
            return await _cacheService.GetOrCreateAsync(CacheKeys.BoardInfo(boardId),
            async () =>
            {
                var filter = new BoardFilterSpecification()
                {
                    BoardId = boardId,
                    IncludeWorkspace = true,
                    IncludeLists = true
                };

                filter.Build();

                var board = await _boardRepository
                    .Entities.Specify(filter)
                    .FirstOrDefaultAsync();

                if (board == null)
                {
                    return null;
                }

                var issueTypes = await _mediator.Send(new GetIssueTypesQuery(boardId));

                var result = new BoardInfo()
                {
                    Id = board.Id,
                    Title = board.Title,
                    Type = board.Type,
                    Lists = _mapper.Map<List<BoardListDto>>(board.Lists),
                    IssueTypes = issueTypes.Data,
                    Key = board.Key,
                    IndexName = StringUtilities.SlugifyString($"{board.Workspace.Name}-{board.Title}"),
                    ActiveSprints = new List<SprintDto>()
                };

                if (board.Type == BoardType.Scrum)
                {
                    var activeSprints = await _sprintRepository.GetActiveSprints(board.Id);

                    if (activeSprints.Any())
                    {
                        result.ActiveSprints = [.. _mapper.Map<List<SprintDto>>(activeSprints)];
                    }
                }

                return result;
            }, TimeSpan.FromMinutes(5));
        }

        public async Task<Board> LoadBoard(Board board, int maxCardsPerList, Guid? sprintId = null)
        {
            var cards = await GetBoardListCards(board,
                 board.Lists.Select(x => x.Id).ToList(), 1, maxCardsPerList, sprintId);

            foreach (var boardList in board.Lists)
            {
                boardList.Cards = [.. cards.Where(card => card.BoardListId == boardList.Id)];
            }

            return board;
        }

        private async Task<CardSummary> GetCardSummary(Card card)
        {
            var summary = new CardSummary();

            var cardLabels = await _cardLabelRepository.GetLabelIds(card.Id);
            var checkListItems = await _checkListItemRepository.GetItems(card.CheckLists.Select(c => c.Id));
            var totalChildren = await _cardRepository.GetTotalChildren(card.Id);
            var cardMembers = await _userCardRepository.GetCardMembers(card.Id);
            var totalLinks = await _linkRepository.GetTotalLinks(card.Id);
            var totalComments = await _commentRepository.GetTotalComments(card.Id);
            var totalAttachments = await _attachmentRepository.CountAttachments(card.Id);

            summary.Labels = cardLabels;
            summary.TotalChildren = totalChildren;
            summary.Members = cardMembers;
            summary.TotalLinks = totalLinks;
            summary.TotalComments = totalComments;
            summary.TotalAttachments = totalAttachments;
            
            foreach(var itemGroup in checkListItems.GroupBy(i => i.CheckListId))
            {
                var items = itemGroup.ToList();
                summary.CheckLists.Add(new CheckListSummary()
                {
                    CheckListId = itemGroup.Key,
                    TotalItems = items.Count,
                    TotalItemsChecked = items.Where(i => i.IsChecked).Count(),
                });
            }

            return summary;
        }

        public async Task<Board> LoadBoardOld(Guid boardId, int maxCardsPerList, Guid? sprintId = null)
        {
            try
            {
                //return await LoadBoardNew(boardId, maxCardsPerList, sprintId);

                var query = "[dbo].[LoadBoard]";
                var parameters = new DynamicParameters();
                parameters.Add("@BoardId", boardId, DbType.Guid, ParameterDirection.Input);
                parameters.Add("@cardsPerList", maxCardsPerList, DbType.Int32, ParameterDirection.Input);

                if (sprintId.HasValue)
                {
                    parameters.Add("@sprintId", sprintId, DbType.Guid, ParameterDirection.Input);
                }

                using (var connection = new SqlConnection(_connectionString))
                using (var multi = await connection.QueryMultipleAsync(query, parameters, commandType: CommandType.StoredProcedure))
                {
                    var board = await multi.ReadSingleOrDefaultAsync<Board>();

                    if (board != null)
                    {
                        var boardLists = (await multi.ReadAsync<BoardList>()).ToList();

                        var cards = (await multi.ReadAsync<Card>()).ToList();

                        var labels = (await multi.ReadAsync<Label>()).ToList();
                        var cardLabels = (await multi.ReadAsync<CardLabel>()).ToList();
                        var attachments = (await multi.ReadAsync<Attachment>()).ToList();
                        var userCards = (await multi.ReadAsync<UserCard>()).ToList();
                        var checkLists = (await multi.ReadAsync<CheckList>()).ToList();
                        var checkListItems = (await multi.ReadAsync<CheckListItem>()).ToList();
                        var issueTypes = (await multi.ReadAsync<IssueType>()).ToList();
                        var sprints = (await multi.ReadAsync<Sprint>()).ToList();
                        var comments = (await multi.ReadAsync<CardCommentsDto>()).ToList();
                        var children = (await multi.ReadAsync<CardChildrenDto>()).ToList();
                        var links = (await multi.ReadAsync<Link>()).ToList();

                        foreach (var cardLabel in cardLabels)
                        {
                            cardLabel.Label = labels.FirstOrDefault(l => l.Id == cardLabel.LabelId);
                        }

                        foreach (var checkList in checkLists)
                        {
                            checkList.Items = checkListItems.Where(i => i.CheckListId == checkList.Id).ToList();
                        }

                        foreach (var card in cards)
                        {
                            var cardsLabels = cardLabels.Where(cl => cl.CardId == card.Id).ToList();
                            if (cardsLabels.Any())
                            {
                                card.Labels = new List<CardLabel>();
                                foreach (var cardLabel in cardsLabels)
                                {
                                    card.Labels.Add(cardLabel);
                                }
                            }

                            if (attachments.Any(a => a.CardId == card.Id))
                            {
                                card.Attachments = new List<Attachment>();
                                card.Attachments.AddRange(attachments.Where(a => a.CardId == card.Id));
                            }

                            if (userCards.Any(uc => uc.CardId == card.Id))
                            {
                                card.Members = new List<UserCard>();
                                card.Members.AddRange(userCards.Where(uc => uc.CardId == card.Id));
                            }

                            card.CheckLists = checkLists.Where(l => l.CardId == card.Id).ToList();

                            if (card.IssueTypeId.HasValue)
                            {
                                card.IssueType = issueTypes
                                    .FirstOrDefault(it => it.Id == card.IssueTypeId);
                            }

                            if (card.SprintId.HasValue && sprints.Any())
                            {
                                card.Sprint = sprints.FirstOrDefault(s => s.Id == card.SprintId);
                            }

                            // used for presentation only
                            var totalComments = comments.FirstOrDefault(c => c.CardId == card.Id);
                            if (totalComments != null)
                            {
                                card.Comments = new List<Comment>();

                                for (var i = 0; i < totalComments.TotalComments; i++)
                                {
                                    card.Comments.Add(new Comment());
                                }
                            }

                            var cardChildren = children.FirstOrDefault(c => c.CardId == card.Id);
                            if (cardChildren != null)
                            {
                                card.Children = new List<Card>();

                                for (var i = 0; i < cardChildren.TotalChildren; i++)
                                {
                                    card.Children.Add(new Card());
                                }
                            }

                            card.Links = links.Where(l => l.SourceCardId == card.Id).ToList();
                        }

                        board.Lists = new List<BoardList>();
                        foreach (var boardList in boardLists)
                        {
                            boardList.Cards = new List<Card>();
                            boardList.Cards.AddRange(cards.Where(card => card.BoardListId == boardList.Id));

                            board.Lists.Add(boardList);
                        }

                        board.IssueTypes = issueTypes;

                        return board;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return new Board();
        }

        public async Task<List<Card>> LoadBoardListCardsOld(Guid boardId, Guid boardListId, int page, int maxCardsPerList, Guid? sprintId = null)
        {
            try
            {
                var query = "[dbo].[LoadBoardListCards]";
                var parameters = new DynamicParameters();
                parameters.Add("@BoardId", boardId, DbType.Guid, ParameterDirection.Input);
                parameters.Add("@BoardListId", boardListId, DbType.Guid, ParameterDirection.Input);
                parameters.Add("@page", page, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@cardsPerList", maxCardsPerList, DbType.Int32, ParameterDirection.Input);

                if (sprintId.HasValue)
                {
                    parameters.Add("@sprintId", sprintId, DbType.Guid, ParameterDirection.Input);
                }

                using (var connection = new SqlConnection(_connectionString))
                using (var multi = await connection.QueryMultipleAsync(query, parameters, commandType: CommandType.StoredProcedure))
                {
                    var cards = (await multi.ReadAsync<Card>()).ToList();

                    var labels = (await multi.ReadAsync<Label>()).ToList();
                    var cardLabels = (await multi.ReadAsync<CardLabel>()).ToList();
                    var attachments = (await multi.ReadAsync<Attachment>()).ToList();
                    var userCards = (await multi.ReadAsync<UserCard>()).ToList();
                    var checkLists = (await multi.ReadAsync<CheckList>()).ToList();
                    var checkListItems = (await multi.ReadAsync<CheckListItem>()).ToList();
                    var issueTypes = (await multi.ReadAsync<IssueType>()).ToList();
                    var sprints = (await multi.ReadAsync<Sprint>()).ToList();
                    var comments = (await multi.ReadAsync<CardCommentsDto>()).ToList();
                    var children = (await multi.ReadAsync<CardChildrenDto>()).ToList();
                    var links = (await multi.ReadAsync<Link>()).ToList();

                    foreach (var cardLabel in cardLabels)
                    {
                        cardLabel.Label = labels.FirstOrDefault(l => l.Id == cardLabel.LabelId);
                    }

                    foreach (var checkList in checkLists)
                    {
                        checkList.Items = checkListItems.Where(i => i.CheckListId == checkList.Id).ToList();
                    }

                    foreach (var card in cards)
                    {
                        var cardsLabels = cardLabels.Where(cl => cl.CardId == card.Id).ToList();
                        if (cardsLabels.Any())
                        {
                            card.Labels = new List<CardLabel>();
                            foreach (var cardLabel in cardsLabels)
                            {
                                card.Labels.Add(cardLabel);
                            }
                        }

                        if (attachments.Any(a => a.CardId == card.Id))
                        {
                            card.Attachments = new List<Attachment>();
                            card.Attachments.AddRange(attachments.Where(a => a.CardId == card.Id));
                        }

                        if (userCards.Any(uc => uc.CardId == card.Id))
                        {
                            card.Members = new List<UserCard>();
                            card.Members.AddRange(userCards.Where(uc => uc.CardId == card.Id));
                        }

                        card.CheckLists = checkLists.Where(l => l.CardId == card.Id).ToList();

                        if (card.IssueTypeId.HasValue)
                        {
                            card.IssueType = issueTypes
                                .FirstOrDefault(it => it.Id == card.IssueTypeId);
                        }

                        // used for presentation only
                        var totalComments = comments.FirstOrDefault(c => c.CardId == card.Id);
                        if (totalComments != null)
                        {
                            card.Comments = new List<Comment>();

                            for (var i = 0; i < totalComments.TotalComments; i++)
                            {
                                card.Comments.Add(new Comment());
                            }
                        }

                        card.Links = links.Where(l => l.SourceCardId == card.Id).ToList();
                    }

                    return cards;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Card>> LoadBoardListCards(Board board, Guid boardListId, int page,
            int maxCardsPerList, Guid? sprintId = null)
        {
            var cards = await GetBoardListCards(board, new List<Guid> { boardListId }, page, maxCardsPerList, sprintId);

            foreach (var boardList in board.Lists)
            {
                boardList.Cards = [.. cards.Where(card => card.BoardListId == boardList.Id)];
            }

            return cards;
        }
        private async Task<List<Card>> GetBoardListCards(Board board, List<Guid> boardListIds, int page, 
            int maxCardsPerList, Guid? sprintId = null)
        {
            // cards
            var cardFilter = new CardItemFilterSpecification()
            {
                BoardId = board.Id,
                Statuses = new List<CardStatus> { CardStatus.Active },
                BoardLists = boardListIds,
                SkipChildren = true,
                IncludeCheckLists = true,
            };

            if (sprintId.HasValue)
            {
                cardFilter.Sprints = new List<Guid> { sprintId.Value };
            }

            cardFilter.Build();

            var cards = await _cardRepository
                .Entities.AsNoTracking()
                .Specify(cardFilter)
                .Skip((page - 1) * maxCardsPerList).Take(maxCardsPerList)
                .ToListAsync();

            foreach (var card in cards)
            {
                var cardSummary = await _cacheService
                    .GetOrCreateAsync(CacheKeys.CardSummary(card.Id),
                    async () => await GetCardSummary(card),
                    TimeSpan.FromMinutes(2));

                #region labels

                var cardLabels = cardSummary.Labels;
                card.Labels = new List<CardLabel>(cardLabels.Count);

                for (var i = 0; i < cardLabels.Count; i++)
                {
                    var labelId = cardLabels[i];
                    var label = board.Labels.FirstOrDefault(l => l.Id == labelId);
                    card.Labels.Add(new CardLabel()
                    {
                        LabelId = labelId,
                        Label = label,
                    });
                }

                foreach (var cardLabel in card.Labels)
                {
                    cardLabel.Label = board.Labels.FirstOrDefault(l => l.Id == cardLabel.LabelId);
                }

                #endregion

                #region checklists

                foreach (var checkList in card.CheckLists)
                {
                    var checkListSummary = cardSummary.CheckLists.FirstOrDefault(l => l.CheckListId == checkList.Id);
                    checkList.Items = new List<CheckListItem>();

                    if (checkListSummary != null)
                    {
                        for (var i = 0; i < checkListSummary.TotalItems; i++)
                        {
                            checkList.Items.Add(new CheckListItem()
                            {
                                CheckListId = checkList.Id
                            });
                        }
                        for (var i = 0; i < checkListSummary.TotalItemsChecked; i++)
                        {
                            checkList.Items[i].IsChecked = true;
                        }
                    }
                }

                #endregion

                #region children

                var totalChildren = cardSummary.TotalChildren;
                card.Children = new List<Card>(totalChildren);

                for (var i = 0; i < totalChildren; i++)
                {
                    card.Children.Add(new Card());
                }

                #endregion

                #region user cards

                var cardMembers = cardSummary.Members;
                card.Members = new List<UserCard>();
                for (var i = 0; i < cardMembers.Count; i++)
                {
                    card.Members.Add(new UserCard()
                    {
                        UserId = cardMembers[i]
                    });
                }

                #endregion

                #region Links

                var totalLinks = cardSummary.TotalLinks;
                card.Links = new List<Link>(totalLinks);

                for (var i = 0; i < totalLinks; i++)
                {
                    card.Links.Add(new Link());
                }

                #endregion

                #region Comments

                var totalComments = cardSummary.TotalComments;
                card.Comments = new List<Comment>(totalComments);

                for (var i = 0; i < totalComments; i++)
                {
                    card.Comments.Add(new Comment());
                }

                #endregion

                #region attachments

                var totalAttachments = cardSummary.TotalAttachments;
                card.Attachments = new List<Attachment>(totalAttachments);

                for (var i = 0; i < totalAttachments; i++)
                {
                    card.Attachments.Add(new Attachment());
                }

                #endregion

                if (card.IssueTypeId.HasValue)
                {
                    card.IssueType = board.IssueTypes
                        .FirstOrDefault(it => it.Id == card.IssueTypeId);
                }
            }

            foreach (var boardList in board.Lists)
            {
                boardList.Cards = [.. cards.Where(card => card.BoardListId == boardList.Id)];
            }

            return cards;
        }

        public async Task<List<SprintSummary>> GetSprintsSummaries(Guid boardId, string term,
            int pageNumber, int pageSize, SprintStatus? status)
        {
            var query = _sprintRepository.Entities
                .Include(s => s.Cards
                    .Where(c => c.Status != CardStatus.Backlog))
                .Select(sprint => new SprintSummary()
                {
                    Id = sprint.Id,
                    BoardId = sprint.BoardId,
                    Name = sprint.Name,
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate,
                    Status = sprint.Status,
                    Goal = sprint.Goal,
                    TotalCards = sprint.Cards.Count,
                    StoryPoints = sprint.Cards.Sum(c => c.StoryPoints) ?? 0
                })
                .Where(sprint =>
                    (sprint.BoardId == boardId) &&
                    (status == null ? true : sprint.Status == status.Value) &&
                    (string.IsNullOrEmpty(term) ? true : sprint.Name.ToLower().Contains(term)));

            var result = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize).ToListAsync();

            return result;
        }
    }

    public class SprintStatusOrder
    {
        public static Dictionary<SprintStatus, int> Orders =
        new Dictionary<SprintStatus, int>() {
          {SprintStatus.Idle, 2},
          {SprintStatus.Active, 1},
          {SprintStatus.Completed, 2},
      };
    }
}
