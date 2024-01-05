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
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICardManager : IManager
    {
        Task<IResult<LoadCardResponse>> LoadCardAsync(LoadCardQuery request);
        Task<IResult<CardDto>> CreateCardAsync(CreateCardCommand request);
        Task<IResult<CardDto>> CreateChildIssueAsync(CreateChildIssueCommand request);
        Task<IResult<CardDto>> CreateBacklogItemAsync(CreateBacklogCommand request);
        Task<IResult<bool>> UpdateBacklogItemAsync(UpdateBacklogCommand request);
        Task<IResult<CardDto>> MoveCardAsync(MoveCardCommand request);
        Task<IResult<bool>> UpdateDescriptionAsync(UpdateCardDescriptionCommand request);
        Task<IResult<bool>> UpdateStoryPointsAsync(UpdateCardStoryPointsCommand request);
        Task<IResult<bool>> UpdateIssueTypeAsync(UpdateCardIssueTypeCommand request);
        Task<IResult<bool>> UpdateTitleAsync(UpdateCardTitleCommand request);
        Task<IResult<bool>> UpdateStatusAsync(UpdateCardStatusCommand request);
        Task<IResult<List<LabelDto>>> GetCardLabelsAsync(GetCardLabelsQuery request);
        Task<IResult<LabelDto>> ToggleCardLabel(ToggleCardLabelCommand request);
        Task<IResult<bool>> UpdateDatesAsync(UpdateCardDatesCommand request);
        Task<IResult<List<CardActivityDto>>> GetCardActivityAsync(GetCardActivityQuery request);
        Task<IResult<List<CardMemberResponse>>> GetCardMembersAsync(string cardId);
        Task<IResult<AddUserCardResponse>> AddCardMemberAsync(AddUserCardCommand command);
        Task<IResult<RemoveUserCardResponse>> RemoveCardMemberAsync(RemoveUserCardCommand command);
        Task<IResult<RemoveCardAttachmentResponse>> RemoveCardAttachmentAsync(RemoveCardAttachmentCommand command);
        Task<IResult<SprintDto>> MoveCardsToSprintAsync(MoveToSprintCommand request);
    }
}
