using Azure;
using Harmony.Api.Controllers;
using Harmony.Api.Controllers.Management;
using Harmony.Application.Constants;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Harmony.Api.Controllers.Automation
{
    public class AutomationsController : BaseApiController<CardsController>
    {
        private readonly HttpClient? _httpClient;
        public AutomationsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(EndpointConstants.Automation);
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var result = await _httpClient.GetFromJsonAsync<
                    Shared.Wrapper.Result<List<AutomationTemplateDto>>>("/api/Automations/templates");

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAutomationCommand command)
        {
            var responseMessage = await _httpClient.PostAsJsonAsync("/api/Automations", command);

            var result = await responseMessage.Content
                .ReadFromJsonAsync<Shared.Wrapper.Result<string>>();
            
            return Ok(result);
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
