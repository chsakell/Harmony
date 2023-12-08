using Harmony.Application.DTO;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;
using Harmony.Application.Features.Lists.Commands.ArchiveList;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Application.Features.Comments.Commands.CreateComment;
using Harmony.Domain.Enums;

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

        public async Task<IResult<List<CommentDto>>> GetCardComments(Guid cardId)
        {
            var response = await _httpClient
                .GetAsync(Routes.CommentEndpoints.GetCard(cardId));

            return await response.ToResult<List<CommentDto>>();
        }
    }
}
