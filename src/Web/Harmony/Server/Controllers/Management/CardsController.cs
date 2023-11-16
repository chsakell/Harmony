using Harmony.Application.Features.Cards.Commands.AddUserCard;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.RemoveCardAttachment;
using Harmony.Application.Features.Cards.Commands.RemoveUserCard;
using Harmony.Application.Features.Cards.Commands.ToggleCardLabel;
using Harmony.Application.Features.Cards.Commands.UpdateCardDates;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Commands.UpdateCardStatus;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Cards.Queries.GetActivity;
using Harmony.Application.Features.Cards.Queries.GetCardMembers;
using Harmony.Application.Features.Cards.Queries.GetLabels;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Microsoft.AspNetCore.Mvc;

namespace Harmony.Server.Controllers.Management
{
    /// <summary>
    /// Controller for Card operations
    /// </summary>
    public class CardsController : BaseApiController<CardsController>
    {

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await _mediator.Send(new LoadCardQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateCardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

		[HttpPut("{id:guid}/move")]
		public async Task<IActionResult> Put(Guid id, MoveCardCommand command)
		{
			return Ok(await _mediator.Send(command));
		}

        [HttpPut("{id:guid}/description")]
        public async Task<IActionResult> UpdateDescription(Guid id, UpdateCardDescriptionCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:guid}/title")]
        public async Task<IActionResult> UpdateTitle(Guid id, UpdateCardTitleCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateCardStatusCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("{id:guid}/labels")]
        public async Task<IActionResult> GetLabels(Guid id)
        {
            return Ok(await _mediator.Send(new GetCardLabelsQuery(id)));
        }

        [HttpGet("{id:guid}/activity")]
        public async Task<IActionResult> GetActivity(Guid id)
        {
            return Ok(await _mediator.Send(new GetCardActivityQuery(id)));
        }

        [HttpPut("{id:guid}/labels")]
        public async Task<IActionResult> ToggleLabel(ToggleCardLabelCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpPut("{id:guid}/dates")]
        public async Task<IActionResult> UpdateDates(UpdateCardDatesCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            return Ok(await _mediator.Send(new GetCardMembersQuery(id)));
        }

        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(AddUserCardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("{id:guid}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(Guid id, string userId)
        {
            return Ok(await _mediator.Send(new RemoveUserCardCommand(id, userId)));
        }

        [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
        public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId)
        {
            return Ok(await _mediator.Send(new RemoveCardAttachmentCommand(id, attachmentId)));
        }
    }
}
