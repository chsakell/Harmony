using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.AddUserCard;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.CreateChildIssue;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;
using Harmony.Application.Features.Cards.Commands.RemoveCardAttachment;
using Harmony.Application.Features.Cards.Commands.RemoveUserCard;
using Harmony.Application.Features.Cards.Commands.ToggleCardLabel;
using Harmony.Application.Features.Cards.Commands.UpdateBacklog;
using Harmony.Application.Features.Cards.Commands.UpdateCardDates;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Commands.UpdateCardIssueType;
using Harmony.Application.Features.Cards.Commands.UpdateCardStatus;
using Harmony.Application.Features.Cards.Commands.UpdateCardStoryPoints;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Cards.Queries.GetActivity;
using Harmony.Application.Features.Cards.Queries.GetCardMembers;
using Harmony.Application.Features.Cards.Queries.GetLabels;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for cards
    /// </summary>
    public class CardManager : ICardManager
    {
        private readonly HttpClient _httpClient;
        

        public CardManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<LoadCardResponse>> LoadCardAsync(LoadCardQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.CardEndpoints.Get(request.CardId));

            return await response.ToResult<LoadCardResponse>();
        }

        public async Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request)
		{
			var response = await _httpClient.PostAsJsonAsync(Routes.CardEndpoints.Index, request);

			return await response.ToResult<CardDto>();
		}

        public async Task<IResult<CardDto>> CreateChildIssueAsync(CreateChildIssueCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.CardEndpoints.ChildIssue(request.CardId), request);

            return await response.ToResult<CardDto>();
        }

        public async Task<IResult<CardDto>> CreateBacklogItemAsync(CreateBacklogCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.CardEndpoints.Backlog, request);

            return await response.ToResult<CardDto>();
        }

        public async Task<IResult<bool>> UpdateBacklogItemAsync(UpdateBacklogCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes
                .CardEndpoints.BacklogItem(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<CardDto>> MoveCardAsync(MoveCardCommand request)
		{
			var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Move(request.CardId), request);

			return await response.ToResult<CardDto>();
		}

        public async Task<IResult<bool>> UpdateDescriptionAsync(UpdateCardDescriptionCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Description(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateStoryPointsAsync(UpdateCardStoryPointsCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.StoryPoints(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateIssueTypeAsync(UpdateCardIssueTypeCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.IssueType(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateTitleAsync(UpdateCardTitleCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Title(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> UpdateStatusAsync(UpdateCardStatusCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Status(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<LabelDto>> ToggleCardLabel(ToggleCardLabelCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Labels(request.CardId), request);
            
            return await response.ToResult<LabelDto>();
        }

        public async Task<IResult<List<LabelDto>>> GetCardLabelsAsync(GetCardLabelsQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.CardEndpoints.GetLabels(request.CardId));

            return await response.ToResult<List<LabelDto>>();
        }

        public async Task<IResult<bool>> UpdateDatesAsync(UpdateCardDatesCommand request)
        {
            var response = await _httpClient.PutAsJsonAsync(Routes.CardEndpoints.Dates(request.CardId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<List<CardActivityDto>>> GetCardActivityAsync(GetCardActivityQuery request)
        {
            var response = await _httpClient.GetAsync(Routes.CardEndpoints.GetActivity(request.CardId));

            return await response.ToResult<List<CardActivityDto>>();
        }

        public async Task<IResult<List<CardMemberResponse>>> GetCardMembersAsync(string cardId)
        {
            var response = await _httpClient.GetAsync(Routes.CardEndpoints.GetMembers(cardId));

            return await response.ToResult<List<CardMemberResponse>>();
        }

        public async Task<IResult<AddUserCardResponse>> AddCardMemberAsync(AddUserCardCommand command)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.CardEndpoints
                .GetMembers(command.CardId.ToString()), command);

            return await response.ToResult<AddUserCardResponse>();
        }

        public async Task<IResult<RemoveUserCardResponse>> RemoveCardMemberAsync(RemoveUserCardCommand command)
        {
            var response = await _httpClient.DeleteAsync(Routes.CardEndpoints
                .GetCardMember(command.CardId.ToString(), command.UserId, command.BoardId));

            return await response.ToResult<RemoveUserCardResponse>();
        }

        public async Task<IResult<RemoveCardAttachmentResponse>> RemoveCardAttachmentAsync(RemoveCardAttachmentCommand command)
        {
            var response = await _httpClient.DeleteAsync(Routes.CardEndpoints
                .GetCardAttachment(command.CardId, command.AttachmentId));

            return await response.ToResult<RemoveCardAttachmentResponse>();
        }

        public async Task<IResult<SprintDto>> MoveCardsToSprintAsync(MoveToSprintCommand request)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.BoardEndpoints
                .Sprints(request.BoardId), request);

            return await response.ToResult<SprintDto>();
        }
    }
}
