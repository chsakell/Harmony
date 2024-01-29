using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Specifications.Cards;
using Microsoft.EntityFrameworkCore;
using Harmony.Application.Extensions;
using Harmony.Api.Protos;

namespace Harmony.Api.Services.gRPC
{
    public class CardService : Protos.CardService.CardServiceBase
    {
        private readonly ILogger<CardService> _logger;
        private readonly ICardRepository _cardRepository;

        public CardService(ILogger<CardService> logger, ICardRepository cardRepository)
        {
            _logger = logger;
            _cardRepository = cardRepository;
        }

        public override async Task<CardResponse> GetCard(CardFilterRequest request,
            ServerCallContext context)
        {
            var includes = new CardIncludes()
            {
                Attachments = request.Attachments,
                Board = request.Board,
                BoardList = request.BoardList,
                IssueType = request.IssueType,
                Children = request.Children,
                Members = request.Members
            };

            var cardId = Guid.Parse(request.CardId);

            var filter = new CardFilterSpecification(cardId, includes);

            var card = await _cardRepository
                .Entities.IgnoreQueryFilters().Specify(filter)
                .FirstOrDefaultAsync();

            return MapToProto(card);
        }

        public override async Task<MoveCardToListResponse> MoveCardToList(Protos.MoveCardToListRequest request, ServerCallContext context)
        {
            var includes = new CardIncludes() { };
            var cardId = Guid.Parse(request.CardId);

            var filter = new CardFilterSpecification(cardId, includes);
            var moveToBoardListId = Guid.Parse(request.BoardListId);

            var card = await _cardRepository
                .Entities.IgnoreQueryFilters().Specify(filter)
                .FirstOrDefaultAsync();

            card.BoardListId = moveToBoardListId;

            // make this the last card in the list
            var totalCards = await _cardRepository.CountCards(moveToBoardListId);
            card.Position = (short)totalCards;

            var updateResult = await _cardRepository.Update(card);

            var result = new Protos.MoveCardToListResponse()
            {
                Success = updateResult > 0,
                NewPosition = card.Position
            };

            return result;
        }

        private CardResponse MapToProto(Domain.Entities.Card card)
        {
            if (card == null)
            {
                return new CardResponse()
                {
                    Found = false
                };
            }

            return new CardResponse()
            {
                Found = true,
                Card = MapToProtoCard(card)
            };
        }

        private Protos.Card MapToProtoCard(Domain.Entities.Card card)
        {

            var protoCard = new Protos.Card()
            {
                CardId = card.Id.ToString(),
                Title = card.Title,
                Position = card.Position,
                BoardListId = card.BoardListId?.ToString(),
                BoardTitle = card.BoardList?.Board?.Title ?? card.IssueType?.Board?.Title,
                DueDateReminderType = card.DueDateReminderType.HasValue ? (int)card.DueDateReminderType.Value : null,
                IsCompleted = card?.BoardList?.CardStatus == Domain.Enums.BoardListCardStatus.DONE,
                BoardId = card.IssueType?.BoardId.ToString()
            };

            protoCard.DueDate = GetDate(card.DueDate);
            protoCard.StartDate = GetDate(card.StartDate);

            if (card.Children != null && card.Children.Any())
            {
                protoCard.Children.AddRange(card.Children.Select(MapToProtoCard));
            }

            if (card.Members != null && card.Members.Any())
            {
                protoCard.Members.AddRange(card.Members.Select(m => m.UserId));
            }

            return protoCard;
        }

        private Google.Protobuf.WellKnownTypes.Timestamp GetDate(DateTime? date)
        {
            if (date.HasValue)
            {
                var utcKind = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
                return Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(utcKind);
            }
            else
            {
                var emptyTimestamp = new Google.Protobuf.WellKnownTypes.Timestamp();
                emptyTimestamp.Nanos = 0;
                emptyTimestamp.Seconds = 0;

                return emptyTimestamp;
            }
        }
    }
}
