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

namespace Harmony.Application.Features.Boards.Commands
{
    public class CreateBoardCommand : IRequest<Result<Guid>>
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        [Required]
        public string WorkspaceId { get; set; }
        public BoardVisibility Visibility { get; set; }
    }
}
