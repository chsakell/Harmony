using AutoMapper;
using AutoMapper.QueryableExtensions;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Requests;
using Harmony.Application.Specifications.Cards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Harmony.Shared.Constants.Permission.Permissions;

namespace Harmony.Application.Features.Cards.Commands.UploadFile
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<UploadFileResponse>>
    {
        private readonly IUploadService _uploadService;
        private readonly ICardActivityService _cardActivityService;
        private readonly IHubClientNotifierService _hubClientNotifierService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICardRepository _cardRepository;

        public UploadFileCommandHandler(IUploadService uploadService,
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

        public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var includes = new CardIncludes() { Attachments = true };

            var filter = new CardFilterSpecification(command.CardId, includes);

            var card = await _cardRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            if (card == null)
            {
                return Result<UploadFileResponse>.Fail("Card not found");
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
                await _cardActivityService.CreateActivity(card.Id, userId,
                    CardActivityType.AttachmentAdded, card.DateUpdated.Value, 
                    command.FileName, url: $"files/{attachment.Type.ToDescriptionString()}/{attachment.FileName}");
            }

            var attachmentDto = _mapper.Map<AttachmentDto>(attachment);

            var boardId = await _cardRepository.GetBoardId(card.Id);

            await _hubClientNotifierService.AddCardAttachment(boardId, card.Id, attachmentDto);

            var result = new UploadFileResponse(command.CardId, attachmentDto);

            return await Result<UploadFileResponse>.SuccessAsync(result, "File uploaded successfully.");
        }
    }
}
