using Azure.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Entities;
using Harmony.Persistence.Migrations;

namespace Harmony.Infrastructure.Services.Management
{
	public class CardService : ICardService
	{
		private readonly ICardRepository _cardRepository;

		public CardService(ICardRepository cardRepository)
        {
			_cardRepository = cardRepository;
		}

        public async Task<bool> PositionCard(Card card, Guid newListId, byte newPosition)
		{
			if(card.BoardListId == newListId)
			{
				return await SwapCards(card, newPosition);
			}
            else
            {
				return await ReorderOtherCardsAndMove(card, newListId, newPosition);
            }
        }

		private async Task<bool> ReorderOtherCardsAndMove(Card card, Guid newListId, byte newPosition)
		{
			var currentPosition = card.Position;

			await ReorderCards(card.BoardListId, currentPosition, true);

			return await MoveCard(card, newListId, newPosition);
		}

		private async Task<bool> MoveCard(Card card, Guid newListId, byte newPosition)
		{
			// reorder cards in the new list
			await ReorderCards(newListId, newPosition, false);

			card.BoardListId = newListId;
			card.Position = newPosition;

			// commit all the changes
			var dbResult = await _cardRepository.Update(card);

			return dbResult > 0;
		}

		private async Task ReorderCards(Guid currentListId, byte position, bool belongsToSameList)
		{
			var offset = belongsToSameList ? -1 : 1;

			// Find all items in curerrent list with position > current position and decline by one
			var cardsInCurrentListToReorder = belongsToSameList ? 
				await _cardRepository.GetCardsInPositionGreaterThan(currentListId, position) :
				await _cardRepository.GetCardsInPositionGreaterOrEqualThan(currentListId, position);

			foreach (var cardToReorder in cardsInCurrentListToReorder)
			{
				cardToReorder.Position = (byte)(cardToReorder.Position + offset);
				_cardRepository.UpdateEntry(cardToReorder);
			}
		}

		private async Task<bool> SwapCards(Card card, byte newPosition)
		{
			var currentPosition = card.Position;

			// needs swapping
			if (currentPosition != newPosition)
			{
				var currentCardInNewPosition = await _cardRepository.GetByPosition(card.BoardListId, newPosition);
				
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
