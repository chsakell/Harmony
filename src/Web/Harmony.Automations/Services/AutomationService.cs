using Grpc.Core;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Automations.Protos;
using Harmony.Domain.Enums;
using MediatR;
using System.Diagnostics;

namespace Harmony.Automations.Services
{
    public class AutomationService : Protos.AutomationService.AutomationServiceBase
    {
        private readonly IMediator _mediator;

        public AutomationService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async override Task<GetAutomationTemplatesResponse> GetAutomationTemplates(GetAutomationTemplatesRequest request, ServerCallContext context)
        {
            var result = await _mediator.Send(new GetAutomationTemplatesQuery());

            var response = new GetAutomationTemplatesResponse()
            {
                Success = result.Succeeded
            };

            if(result.Succeeded)
            {
                response.Templates.AddRange(result.Data.Select(MapToProto));
            }

            response.Messages.AddRange(result.Messages);

            return response;
        }
        public async override Task<CreateAutomationResponse> CreateAutomation(CreateAutomationRequest request, ServerCallContext context)
        {
            var result = await _mediator
                .Send(new CreateAutomationCommand(request.Automation,(AutomationType) request.Type));

            var response = new CreateAutomationResponse()
            {
                Success = result.Succeeded,
                AutomationId = result.Data,
            };

            response.Messages.AddRange(result.Messages);

            return response;
        }

        private AutomationTemplate MapToProto(AutomationTemplateDto automationTemplateDto)
        {
            return new AutomationTemplate()
            {
                 Id = automationTemplateDto.Id,
                 Enabled = automationTemplateDto.Enabled,
                 Name = automationTemplateDto.Name,
                 Summary = automationTemplateDto.Summary,
                 Type = (int)automationTemplateDto.Type
            };
        }
    }
}
