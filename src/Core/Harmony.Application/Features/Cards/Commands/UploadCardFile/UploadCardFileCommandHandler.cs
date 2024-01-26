using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Helpers;
using Harmony.Application.Notifications;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Specifications.Cards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.Cards.Commands.UploadCardFile
{
    public class UploadCardFileCommandHandler : IRequestHandler<UploadCardFileCommand, Result<UploadCardFileResponse>>
    {
        private readonly IUploadService _uploadService;
        private readonly ICardActivityService _cardActivityService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IBoardService _boardService;
        private readonly INotificationsPublisher _notificationsPublisher;
        private readonly ICardRepository _cardRepository;

        public UploadCardFileCommandHandler(IUploadService uploadService,
            ICardActivityService cardActivityService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IBoardService boardService,
            INotificationsPublisher notificationsPublisher,
            ICardRepository cardRepository)
        {
            _uploadService = uploadService;
            _cardActivityService = cardActivityService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _boardService = boardService;
            _notificationsPublisher = notificationsPublisher;
            _cardRepository = cardRepository;
        }

        public async Task<Result<UploadCardFileResponse>> Handle(UploadCardFileCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var includes = new CardIncludes() { Attachments = true };

            var filter = new CardFilterSpecification(command.CardId, includes);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (card == null)
            {
                return Result<UploadCardFileResponse>.Fail("Card not found");
            }

            var newFileName = _uploadService.UploadAsync(command).Replace(@"\", "/");

            var attachment = new Attachment()
            {
                CardId = command.CardId,
                FileName = newFileName,
                OriginalFileName = command.FileName,
                Type = command.Type,
                UserId = userId
            };

            card.Attachments.Add(attachment);

            var dbResult = await _cardRepository.Update(card);

            if (dbResult > 0)
            {
                var activityType = FileHelper.GetAttachmentType(command.Extension) == AttachmentType.CardImage ?
                    CardActivityType.ImageAttachmentAdded : CardActivityType.DocumentAttachmentAdded;

                await _cardActivityService.CreateActivity(card.Id, userId,
                    activityType, card.DateUpdated.Value,
                    command.FileName,
                    url: $"files/{attachment.Type.ToDescriptionString()}/{attachment.FileName}");

                if (card.Attachments.Count == 1)
                {
                    var board = await _boardService.GetBoardInfo(command.BoardId);

                    _notificationsPublisher
                            .PublishSearchIndexNotification(new CardHasAttachmentsUpdatedIndexNotification()
                            {
                                ObjectID = card.Id.ToString(),
                                HasAttachments = true
                            }, board.IndexName);
                }
            }

            var attachmentDto = _mapper.Map<AttachmentDto>(attachment);

            var message = new AttachmentAddedMessage(command.BoardId, card.Id, attachmentDto);

            _notificationsPublisher.PublishMessage(message,
                NotificationType.CardAttachmentAdded, routingKey: BrokerConstants.RoutingKeys.SignalR);

            var result = new UploadCardFileResponse(command.CardId, attachmentDto);

            return await Result<UploadCardFileResponse>.SuccessAsync(result, "File uploaded successfully.");
        }
    }
}
