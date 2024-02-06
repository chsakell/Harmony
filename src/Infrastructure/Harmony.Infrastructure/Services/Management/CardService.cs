using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Features.Boards.Queries.GetArchivedItems;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetIssueTypes;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services.Management
{
    public class CardService : ICardService
	{
		private readonly ICardRepository _cardRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly IBoardListRepository _boardListRepository;
        private readonly IMediator _mediator;

        public CardService(ICardRepository cardRepository, 
			IBoardRepository boardRepository,
			IIssueTypeRepository issueTypeRepository,
			IBoardListRepository boardListRepository,
            IMediator mediator)
        {
			_cardRepository = cardRepository;
            _boardRepository = boardRepository;
            _issueTypeRepository = issueTypeRepository;
            _boardListRepository = boardListRepository;
            _mediator = mediator;
        }

        public async Task<bool> PositionCard(Card card, Guid? newListId, short newPosition, CardStatus status)
		{
			if(card.BoardListId == newListId)
			{
				if(card.BoardList == null && card.Status == CardStatus.Backlog)
				{
                    await ReorderCardsInBacklog(card.Id, card.Position, newPosition);
                }
				else if (card.BoardListId.HasValue)
				{
                    await ReorderCardsInSameList(card.BoardListId.Value, card.Position, newPosition);
                }
				
                card.Position = newPosition;

                // commit all the changes
                var dbResult = await _cardRepository.Update(card);

                return dbResult > 0;
            }
            else
            {
				return await ReorderOtherCardsAndMove(card, newListId.Value, newPosition);
            }
        }

		public async Task<IResult<List<Card>>> MoveCardsToSprint(Guid boardId, List<Guid> cardsToMove, Guid sprintId, Guid boardListId)
		{
			var cards = await _cardRepository
                .Entities.IgnoreQueryFilters()
                .Where(card => cardsToMove.Contains(card.Id))
				.ToListAsync();

			if(cards.Any())
			{
                // Get the last index in the board list id
                var currentMaxPosition = await _cardRepository.GetMaxActivePosition(boardListId);

                var issueTypesResult = await _mediator
                            .Send(new GetIssueTypesQuery(boardId));

                foreach (var card in cards.OrderBy(c => c.Position))
				{
					card.BoardListId = boardListId;
					card.Position = ++currentMaxPosition;
					card.SprintId = sprintId;
					card.Status = CardStatus.Active;

                    if (card.ParentCardId.HasValue)
                    {
                        card.ParentCardId = null;

                        if (issueTypesResult.Succeeded && issueTypesResult.Data.Any())
                        {
                            card.IssueTypeId = issueTypesResult.Data.First().Id;
                        }
                    }
                }

				var result = await _cardRepository.UpdateRange(cards);

				if(result > 0)
				{
					return await Result<List<Card>>.SuccessAsync(cards);
                }
            }

			return await Result<List<Card>>.FailAsync("Failed to move cards");
		}

        public async Task<IResult<List<Card>>> ReactivateCards(Guid boardId, List<Guid> cardIds, Guid boardListId)
        {
            var cards = await _cardRepository
                .Entities.IgnoreQueryFilters()
                .Where(card => cardIds.Contains(card.Id))
                .ToListAsync();

            if (cards.Any())
            {
                // Get the last index in the board list id
                var totalCards = await _cardRepository.CountActiveCards(boardListId);
                
                var issueTypesResult = await _mediator
                            .Send(new GetIssueTypesQuery(boardId));

                foreach (var card in cards.OrderBy(c => c.Position))
                {
                    card.BoardListId = boardListId;
                    card.Position = (short)totalCards++;
                    
                    card.Status = CardStatus.Active;

                    if(card.ParentCardId.HasValue)
                    {
                        card.ParentCardId = null;

                        if (issueTypesResult.Succeeded && issueTypesResult.Data.Any())
                        {
                            card.IssueTypeId = issueTypesResult.Data.First().Id;
                        }
                    }
                }

                var result = await _cardRepository.UpdateRange(cards);

                if (result > 0)
                {
                    return await Result<List<Card>>.SuccessAsync(cards);
                }
            }

            return await Result<List<Card>>.FailAsync("Failed to move cards");
        }

        public async Task<IResult<List<Card>>> MoveCardsToBacklog(Guid boardId, List<Guid> cardsToMove)
        {
            var cards = await _cardRepository
                .Entities.Where(card => cardsToMove.Contains(card.Id))
                .ToListAsync();

            // Get the last index in the board list id
            var nextPosition = await _cardRepository.GetNextBacklogPosition(boardId);

            if (cards.Any())
            {
                foreach (var card in cards.OrderBy(c => c.Position))
                {
                    card.BoardListId = null;
                    card.Position = (short)nextPosition++;
                    card.SprintId = null;
                    card.Status = CardStatus.Backlog;
                    card.DateCompleted = null;
                }

                var result = await _cardRepository.UpdateRange(cards);

                if (result > 0)
                {
                    return await Result<List<Card>>.SuccessAsync(cards);
                }
            }

            return await Result<List<Card>>.FailAsync("Failed to move cards");
        }

        public async Task<List<GetBacklogItemResponse>> SearchBacklog(Guid boardId, string term, int pageNumber, int pageSize)
        {
            IQueryable<GetBacklogItemResponse> query = null;

			query = from card in _cardRepository.Entities
					join issueType in _issueTypeRepository.Entities
						on card.IssueTypeId equals issueType.Id
					join board in _boardRepository.Entities
						on issueType.BoardId equals board.Id
					where (board.Id == boardId
						&& card.Status == CardStatus.Backlog &&
						(string.IsNullOrEmpty(term) ? true : card.Title.Contains(term)))
					orderby card.Position
					select new GetBacklogItemResponse()
					{
						Id = card.Id,
						Title = card.Title,
						StartDate = card.StartDate,
						DueDate = card.DueDate,
                        BoardKey = $"{board.Key}",
                        SerialKey = $"{board.Key}-{card.SerialNumber}",
						Position = card.Position,
						IssueType = new Application.DTO.IssueTypeDto()
						{
							Id = issueType.Id,
							Summary = issueType.Summary
						},
                        StoryPoints = card.StoryPoints
					};


			var result = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize).ToListAsync();

            return result;
        }

		public async Task<bool> CardCompleted(Guid cardId)
		{
			return (await _cardRepository.Entities.AsNoTracking()
				.Include(c => c.BoardList)
				.CountAsync(c => c.Id == cardId && 
					c.BoardList.CardStatus == BoardListCardStatus.DONE)) > 0;
		}

        private async Task<bool> ReorderOtherCardsAndMove(Card card, Guid newListId, short newPosition)
		{
			var currentPosition = card.Position;

			await ReorderCards(card.BoardListId.Value, currentPosition, true);

			return await MoveCard(card, newListId, newPosition);
		}

		private async Task<bool> MoveCard(Card card, Guid newListId, short newPosition)
		{
			// reorder cards in the new list
			await ReorderCards(newListId, newPosition, false);

			card.BoardListId = newListId;
			card.Position = newPosition;

			// commit all the changes
			var dbResult = await _cardRepository.Update(card);

			return dbResult > 0;
		}

        private async Task ReorderCardsInSameList(Guid listId, short previousPosition, short newPosition)
        {
			var offSet = newPosition < previousPosition ? 1 : -1;

			List<Card> cardsToReOrder = null;

			if(offSet == 1)
			{
                cardsToReOrder = await _cardRepository.Entities
					.Where(c => c.Position >= newPosition 
					&& c.Position < previousPosition && c.BoardListId == listId).ToListAsync();
            }
			else
			{
                cardsToReOrder = await _cardRepository.Entities
                    .Where(c => c.Position <= newPosition 
					&& c.Position > previousPosition && c.BoardListId == listId).ToListAsync();
            }

            foreach (var cardToReorder in cardsToReOrder)
            {
                cardToReorder.Position = (short)(cardToReorder.Position + offSet);
                _cardRepository.UpdateEntry(cardToReorder);
            }
        }

        public async Task ReorderCardsAfterArchive(Guid listId, short archivedCardPositionn)
        {
            List<Card> cardsToReOrder = await _cardRepository.Entities
                    .Where(c => c.Position > archivedCardPositionn
                    && c.Status == CardStatus.Active && c.BoardListId == listId).ToListAsync();


            foreach (var cardToReorder in cardsToReOrder)
            {
                cardToReorder.Position = (short)(cardToReorder.Position - 1);
                _cardRepository.UpdateEntry(cardToReorder);
            }
        }

        private async Task ReorderCardsInBacklog(Guid cardId, short previousPosition, short newPosition)
        {
            var offSet = newPosition < previousPosition ? 1 : -1;

            List<Card>? cardsToReOrder = null;

            if (offSet == 1)
            {
                cardsToReOrder = await (from card in _cardRepository.Entities
                                 join issueType in _issueTypeRepository.Entities
                                     on card.IssueTypeId equals issueType.Id
                                 join board in _boardRepository.Entities
                                     on issueType.BoardId equals board.Id
                                 where (board.Id == issueType.BoardId 
                                        && card.Status == CardStatus.Backlog
                                        && card.Position >= newPosition
                                        && card.Position < previousPosition)
                                 orderby card.Position
                                 select card).ToListAsync();
            }
            else
            {
                cardsToReOrder = await (from card in _cardRepository.Entities
                       join issueType in _issueTypeRepository.Entities
                           on card.IssueTypeId equals issueType.Id
                       join board in _boardRepository.Entities
                           on issueType.BoardId equals board.Id
                       where (board.Id == issueType.BoardId
                              && card.Status == CardStatus.Backlog
                              && card.Position <= newPosition
                              && card.Position > previousPosition)
                       orderby card.Position
                       select card).ToListAsync();
            }

            foreach (var cardToReorder in cardsToReOrder)
            {
                cardToReorder.Position = (short)(cardToReorder.Position + offSet);
                _cardRepository.UpdateEntry(cardToReorder);
            }
        }

        private async Task ReorderCards(Guid currentListId, short position, bool belongsToSameList)
		{
			var offset = belongsToSameList ? -1 : 1;

			// Find all items in current list with position > current position and decline by one
			var cardsInCurrentListToReorder = belongsToSameList ? 
				await _cardRepository.GetCardsInPositionGreaterThan(currentListId, position) :
				await _cardRepository.GetCardsInPositionGreaterOrEqualThan(currentListId, position);

			foreach (var cardToReorder in cardsInCurrentListToReorder)
			{
				cardToReorder.Position = (short)(cardToReorder.Position + offset);
				_cardRepository.UpdateEntry(cardToReorder);
			}
		}

        public async Task<List<GetArchivedItemResponse>> SearchArchivedItems(Guid boardId, string term, int pageNumber, int pageSize)
        {
            IQueryable<GetArchivedItemResponse> query = null;

            query = from card in _cardRepository.Entities.IgnoreQueryFilters()
                    join issueType in _issueTypeRepository.Entities
                        on card.IssueTypeId equals issueType.Id
                    join board in _boardRepository.Entities
                        on issueType.BoardId equals board.Id
                    where (board.Id == boardId
                        && card.Status == CardStatus.Archived &&
                        (string.IsNullOrEmpty(term) ? true : card.Title.Contains(term)))
                    orderby card.Position
                    select new GetArchivedItemResponse()
                    {
                        Id = card.Id,
                        Title = card.Title,
                        StartDate = card.StartDate,
                        DueDate = card.DueDate,
                        BoardKey = $"{board.Key}",
                        SerialKey = $"{board.Key}-{card.SerialNumber}",
                        Position = card.Position,
                        IssueType = new Application.DTO.IssueTypeDto()
                        {
                            Id = issueType.Id,
                            Summary = issueType.Summary
                        },
                        StoryPoints = card.StoryPoints,
                        BoardType = board.Type
                    };


            var result = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize).ToListAsync();

            return result;
        }
    }
}
