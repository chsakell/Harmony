using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Responses;
using Harmony.Client.Infrastructure.Models.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
