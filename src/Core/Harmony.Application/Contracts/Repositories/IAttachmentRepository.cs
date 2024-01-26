using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Attachments
    /// </summary>
    public interface IAttachmentRepository
    {
        IQueryable<Attachment> Entities { get; }
        Task<int> CountAttachments(string userId);
    }
}
