using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Mappings.Management
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {
            CreateMap<Workspace, GetAllForUserWorkspaceResponse>().ReverseMap();
			CreateMap<Board, GetAllForUserBoardResponse>().ReverseMap();
			CreateMap<Board, GetBoardResponse>();

            CreateMap<BoardList, BoardListDto>();
            CreateMap<Card, CardDto>();
		}
    }
}
