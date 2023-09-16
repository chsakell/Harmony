using AutoMapper;
using Harmony.Application.Responses;
using Harmony.Persistence.Identity;

namespace Harmony.Infrastructure.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleResponse, HarmonyRole>().ReverseMap();
        }
    }
}