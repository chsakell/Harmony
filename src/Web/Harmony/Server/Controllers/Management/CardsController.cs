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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _mediator.Send(new LoadCardQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateCardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

		[HttpPut("{id:int}/move")]
		public async Task<IActionResult> Put(Guid id, MoveCardCommand command)
		{
			return Ok(await _mediator.Send(command));
		}

        [HttpPut("{id:int}/description")]
        public async Task<IActionResult> UpdateDescription(Guid id, UpdateCardDescriptionCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:int}/title")]
        public async Task<IActionResult> UpdateTitle(Guid id, UpdateCardTitleCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateCardStatusCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("{id:int}/labels")]
        public async Task<IActionResult> GetLabels(int id)
        {
            return Ok(await _mediator.Send(new GetCardLabelsQuery(id)));
        }

        [HttpGet("{id:int}/activity")]
        public async Task<IActionResult> GetActivity(int id)
        {
            return Ok(await _mediator.Send(new GetCardActivityQuery(id)));
        }

        [HttpPut("{id:int}/labels")]
        public async Task<IActionResult> ToggleLabel(ToggleCardLabelCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpPut("{id:int}/dates")]
        public async Task<IActionResult> UpdateDates(UpdateCardDatesCommand request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpGet("{id:int}/members")]
        public async Task<IActionResult> GetMembers(int id)
        {
            return Ok(await _mediator.Send(new GetCardMembersQuery(id)));
        }

        [HttpPost("{id:int}/members")]
        public async Task<IActionResult> GetMembers(AddUserCardCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("{id:int}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, string userId)
        {
            return Ok(await _mediator.Send(new RemoveUserCardCommand(id, userId)));
        }

        [HttpDelete("{id:int}/attachments/{attachmentId:guid}")]
        public async Task<IActionResult> DeleteAttachment(int id, Guid attachmentId)
        {
            return Ok(await _mediator.Send(new RemoveCardAttachmentCommand(id, attachmentId)));
        }
    }
}
