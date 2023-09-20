using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Commands.CreateCard;

public class CreateCardCommand : IRequest<Result<CardDto>>
{
	[Required]
	public string Name { get; set; }
	public Guid BoardId { get; set; }
	public Guid ListId { get; set; }

	public CreateCardCommand(string name, Guid boardId, Guid listId)
	{
		Name = name;
		BoardId = boardId;
		ListId = listId;
	}
}
