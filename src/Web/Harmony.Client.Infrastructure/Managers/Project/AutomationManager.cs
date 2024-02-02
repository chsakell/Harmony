using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;
using System.Net.Http.Json;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    /// <summary>
    /// Client manager for boards
    /// </summary>
    public class AutomationManager : IAutomationManager
    {
        private readonly HttpClient _httpClient;

        public AutomationManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<List<AutomationTemplateDto>>> GetTemplates()
        {
            var response = await _httpClient.GetAsync(Routes.AutomationEndpoints.Templates);
            return await response.ToResult<List<AutomationTemplateDto>>();
        }

        public async Task<IResult<List<T>>> GetAutomations<T>(Guid boardId, AutomationType type)
            where T : IAutomationDto
        {
            var response = await _httpClient.GetAsync(Routes.AutomationEndpoints.Automations(boardId, type));

            return await response.ToResult<List<T>>();
        }

        public async Task<IResult<string>> CreateAutomation(CreateAutomationCommand command)
        {
            var response = await _httpClient.PostAsJsonAsync(Routes.AutomationEndpoints.Index, command);

            return await response.ToResult<string>();
        }

        public async Task<IResult<bool>> ToggleAutomation(ToggleAutomationCommand command)
        {
            var response = await _httpClient
                .PutAsJsonAsync(Routes.AutomationEndpoints.ToggleAutomation(command.AutomationId), command);

            return await response.ToResult<bool>();
        }

        public async Task<IResult<bool>> RemoveAutomation(string automationId)
        {
            var response = await _httpClient
                .DeleteAsync(Routes.AutomationEndpoints.Automation(automationId));

            return await response.ToResult<bool>();
        }
    }
}
