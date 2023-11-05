using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Responses;
using Harmony.Client.Infrastructure.Models.Account;
using Harmony.Client.Infrastructure.Models.Board;

namespace Harmony.Client.Infrastructure.Mappings
{
    /// <summary>
    /// Automapper profile
    /// </summary>
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<LoadCardResponse, EditableCardModel>();
            CreateMap<BoardListDto, EditableBoardListModel>();
            CreateMap<CheckListDto, EditableCheckListModel>();
            CreateMap<CheckListItemDto, EditableCheckListItemModel>();
            CreateMap<UserResponse, UserModel>();
        }
    }
}
