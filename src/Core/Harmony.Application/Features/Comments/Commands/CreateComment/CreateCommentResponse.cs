using Harmony.Application.DTO;

namespace Harmony.Application.Features.Comments.Commands.CreateComment
{
    public class CreateCommentResponse
    {
        public Guid Id { get; set; }
        public UserPublicInfo User { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
