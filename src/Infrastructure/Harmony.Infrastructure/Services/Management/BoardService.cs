using Bogus.DataSets;
using Dapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Domain.Entities;
using Harmony.Infrastructure.Repositories;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Migrations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Infrastructure.Services.Management
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly ICardRepository _cardRepository;
        private readonly string _connectionString;

        public BoardService(IConfiguration configuration, IBoardRepository boardRepository,
            IUserBoardRepository userBoardRepository, IUserWorkspaceRepository userWorkspaceRepository,
            ISprintRepository sprintRepository, IBoardListRepository boardListRepository,
            IIssueTypeRepository issueTypeRepository,
            ICardRepository cardRepository)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _boardRepository = boardRepository;
            _userBoardRepository = userBoardRepository;
            _userWorkspaceRepository = userWorkspaceRepository;
            _sprintRepository = sprintRepository;
            _boardListRepository = boardListRepository;
            _issueTypeRepository = issueTypeRepository;
            _cardRepository = cardRepository;
        }

        public async Task<bool> HasUserAccessToBoard(string userId, Guid boardId)
        {
            var board = await _boardRepository.GetAsync(boardId);
            var hasAccess = false;

            switch (board.Visibility)
            {
                case Domain.Enums.BoardVisibility.Private:
                    var userBoard = await _userBoardRepository.GetBoardAccessMember(boardId, userId);
                    hasAccess = userBoard != null;
                    break;
                case Domain.Enums.BoardVisibility.Workspace:
                    var userWorkspace = await _userWorkspaceRepository.GetUserWorkspace(board.WorkspaceId, userId);
                    hasAccess = userWorkspace != null;
                    break;
                case Domain.Enums.BoardVisibility.Public:
                    hasAccess = true;
                    break;
            }

            return hasAccess;
        }

        public async Task<List<Board>> GetUserBoards(Guid workspaceId, string userId)
        {
            var userWorkspaceBoardsQuery = _userWorkspaceRepository
                    .GetUserWorkspaceBoardsQuery(workspaceId, userId);

            var userBoardsQuery = _userBoardRepository.GetUserBoardsQuery(workspaceId, userId);

            var result = await userWorkspaceBoardsQuery.Union(userBoardsQuery).Distinct().ToListAsync();

            return result;
        }

        public async Task<Board> LoadBoard(Guid boardId, int maxCardsPerList)
        {
            try
            {
                var query = "[dbo].[LoadBoard]";
                var parameters = new DynamicParameters();
                parameters.Add("@BoardId", boardId, DbType.Guid, ParameterDirection.Input);
                parameters.Add("@cardsPerList", maxCardsPerList, DbType.Int32, ParameterDirection.Input);

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

                            if(card.IssueTypeId.HasValue)
                            {
                                card.IssueType = issueTypes
                                    .FirstOrDefault(it => it.Id == card.IssueTypeId);
                            }
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

        public async Task<List<Card>> LoadBoardListCards(Guid boardId, Guid boardListId, int page, int maxCardsPerList)
        {
            try
            {
                var query = "[dbo].[LoadBoardListCards]";
                var parameters = new DynamicParameters();
                parameters.Add("@BoardId", boardId, DbType.Guid, ParameterDirection.Input);
                parameters.Add("@BoardListId", boardListId, DbType.Guid, ParameterDirection.Input);
                parameters.Add("@page", page, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@cardsPerList", maxCardsPerList, DbType.Int32, ParameterDirection.Input);

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
                        }

                    return cards;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<GetSprintCardResponse>> SearchSprints(Guid boardId, string term, int pageNumber, int pageSize)
        {
            IQueryable<GetSprintCardResponse> query = null;

            query = from sprint in _sprintRepository.Entities
                    join board in _boardRepository.Entities
                        on sprint.BoardId equals board.Id
                    join card in _cardRepository.Entities
                        on sprint.Id equals card.SprintId into grouping
                    from p in grouping.DefaultIfEmpty()
                    join issueType in _issueTypeRepository.Entities
                        on p.IssueTypeId equals issueType.Id into issueGrouping
                    from issue in issueGrouping.DefaultIfEmpty()
                    where (board.Id == boardId
                        && p.Status != Domain.Enums.CardStatus.Backlog &&
                        (string.IsNullOrEmpty(term) ? true : sprint.Name.Contains(term)))
                    select new GetSprintCardResponse()
                    {
                        CardId = (Guid?)p.Id,
                        CardTitle = p != null ? p.Title : null,
                        CardStartDate = p != null ? p.StartDate : null,
                        CardDueDate = p != null ? p.DueDate : null,
                        CardSerialKey = p != null ? $"{board.Key}-{p.SerialNumber}" : null,
                        Sprint = sprint.Name,
                        SprintStatus = sprint.Status,
                        SprintStartDate = sprint.StartDate,
                        SprintEndDate = sprint.EndDate,
                        SprintId = sprint.Id,
                        CardIssueType = new IssueTypeDto()
                        {
                            Id = issue != null ? issue.Id : Guid.Empty,
                            Summary = issue != null ? issue.Summary : null
                        }
                    };


            var result = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize).ToListAsync();

            return result;
        }
    }
}
