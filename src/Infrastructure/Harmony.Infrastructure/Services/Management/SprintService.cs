using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using static Harmony.Application.Events.BoardListArchivedEvent;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Harmony.Infrastructure.Services.Management
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly ICardRepository _cardRepository;

        public SprintService(ISprintRepository sprintRepository,
                                ICardRepository cardRepository)
        {
            _sprintRepository = sprintRepository;
            _cardRepository = cardRepository;
        }

        public async Task<GetSprintReportsResponse> GetSprintReports(Guid sprintId)
        {
            var sprint = await _sprintRepository.GetSprint(sprintId);

            var sprintCards = await (from card in _cardRepository.Entities
                                     where card.SprintId == sprintId
                                     select card).ToListAsync();

            var completedCards = sprintCards.Where(c => c.DateCompleted.HasValue);

            var result = new GetSprintReportsResponse();

            var totalStoryPoints = sprintCards.Sum(c => c.StoryPoints) ?? 0;
            var remainingStoryPoints = totalStoryPoints;
            var burnDownReportDates = new List<string>();
            var remainingStoryPointsSeries = new List<double>();

            foreach (var day in EachDay(sprint.StartDate.Value, sprint.EndDate.Value))
            {
                burnDownReportDates.Add(day.Date.ToString("MMM dd"));

                foreach (var completedCard in completedCards
                    .Where(c => c.DateCompleted.Value.Date == day.Date))
                {
                    remainingStoryPoints -= (int)(completedCard.StoryPoints ?? 0);
                }

                remainingStoryPointsSeries.Add(remainingStoryPoints);
            }

            var burnDownReport = new BurnDownReportDto()
            {
                Name = "BurnDown chart",
                Dates = burnDownReportDates,
                RemainingStoryPoints = remainingStoryPointsSeries
            };

            result.BurnDownReport = burnDownReport;

            return result;
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}
