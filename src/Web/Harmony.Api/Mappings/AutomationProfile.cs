using AutoMapper;
using Harmony.Application.DTO.Automation;
using Harmony.Automations.Protos;
using Harmony.Domain.Enums.Automations;

namespace Harmony.Api.Mappings
{
    public class AutomationProfile : Profile
    {
        public AutomationProfile()
        {
            CreateMap<AutomationTemplateProto, AutomationTemplateDto>()
                .ForMember(dto => dto.Type, opt =>
                    opt.MapFrom(auto => (AutomationType)auto.Type));
        }
    }
}
