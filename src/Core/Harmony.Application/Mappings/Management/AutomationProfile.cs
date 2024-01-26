using AutoMapper;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Enums;

namespace Harmony.Application.Mappings.Management
{
    public class AutomationProfile : Profile
    {
        public AutomationProfile()
        {
            CreateMap<Automations.Protos.AutomationTemplate, AutomationTemplateDto>()
                .ForMember(dto => dto.Type, opt =>
                    opt.MapFrom(auto => (AutomationType)auto.Type));
        }
    }
}
