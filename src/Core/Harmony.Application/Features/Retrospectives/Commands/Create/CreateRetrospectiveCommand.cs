using Harmony.Application.DTO;
using Harmony.Application.Models;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Retrospectives.Commands.Create
{
    /// <summary>
    /// Command to create a retrospective
    /// </summary>
    public class CreateRetrospectiveCommand : BaseBoardCommand, IRequest<Result<RetrospectiveDto>>
    {
        [Required]
        [MaxLength(300)]
        [MinLength(5)]
        public string Name { get; set; }
        public RetrospectiveType Type { get; set; }
        public Guid? SprintId { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxVotesPerUser { get; set; }
        public bool HideVoteCount { get; set; }
        public bool HideCardsInitially { get; set; }
        public bool DisableVotingInitially { get; set; }
        public bool ShowCardsAuthor { get; set; }

        public CreateRetrospectiveCommand(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}
