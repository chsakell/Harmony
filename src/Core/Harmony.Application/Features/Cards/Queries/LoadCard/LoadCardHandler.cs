﻿using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Features.Workspaces.Queries.GetIssueTypes;
using Harmony.Domain.Enums.SourceControl;
using Harmony.Integrations.SourceControl.Protos;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Cards.Queries.LoadCard
{
    /// <summary>
    /// Handler for loading card
    /// </summary>
    public class LoadCardHandler : IRequestHandler<LoadCardQuery, IResult<LoadCardResponse>>
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly ISender _sender;
        private readonly ILinkService _linkService;
        private readonly IBoardRepository _boardRepository;
        private readonly SourceControlService.SourceControlServiceClient _sourceControlServiceClient;
        private readonly IStringLocalizer<LoadCardHandler> _localizer;
        private readonly IMapper _mapper;

        public LoadCardHandler(ICardRepository CardRepository,
            ICurrentUserService currentUserService,
            IUserService userService,
            ISender sender,
            ILinkService linkService,
            IBoardRepository boardRepository,
            SourceControlService.SourceControlServiceClient sourceControlServiceClient,
            IStringLocalizer<LoadCardHandler> localizer,
            IMapper mapper)
        {
            _cardRepository = CardRepository;
            _currentUserService = currentUserService;
            _userService = userService;
            _sender = sender;
            _linkService = linkService;
            _boardRepository = boardRepository;
            _sourceControlServiceClient = sourceControlServiceClient;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<LoadCardResponse>> Handle(LoadCardQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<LoadCardResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var card = await _cardRepository.Load(request.CardId);

            var result = _mapper.Map<LoadCardResponse>(card);

            result.Links = await _linkService.GetLinksForCard(request.CardId);

            var issueTypesResult = await _sender.Send(new GetIssueTypesQuery(result.BoardId));

            if(issueTypesResult.Succeeded)
            {
                result.IssueTypes = issueTypesResult.Data;
            }

            var boardLists = await _sender.Send(new GetBoardListsQuery(result.BoardId));
            
            if(boardLists.Succeeded)
            {
                result.BoardLists = boardLists.Data;
            }

            var boardId = card?.BoardList?.Board.Id ?? card.IssueType?.BoardId;
            var board = await _boardRepository.GetAsync(boardId.Value);

            var cardBranches = await _sourceControlServiceClient
                .GetCardBranchesAsync(new GetCardBranchesRequest()
                {
                    SerialKey = $"{board.Key}-{result.SerialNumber}"
            });

            if(cardBranches.Success && cardBranches.Branches.Any())
            {
                result.Branches = new List<CardBranchDto>();

                foreach (var cardBranch in cardBranches.Branches)
                {
                    result.Branches.Add(new CardBranchDto()
                    {
                        Id = cardBranch.Id,
                        Name = cardBranch.Name,
                        Provider = (SourceControlProvider)cardBranch.Provider,
                    });
                }
            }

            var cardUserIds = card.Members.Select(m => m.UserId).Distinct();

            var cardUsers = (await _userService.GetAllAsync(cardUserIds)).Data;

            result.Members = _mapper.Map<List<CardMemberDto>>(cardUsers);

            return await Result<LoadCardResponse>.SuccessAsync(result);
        }
    }
}
