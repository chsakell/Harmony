using Harmony.Application.Features.Cards.Commands.UploadFile;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Content
{
    public class FileManager : IFileManager
    {
        private readonly HttpClient _client;
        

        public FileManager(HttpClient client)
        {
            _client = client;
        }

        public async Task<IResult<UploadFileResponse>> UploadFile(UploadFileCommand command)
        {
            var response = await _client.PostAsJsonAsync(Routes.FileEndpoints.Index, command);

            return await response.ToResult<UploadFileResponse>();
        }
    }
}
