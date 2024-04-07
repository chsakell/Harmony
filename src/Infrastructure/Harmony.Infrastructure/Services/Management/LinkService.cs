using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services.Management
{
    public class LinkService : ILinkService
    {
        private readonly ILinkRepository _linkRepository;
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly ICardRepository _cardRepository;

        public LinkService(ILinkRepository linkRepository,
            IIssueTypeRepository issueTypeRepository,
            IBoardRepository boardRepository,
            ICardRepository cardRepository)
        {
            _linkRepository = linkRepository;
            _issueTypeRepository = issueTypeRepository;
            _boardRepository = boardRepository;
            _cardRepository = cardRepository;
        }

        public async Task<List<LinkDto>> GetLinksForCard(Guid cardId)
        {
            var result = new List<LinkDto>();

            var links = await _linkRepository.Entities
                .Where(l => l.SourceCardId == cardId || l.TargetCardId == cardId)
                .OrderBy(l => l.DateCreated)
                .ToListAsync();

            var distinctCards = links.Select(l => l.SourceCardId)
                .Union(links.Select(l => l.TargetCardId)).Distinct();

            var cards = await (from card in _cardRepository.Entities.IgnoreQueryFilters()
                        join issue in _issueTypeRepository.Entities
                            on card.IssueTypeId equals issue.Id
                        join board in _boardRepository.Entities
                            on issue.BoardId equals board.Id
                        where distinctCards.Contains(card.Id)
                        select new
                        {
                            CardId = card.Id,
                            CardTitle = card.Title,
                            Board = new BoardDto()
                            {
                                Id = board.Id,
                                Title = board.Title,
                                Key = board.Key,
                            }
                        }).ToListAsync();

            foreach (var link in links)
            {
                var sourceCard = cards.FirstOrDefault(c => c.CardId == link.SourceCardId);
                var targetCard = cards.FirstOrDefault(c => c.CardId != link.TargetCardId);

                if(sourceCard == null || targetCard == null)
                {
                    continue;
                }

                var dto = new LinkDto()
                {
                    Id = link.Id,
                    SourceCardId = sourceCard.CardId,
                    SourceCardTitle = sourceCard.CardTitle,
                    SourceCardBoard = sourceCard.Board,
                    TargetCardId = targetCard.CardId,
                    TargetCardTitle = targetCard.CardTitle,
                    TargetCardBoard = targetCard.Board,
                    Type = link.Type,
                    DateCreated = link.DateCreated,
                };

                result.Add(dto);
            }

            return result;
        }
    }
}
