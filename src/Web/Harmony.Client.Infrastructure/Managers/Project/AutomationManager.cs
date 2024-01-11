using Harmony.Application.DTO;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Events;
using Harmony.Application.Features.Boards.Commands.AddUserBoard;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Commands.RemoveUserBoard;
using Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess;
using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Application.Features.Cards.Commands.MoveToBacklog;
using Harmony.Application.Features.Cards.Commands.MoveToSprint;
using Harmony.Application.Features.Lists.Commands.UpdateListsPositions;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Features.Lists.Queries.LoadBoardList;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;
using MediatR;
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
    }
}
