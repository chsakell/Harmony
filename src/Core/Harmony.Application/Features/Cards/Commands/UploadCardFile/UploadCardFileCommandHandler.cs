using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Helpers;
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
        private readonly IHubClientNotifierService _hubClientNotifierService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICardRepository _cardRepository;

        public UploadCardFileCommandHandler(IUploadService uploadService,
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
                Type = command.Type
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
            }

            var attachmentDto = _mapper.Map<AttachmentDto>(attachment);

            await _hubClientNotifierService.AddCardAttachment(command.BoardId, card.Id, attachmentDto);

            var result = new UploadCardFileResponse(command.CardId, attachmentDto);

            return await Result<UploadCardFileResponse>.SuccessAsync(result, "File uploaded successfully.");
        }
    }
}
