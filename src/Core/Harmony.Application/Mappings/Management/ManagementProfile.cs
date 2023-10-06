using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Features.Workspaces.Queries.LoadWorkspace;
using Harmony.Application.Responses;
using Harmony.Domain.Entities;

namespace Harmony.Application.Mappings.Management
{
    public class WorkspaceProfile : Profile
    {
        public WorkspaceProfile()
        {
            CreateMap<Workspace, WorkspaceDto>().ReverseMap();
            CreateMap<UserResponse, UserWorkspaceResponse>();

            CreateMap<Board, LoadWorkspaceResponse>().ReverseMap();
			CreateMap<Board, GetBoardResponse>();
            CreateMap<Board, GetWorkspaceBoardResponse>()
                .ForMember(dto => dto.TotalUsers, opt =>
                    opt.MapFrom(c => c.Users.Count()));

            CreateMap<BoardList, BoardListDto>();
            CreateMap<Card, CardDto>()
                .ForMember(dto => dto.TotalItems, opt =>
                    opt.MapFrom(c => c.CheckLists.SelectMany(l => l.Items).Count()))
                .ForMember(dto => dto.TotalItemsCompleted, opt =>
                    opt.MapFrom(c => c.CheckLists.SelectMany(l => l.Items)
                            .Where(i => i.IsChecked).Count()))
                .ForMember(dto => dto.Labels, opt =>
                    opt.MapFrom(c => c.Labels.Select(cl => cl.Label)));

            CreateMap<CheckList, CheckListDto>();
            CreateMap<CheckListItem, CheckListItemDto>();
            CreateMap<Card, LoadCardResponse>();

            CreateMap<Label, LabelDto>();
            CreateMap<CardLabel, LabelDto>()
                .ForMember(dto => dto.Title, opt => 
                    opt.MapFrom(c => c.Label.Title));
        }

        
    }

    //public class LabelResolver : IValueResolver<CardLabel, LabelDto, int>
    //{
    //    public int Resolve(CardLabel source, LabelDto destination, int member, ResolutionContext context)
    //    {
    //        return source.Value1 + source.Value2;
    //    }
    //}
}
