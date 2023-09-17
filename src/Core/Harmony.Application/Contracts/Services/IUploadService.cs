using Harmony.Application.Requests;

namespace Harmony.Application.Contracts.Services
{
    public interface IUploadService
    {
        string UploadAsync(UploadRequest request);
    }
}