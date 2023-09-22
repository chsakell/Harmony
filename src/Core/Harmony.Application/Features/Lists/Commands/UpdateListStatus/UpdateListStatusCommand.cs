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

namespace Harmony.Application.Features.Lists.Commands.ArchiveList;

public class UpdateListStatusCommand : IRequest<Result<bool>>
{
    public Guid ListId { get; set; }
    public BoardListStatus Status { get; set; }

    public UpdateListStatusCommand(Guid listId, BoardListStatus status)
    {
        ListId = listId;
        Status = status;
    }
}
