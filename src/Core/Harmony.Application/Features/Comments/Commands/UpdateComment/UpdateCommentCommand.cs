using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Comments.Commands.UpdateComment
{
    public class UpdateCommentCommand : IRequest<Result<bool>>
    {
        public UpdateCommentCommand(Guid commentId, string text)
        {
            CommentId = commentId;
            Text = text;
        }

        public Guid CommentId { get; set; }

        [Required]
        public string Text { get; set; }
    }
}
