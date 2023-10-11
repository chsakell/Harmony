using AutoMapper;
using AutoMapper.QueryableExtensions;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Requests;
using Harmony.Application.Specifications.Cards;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.Cards.Commands.UploadFile
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<UploadFileResponse>>
    {
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ICardRepository _cardRepository;

        public UploadFileCommandHandler(IUploadService uploadService,
            IMapper mapper,
            ICardRepository cardRepository)
        {
            _uploadService = uploadService;
            _mapper = mapper;
            _cardRepository = cardRepository;
        }

        public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
        {
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

            await _cardRepository.Update(card);

            var attachmentDto = _mapper.Map<AttachmentDto>(attachment);

            var result = new UploadFileResponse(command.CardId, attachmentDto);

            return await Result<UploadFileResponse>.SuccessAsync(result, "File uploaded successfully.");
        }
    }
}
