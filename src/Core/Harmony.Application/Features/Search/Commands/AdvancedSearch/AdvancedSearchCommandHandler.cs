using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Application.Features.Search.Queries.GlobalSearch;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Harmony.Shared.Constants.Permission.Permissions;

namespace Harmony.Application.Features.Search.Commands.AdvancedSearch
{
    internal class AdvancedSearchCommandHandler : IRequestHandler<AdvancedSearchCommand, IResult<List<SearchableCard>>>
    {
        private readonly ISearchService _searchService;
        private readonly IBoardService _boardService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GlobalSearchHandler> _localizer;

        public AdvancedSearchCommandHandler(ISearchService searchService, 
            IBoardService boardService,
            ICurrentUserService currentUserService,
            IStringLocalizer<GlobalSearchHandler> localizer)
        {
            _searchService = searchService;
            _boardService = boardService;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<IResult<List<SearchableCard>>> Handle(AdvancedSearchCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<SearchableCard>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _boardService.GetUserBoards(null, userId);

            var boardIds = userBoards.Select(b => b.Id).ToList();

            var result = await _searchService.Search(boardIds, request);

            return await Result<List<SearchableCard>>.SuccessAsync(result);
        }
    }
}
