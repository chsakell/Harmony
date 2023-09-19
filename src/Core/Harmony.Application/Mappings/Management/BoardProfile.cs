using AutoMapper;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Mappings.Management
{
    public class BoardProfile : Profile
    {
        public BoardProfile()
        {
            CreateMap<Board, GetAllForUserBoardResponse>().ReverseMap();

            CreateMap<Board, GetBoardResponse>();
        }
    }
}
