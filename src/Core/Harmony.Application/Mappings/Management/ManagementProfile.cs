using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Application.Notifications;
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

            CreateMap<Board, BoardDto>().ReverseMap();
			CreateMap<Board, GetBoardResponse>();
            CreateMap<Board, GetWorkspaceBoardResponse>()
                .ForMember(dto => dto.TotalUsers, opt =>
                    opt.MapFrom(c => c.Users.Count()));

            CreateMap<BoardList, BoardListDto>();
            CreateMap<BoardList, GetBoardListResponse>();

            CreateMap<Card, CardDto>()
                .ForMember(dto => dto.TotalItems, opt =>
                    opt.MapFrom(c => c.CheckLists.SelectMany(l => l.Items).Count()))
                .ForMember(dto => dto.TotalItemsCompleted, opt =>
                    opt.MapFrom(c => c.CheckLists.SelectMany(l => l.Items)
                            .Where(i => i.IsChecked).Count()))
                .ForMember(dto => dto.Labels, opt =>
                    opt.MapFrom(c => c.Labels.Select(cl => cl.Label)))
                .ForMember(dto => dto.TotalAttachments, opt => 
                    opt.MapFrom(c => c.Attachments != null ? c.Attachments.Count : 0))
                .ForMember(dto => dto.IssueType, opt =>
                    opt.MapFrom(c => new IssueTypeDto()
                    {
                        Id = c.IssueType.Id,
                        Summary = c.IssueType.Summary,
                    }))
                .ForMember(dto => dto.TotalComments, opt =>
                    opt.MapFrom(c => c.Comments != null ? c.Comments.Count : 0))
                .ForMember(dto => dto.TotalChildren, opt =>
                    opt.MapFrom(c => c.Children != null ? c.Children.Count : 0));

            CreateMap<Card, GetBacklogItemResponse>();
            CreateMap<CardDto, CardCreatedMessage>();

            CreateMap<CheckList, CheckListDto>();
            CreateMap<CheckListItem, CheckListItemDto>();
            CreateMap<Card, LoadCardResponse>()
                .ForMember(dto => dto.Labels, opt => 
                    opt.MapFrom(src => src.Labels.Select(cl => cl.Label)))
                .ForMember(dto => dto.BoardId, opt =>
                    opt.MapFrom(src => src.IssueType != null ? src.IssueType.BoardId : src.BoardList.BoardId))
                .ForMember(dto => dto.BoardType, opt =>
                    opt.MapFrom(src => src.BoardList.Board.Type))
                .ForMember(dto => dto.BoardTitle, opt =>
                    opt.MapFrom(src => src.IssueType != null ? src.IssueType.Board.Title : src.BoardList.Board.Title))
                .ForMember(dto => dto.IsChild, opt =>
                    opt.MapFrom(src => src.ParentCardId.HasValue)); ;

            CreateMap<Label, LabelDto>();
            CreateMap<CardLabel, LabelDto>()
                .ForMember(dto => dto.Title, opt => 
                    opt.MapFrom(c => c.Label.Title));

            CreateMap<Attachment, AttachmentDto>()
                .ForMember(dto => dto.Url, opt =>
                    opt.MapFrom(c => $"files/{c.Type.ToDescriptionString()}/{c.FileName}"));

            CreateMap<CardActivity, CardActivityDto>();

            CreateMap<UserCard, CardMemberDto>()
                .ForMember(dto => dto.Id, opt =>
                    opt.MapFrom(source => source.UserId));

            CreateMap<UserResponse, CardMemberDto>();

            CreateMap<UpdateListsPositionsCommand, UpdateListsPositionsResponse>();

            CreateMap<IssueType, IssueTypeDto>();
            CreateMap<Sprint, SprintDto>();
        }
    }
}
