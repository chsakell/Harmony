using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Persistence.Identity;

namespace Harmony.Infrastructure.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserResponse, HarmonyUser>().ReverseMap();
            CreateMap<HarmonyUser, UserPublicInfo>();
        }
    }
}