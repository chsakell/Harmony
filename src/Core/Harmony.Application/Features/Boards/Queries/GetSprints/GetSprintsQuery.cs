﻿using Harmony.Application.DTO;
using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetSprints
{
    public class GetSprintsQuery : PagedRequest, IRequest<PaginatedResult<SprintDto>>
    {
        public Guid BoardId { get; set; }

        public GetSprintsQuery(Guid boardId)
        {
            BoardId = boardId;
        }

        public string SearchTerm { get; set; }

        public GetSprintsQuery(Guid workspaceId, int pageNumber, int pageSize,
            string searchTerm, string orderBy)
        {
            BoardId = workspaceId;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                OrderBy = orderBy.Split(',');
            }
        }
    }
}