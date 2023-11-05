using Harmony.Application.Requests;

namespace Harmony.Application.Contracts.Services
{
    /// <summary>
    /// Service to upload files
    /// </summary>
    public interface IUploadService
    {
        string UploadAsync(UploadRequest request);
    }
}