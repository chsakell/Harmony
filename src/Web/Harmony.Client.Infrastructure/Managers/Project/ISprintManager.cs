using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateSprintIssue;
using Harmony.Application.Features.Sprints.Commands.CompleteSprint;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Application.Features.Sprints.Queries.GetSprintCards;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ISprintManager : IManager
    {
        Task<IResult> StartSprint(StartSprintCommand request);
        Task<IResult<bool>> CompleteSprint(CompleteSprintCommand request);
        Task<IResult<SprintDto>> GetSprint(Guid sprintId);
        Task<IResult<GetSprintReportsResponse>> GetSprintReports(Guid sprintId);
        Task<PaginatedResult<CardDto>> GetSprintCards(GetSprintCardsQuery query);
        Task<IResult<CardDto>> CreateSprintCardAsync(CreateSprintIssueCommand request);
    }
}
