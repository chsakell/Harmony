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

namespace Harmony.Application.Features.Boards.Commands.CreateList
{
	public class CreateListCommand : IRequest<Result<Guid>>
	{
		[Required]
		public string Name { get; set; }

		public Guid BoardId { get; set; }

		public CreateListCommand(string name, Guid boardId)
		{
			Name = name;
			BoardId = boardId;
		}
	}
}
