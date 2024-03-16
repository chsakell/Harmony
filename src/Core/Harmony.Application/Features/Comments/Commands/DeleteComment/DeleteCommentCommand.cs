using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Comments.Commands.DeleteComment
{
    public class DeleteCommentCommand : BaseBoardCommand, IRequest<Result<bool>>
    {
        public Guid CommentId { get; set; }
        public Guid CardId { get; set; }

        public DeleteCommentCommand(Guid commentId, Guid cardId)
        {
            CommentId = commentId;
            CardId = cardId;
        }
    }
}
