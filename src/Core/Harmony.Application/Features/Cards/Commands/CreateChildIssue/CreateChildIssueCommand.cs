using Harmony.Application.DTO;
using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateChildIssue;

public class CreateChildIssueCommand : BaseBoardCommand, IRequest<Result<CardDto>>
{
    [Required]
    public string Title { get; set; }
    public Guid CardId { get; set; }

    public Guid ListId { get; set; }
}
