using AutoMapper;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Automation;

namespace Harmony.Automations.Mappings
{
    public class AutomationProfile : Profile
    {
        public AutomationProfile()
        {
            CreateMap<AutomationTemplate, AutomationTemplateDto>();
        }
    }
}
