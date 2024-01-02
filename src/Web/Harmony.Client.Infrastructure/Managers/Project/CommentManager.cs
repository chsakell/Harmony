using Harmony.Application.DTO;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;
using Harmony.Application.Features.Comments.Commands.CreateComment;
using Harmony.Application.Features.Comments.Commands.UpdateComment;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for comments
    /// </summary>
    public class CommentManager : ICommentManager
    {
        private readonly HttpClient _httpClient;

        public CommentManager(HttpClient client)
        {
            _httpClient = client;
        }

		public async Task<IResult<CreateCommentResponse>> CreateCommentAsync(CreateCommentCommand request)
		{
			var response = await _httpClient
                .PostAsJsonAsync(Routes.CommentEndpoints.Index, request);

			return await response.ToResult<CreateCommentResponse>();
		}

        public async Task<IResult<bool>> UpdateCommentAsync(UpdateCommentCommand request)
        {
            var response = await _httpClient
                .PutAsJsonAsync(Routes.CommentEndpoints.GetComment(request.CommentId), request);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<List<CommentDto>>> GetCardComments(Guid cardId)
        {
            var response = await _httpClient
                .GetAsync(Routes.CommentEndpoints.GetCard(cardId));

            return await response.ToResult<List<CommentDto>>();
        }

        public async Task<IResult<bool>> DeleteComment(Guid commentId)
        {
            var response = await _httpClient
                .DeleteAsync(Routes.CommentEndpoints.GetComment(commentId));

            return await response.ToResult<bool>();
        }
    }
}
