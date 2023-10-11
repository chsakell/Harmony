using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Cards.Commands.UploadFile;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Client.Infrastructure.Managers.Project;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Managers.Content
{
    public class FileManager : IFileManager
    {
        private readonly HttpClient _client;
        public event EventHandler<AttachmentAddedEvent> OnCardAttachmentAdded;

        public FileManager(HttpClient client)
        {
            _client = client;
        }

        public async Task<IResult<UploadFileResponse>> UploadFile(UploadFileCommand command)
        {
            var response = await _client.PostAsJsonAsync(Routes.FileEndpoints.Index, command);

            var result = await response.ToResult<UploadFileResponse>();

            if (result.Succeeded)
            {
                OnCardAttachmentAdded?.Invoke(this, 
                    new AttachmentAddedEvent(command.CardId, result.Data.Attachment));
            }

            return result;
        }
    }
}
