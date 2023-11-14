using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Helpers;
using Harmony.Application.Specifications.Cards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.Cards.Commands.RemoveCardAttachment
{
    public class RemoveCardAttachmentCommandHandler : IRequestHandler<RemoveCardAttachmentCommand, Result<RemoveCardAttachmentResponse>>
    {
        private readonly IUploadService _uploadService;
        private readonly ICardActivityService _cardActivityService;
        private readonly IHubClientNotifierService _hubClientNotifierService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICardRepository _cardRepository;

        public RemoveCardAttachmentCommandHandler(IUploadService uploadService,
            ICardActivityService cardActivityService,
            IHubClientNotifierService hubClientNotifierService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ICardRepository cardRepository)
        {
            _uploadService = uploadService;
            _cardActivityService = cardActivityService;
            _hubClientNotifierService = hubClientNotifierService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _cardRepository = cardRepository;
        }

        public async Task<Result<RemoveCardAttachmentResponse>> Handle(RemoveCardAttachmentCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var includes = new CardIncludes() { Attachments = true };

            var filter = new CardFilterSpecification(command.CardId, includes);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (card == null)
            {
                return Result<RemoveCardAttachmentResponse>.Fail("Card not found");
            }

            var attachmentToDelete = card.Attachments.FirstOrDefault(a => a.Id == command.AttachmentId);

            if (attachmentToDelete != null)
            {
                card.Attachments.Remove(attachmentToDelete);

                var dbResult = await _cardRepository.Update(card);

                if (dbResult > 0)
                {
                    var boardId = await _cardRepository.GetBoardId(card.Id);

                    await _hubClientNotifierService.RemoveCardAttachment(boardId, card.Id, command.AttachmentId);

                    var result = new RemoveCardAttachmentResponse(command.CardId, command.AttachmentId);

                    return await Result<RemoveCardAttachmentResponse>
                        .SuccessAsync(result, "Attachment removed successfully.");
                }
            }

            return Result<RemoveCardAttachmentResponse>.Fail("Attachment not found");
        }
    }
}
