using AutoMapper;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Persistence.Identity;

namespace Harmony.Infrastructure.Mappings
{
    public class RoleClaimProfile : Profile
    {
        public RoleClaimProfile()
        {
            CreateMap<RoleClaimResponse, HarmonyRoleClaim>()
                .ForMember(nameof(HarmonyRoleClaim.ClaimType), opt => opt.MapFrom(c => c.Type))
                .ForMember(nameof(HarmonyRoleClaim.ClaimValue), opt => opt.MapFrom(c => c.Value))
                .ReverseMap();

            CreateMap<RoleClaimRequest, HarmonyRoleClaim>()
                .ForMember(nameof(HarmonyRoleClaim.ClaimType), opt => opt.MapFrom(c => c.Type))
                .ForMember(nameof(HarmonyRoleClaim.ClaimValue), opt => opt.MapFrom(c => c.Value))
                .ReverseMap();
        }
    }
}