using AutoMapper;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Extensions;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Specifications.Cards;
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
        private readonly IBoardService _boardService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly ICardRepository _cardRepository;

        public RemoveCardAttachmentCommandHandler(IUploadService uploadService,
            ICardActivityService cardActivityService,
            IHubClientNotifierService hubClientNotifierService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IBoardService boardService,
            INotificationsPublisher notificationsPublisher,
            ICardRepository cardRepository)
        {
            _uploadService = uploadService;
            _cardActivityService = cardActivityService;
            _hubClientNotifierService = hubClientNotifierService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _boardService = boardService;
            _notificationsPublisher = notificationsPublisher;
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
                    await _hubClientNotifierService.RemoveCardAttachment(command.BoardId, card.Id, command.AttachmentId);

                    if (card.Attachments.Count == 0)
                    {
                        var boardId = await _cardRepository.GetBoardId(card.Id);
                        var board = await _boardService.GetBoardInfo(boardId);

                        _notificationsPublisher
                                .PublishSearchIndexNotification(new CardHasAttachmentsUpdatedIndexNotification()
                                {
                                    ObjectID = card.Id.ToString(),
                                    HasAttachments = false
                                }, board.IndexName);
                    }

                    var result = new RemoveCardAttachmentResponse(command.CardId, command.AttachmentId);

                    return await Result<RemoveCardAttachmentResponse>
                        .SuccessAsync(result, "Attachment removed successfully.");
                }
            }

            return Result<RemoveCardAttachmentResponse>.Fail("Attachment not found");
        }
    }
}
