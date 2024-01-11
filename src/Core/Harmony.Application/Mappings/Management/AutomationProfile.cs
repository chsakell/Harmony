using AutoMapper;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Automation;

namespace Harmony.Application.Mappings.Management
{
    public class AutomationProfile : Profile
    {
        public AutomationProfile()
        {
            CreateMap<AutomationTemplate, AutomationTemplateDto>();
        }
    }
}
