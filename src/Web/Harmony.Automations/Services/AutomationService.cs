using AutoMapper;
using Grpc.Core;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.RemoveAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Application.Features.Automations.Queries.GetAutomations;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Automations.Protos;
using Harmony.Domain.Enums.Automations;
using MediatR;
using System.Text.Json;

namespace Harmony.Automations.Services
{
    public class AutomationService : Protos.AutomationService.AutomationServiceBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public AutomationService(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
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

        public async override Task<ToggleAutomationResponse> ToggleAutomation(ToggleAutomationRequest request, ServerCallContext context)
        {
            var result = await _mediator.Send(new ToggleAutomationCommand(request.AutomationId, request.Enabled));

            var response = new ToggleAutomationResponse()
            {
                Success = result.Succeeded,
            };

            response.Messages.AddRange(result.Messages);

            return response;
        }

        public async override Task<RemoveAutomationResponse> RemoveAutomation(RemoveAutomationRequest request, ServerCallContext context)
        {
            var result = await _mediator.Send(new RemoveAutomationCommand(request.AutomationId));

            var response = new RemoveAutomationResponse()
            {
                Success = result.Succeeded,
            };

            response.Messages.AddRange(result.Messages);

            return response;
        }

        public async override Task<GetAutomationsResponse> GetAutomations(GetAutomationsRequest request, ServerCallContext context)
        {
            var result = await _mediator
                .Send(new GetAutomationsQuery((AutomationType)request.AutomationType, Guid.Parse(request.BoardId)));

            var response = new GetAutomationsResponse()
            {
                Success = result.Succeeded,
                Type = request.AutomationType
            };

            if(result.Succeeded)
            {
                response.Automations.AddRange(result.Data
                    .Select(automation => JsonSerializer.Serialize((object)automation)));
            }

            return response;
        }

        private AutomationTemplateProto MapToProto(AutomationTemplateDto automationTemplateDto)
        {
            return new AutomationTemplateProto()
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
