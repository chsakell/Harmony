using Azure.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.Repositories;
using Harmony.Shared.Wrapper;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services.Management
{
    public class CardService : ICardService
	{
		private readonly ICardRepository _cardRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly IBoardListRepository _boardListRepository;

        public CardService(ICardRepository cardRepository, 
			IBoardRepository boardRepository,
			IIssueTypeRepository issueTypeRepository,
			IBoardListRepository boardListRepository)
        {
			_cardRepository = cardRepository;
            _boardRepository = boardRepository;
            _issueTypeRepository = issueTypeRepository;
            _boardListRepository = boardListRepository;
        }

        public async Task<bool> PositionCard(Card card, Guid? newListId, short newPosition, CardStatus status)
		{
			if(card.BoardListId == newListId)
			{
				return await SwapCards(card, newPosition, status);
			}
            else
            {
				return await ReorderOtherCardsAndMove(card, newListId.Value, newPosition);
            }
        }

		public async Task<IResult<List<Card>>> MoveCardsToSprint(List<Guid> cardsToMove, Guid sprintId, Guid boardListId)
		{
			var cards = await _cardRepository
				.Entities.Where(card => cardsToMove.Contains(card.Id) 
					&& card.Status == Domain.Enums.CardStatus.Backlog)
				.ToListAsync();

			if(cards.Any())
			{
                // Get the last index in the board list id
                var totalCards = await _cardRepository.CountCards(boardListId);

				foreach(var card in cards.OrderBy(c => c.Position))
				{
					card.BoardListId = boardListId;
					card.Position = (short)totalCards++;
					card.SprintId = sprintId;
					card.Status = Domain.Enums.CardStatus.Active;
				}

				var result = await _cardRepository.UpdateRange(cards);

				if(result > 0)
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
                    card.Status = Domain.Enums.CardStatus.Backlog;
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
						&& card.Status == Domain.Enums.CardStatus.Backlog &&
						(string.IsNullOrEmpty(term) ? true : card.Title.Contains(term)))
					orderby card.Position
					select new GetBacklogItemResponse()
					{
						Id = card.Id,
						Title = card.Title,
						StartDate = card.StartDate,
						DueDate = card.DueDate,
                        SerialKey = $"{board.Key}-{card.SerialNumber}",
						Position = card.Position,
						IssueType = new Application.DTO.IssueTypeDto()
						{
							Id = issueType.Id,
							Summary = issueType.Summary
						}
					};


			var result = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize).ToListAsync();

            return result;
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

		private async Task ReorderCards(Guid currentListId, short position, bool belongsToSameList)
		{
			var offset = belongsToSameList ? -1 : 1;

			// Find all items in curerrent list with position > current position and decline by one
			var cardsInCurrentListToReorder = belongsToSameList ? 
				await _cardRepository.GetCardsInPositionGreaterThan(currentListId, position) :
				await _cardRepository.GetCardsInPositionGreaterOrEqualThan(currentListId, position);

			foreach (var cardToReorder in cardsInCurrentListToReorder)
			{
				cardToReorder.Position = (short)(cardToReorder.Position + offset);
				_cardRepository.UpdateEntry(cardToReorder);
			}
		}

		private async Task<bool> SwapCards(Card card, short newPosition, CardStatus status)
		{
			var currentPosition = card.Position;

			// needs swapping
			if (currentPosition != newPosition)
			{
				var currentCardInNewPosition = await _cardRepository
						.GetByPosition(card.BoardListId, newPosition, status);
				
				if (currentCardInNewPosition != null)
				{
					currentCardInNewPosition.Position = currentPosition;
					_cardRepository.UpdateEntry(currentCardInNewPosition);
				}

				card.Position = newPosition;

				// commit all the changes
				var dbResult = await _cardRepository.Update(card);

				return dbResult > 0;
			}
			else
			{
				return true;
			}
		}
	}
}
