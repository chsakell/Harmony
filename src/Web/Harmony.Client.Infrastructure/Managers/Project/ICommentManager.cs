using Harmony.Application.DTO;
using Harmony.Application.Features.Comments.Commands.CreateComment;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ICommentManager : IManager
    {
		Task<IResult<CreateCommentResponse>> CreateCommentAsync(CreateCommentCommand request);
        Task<IResult<List<CommentDto>>> GetCardComments(Guid cardId);
    }
}
