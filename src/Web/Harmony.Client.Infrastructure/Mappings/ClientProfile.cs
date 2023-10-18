using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Client.Infrastructure.Models.Board;

namespace Harmony.Client.Infrastructure.Mappings
{

    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<LoadCardResponse, EditableCardModel>();
            CreateMap<BoardListDto, EditableBoardListModel>();
            CreateMap<CheckListDto, EditableCheckListModel>();
            CreateMap<CheckListItemDto, EditableCheckListItemModel>();
        }
    }
}
