using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Comments.Commands.CreateComment
{
    public class CreateCommentCommand : BaseBoardCommand, IRequest<Result<CreateCommentResponse>>
    {
        public CreateCommentCommand(Guid cardId, string text)
        {
            CardId = cardId;
            Text = text;
        }

        public CreateCommentCommand()
        {
            
        }

        public Guid CardId { get; set; }

        [Required]
        public string Text { get; set; }
    }
}
