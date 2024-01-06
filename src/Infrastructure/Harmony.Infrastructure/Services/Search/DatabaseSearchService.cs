using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Application.Specifications.Cards;
using Harmony.Application.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services.Search
{
    public class DatabaseSearchService : ISearchService
    {
        private readonly ICardRepository _cardRepository;

        public DatabaseSearchService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }

        public async Task<List<SearchableCard>> Search(List<Guid> boards, string term)
        {
            var includes = new CardIncludes() { Board = true, IssueType = true };

            var filter = new CardFilterSpecification(term, includes);

            var cards = await _cardRepository
                .Entities.IgnoreQueryFilters()
                .Specify(filter)
                .Select(card => new SearchableCard
                {
                    CardId = card.Id.ToString(),
                    BoardId = card.BoardList != null ? card.BoardList.BoardId : card.IssueType.BoardId,
                    List = card.BoardList != null ? card.BoardList.Title : null,
                    BoardTitle = card.BoardList != null ? card.BoardList.Board.Title : card.IssueType.Board.Title,
                    IsComplete = card.BoardList != null ? card.BoardList.CardStatus == Domain.Enums.BoardListCardStatus.DONE : false,
                    Title = card.Title,
                    IssueType = card.IssueType.Summary,
                    Status = card.Status.ToString(),
                    SerialKey = $"{(card.IssueType.Board.Key)}-{card.SerialNumber}"
                })
                .ToListAsync();

            return cards;
        }

        public async Task<List<SearchableCard>> Search(List<Guid> boards, AdvancedSearchCommand query)
        {
            var includes = new CardIncludes() { Board = true, IssueType = true };

            if(query.HasAttachments)
            {
                includes.Attachments = true;
            }

            var cardFilters = new CardFilters()
            {
                Title = query.Title,
                Description = query.Description,
                HasAttachments = query.HasAttachments,
                CombineCriteria = query.CombineFilters,
                DueDate = query.DueDate,
                BoardId = query.BoardId,
                BoardListId = query.ListId
            };

            var filter = new CardFilterSpecification(cardFilters, includes);

            var cards = await _cardRepository
                .Entities.Specify(filter)
                .Select(card => new SearchableCard
                {
                    CardId = card.Id.ToString(),
                    BoardId = card.IssueType.BoardId,
                    List = card.BoardList != null ? card.BoardList.Title : null,
                    BoardTitle = card.IssueType.Board.Title,
                    IsComplete = card.BoardList != null ? card.BoardList.CardStatus == Domain.Enums.BoardListCardStatus.DONE : false,
                    Title = card.Title,
                    IssueType = card.IssueType.Summary,
                    Status = card.Status.ToString(),
                    SerialKey = $"{card.IssueType.Board.Key}-{card.SerialNumber}"
                })
                .ToListAsync();

            return cards;
        }
    }
}
