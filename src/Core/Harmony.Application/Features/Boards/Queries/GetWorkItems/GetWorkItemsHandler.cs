using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Specifications.Cards;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Harmony.Application.Features.Boards.Queries.GetWorkItems
{
    public class GetWorkItemsHandler : IRequestHandler<GetWorkItemsQuery, PaginatedResult<CardDto>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly IMapper _mapper;

        public GetWorkItemsHandler(ICardRepository cardRepository,
            IMapper mapper)
        {
            _cardRepository = cardRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<CardDto>> Handle(GetWorkItemsQuery request, CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var filter = new CardItemFilterSpecification()
            {
                BoardId = request.BoardId,
                Title = request.CardTitle,
                IncludeIssueType = true,
                IssueTypes = request.IssueTypes,
                BoardLists = request.BoardLists,
                Sprints = request.Sprints,
            };

            filter.Build();

            var totalCards = await _cardRepository
                .Entities.Specify(filter)
                .CountAsync();

            var ordering = string.Empty;
            if(request.OrderBy == null || request.OrderBy.FirstOrDefault() == null)
            {
                ordering = string.Join(",", new string[] { "dateCreated" });
            }
            else
            {
                ordering = string.Join(",", request.OrderBy);
            }

            var cards = await _cardRepository
                .Entities.AsNoTracking()
                .IgnoreQueryFilters()
                .Specify(filter)
                .OrderBy(ordering)
                .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                .ToListAsync();

            var result = _mapper.Map<List<CardDto>>(cards);

            return PaginatedResult<CardDto>
                .Success(result, totalCards, request.PageNumber, request.PageSize);
        }
    }
}
