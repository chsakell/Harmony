﻿using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.MoveToSprint
{
    /// <summary>
    /// Command to move items to a sprint
    /// </summary>
    public class MoveToSprintCommand : IRequest<IResult<List<CardDto>>>
    {
        public Guid BoardId { get; set; }
        public Guid SprintId { get; set; }
        public Guid BoardListId { get; set; }
        public List<Guid> Cards { get; set; }

        public MoveToSprintCommand(Guid boardId, Guid sprintId, 
            Guid boardListId, List<Guid> cards)
        {
            BoardId = boardId;
            SprintId = sprintId;
            BoardListId = boardListId;
            Cards = cards;
        }
    }
}