using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Domain.Entities;

namespace Harmony.Application.Mappings.Management
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {
            CreateMap<Workspace, GetAllForUserWorkspaceResponse>().ReverseMap();
			CreateMap<Board, LoadWorkspaceResponse>().ReverseMap();
			CreateMap<Board, GetBoardResponse>();

            CreateMap<BoardList, BoardListDto>();
            CreateMap<Card, CardDto>();
            CreateMap<CheckList, CheckListDto>();
            CreateMap<CheckListItem, CheckListItemDto>();
            CreateMap<Card, LoadCardResponse>();
		}
    }
}
