using AutoMapper;
using Azure;
using Grpc.Net.Client;
using Harmony.Api.Controllers;
using Harmony.Api.Controllers.Management;
using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Automations.Protos;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Harmony.Api.Controllers.Automation
{
    public class AutomationsController : BaseApiController<CardsController>
    {
        private readonly HttpClient? _httpClient;
        private readonly AppEndpointConfiguration _endpointConfiguration;
        private readonly IMapper _mapper;

        public AutomationsController(IHttpClientFactory httpClientFactory,
            IOptions<AppEndpointConfiguration> endpointConfiguration,
            IMapper mapper)
        {
            _httpClient = httpClientFactory.CreateClient(EndpointConstants.Automation);
            _endpointConfiguration = endpointConfiguration.Value;
            _mapper = mapper;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.AutomationEndpoint);
            var client = new AutomationService.AutomationServiceClient(channel);

            var getAutomationTemplatesResult = await client
                .GetAutomationTemplatesAsync(new GetAutomationTemplatesRequest());;

            return Ok(getAutomationTemplatesResult.Success ?
             Result<List<AutomationTemplateDto>>.Success(_mapper
             .Map<List<AutomationTemplateDto>>(getAutomationTemplatesResult.Templates),
                getAutomationTemplatesResult?.Messages?.FirstOrDefault())
             : Result.Fail(getAutomationTemplatesResult.Messages.ToList()));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAutomationCommand command)
        {
            using var channel = GrpcChannel.ForAddress(_endpointConfiguration.AutomationEndpoint);
            var client = new AutomationService.AutomationServiceClient(channel);

            var createAutomationResult = await client.CreateAutomationAsync(
                              new CreateAutomationRequest()
                              {
                                  Automation = command.Automation,
                                  Type = (int)command.Type
                              });

            return Ok(createAutomationResult.Success ?
             Result<string>.Success(createAutomationResult.AutomationId, 
                createAutomationResult?.Messages?.FirstOrDefault())
             : Result.Fail(createAutomationResult.Messages.ToList()));
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> Update(string id, ToggleAutomationCommand command)
        {
            var responseMessage = await _httpClient
                .PutAsJsonAsync($"/api/Automations/{id}/toggle", command);

            var result = await responseMessage.Content
                .ReadFromJsonAsync<Shared.Wrapper.Result<bool>>();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var responseMessage = await _httpClient
                .DeleteAsync($"/api/Automations/{id}");

            var result = await responseMessage.Content
                .ReadFromJsonAsync<Shared.Wrapper.Result<bool>>();

            return Ok(result);
        }

        [HttpGet("{boardId:guid}/types/{type:int}")]
        public async Task<IActionResult> GetAutomations(Guid boardId, AutomationType type)
        {
            var result = await _httpClient.GetFromJsonAsync<
                    Shared.Wrapper.Result<IEnumerable<SyncParentAndChildIssuesAutomationDto>>>($"/api/Automations/{boardId}/types/{(int)type}");

            return Ok(result);
        }
    }
}
