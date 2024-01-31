﻿using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.ReactivateCards
{
    /// <summary>
    /// Command to move items to a sprint
    /// </summary>
    public class ReactivateCardsCommand : IRequest<IResult<List<CardDto>>>
    {
        public Guid BoardId { get; set; }
        public Guid BoardListId { get; set; }
        public List<Guid> Cards { get; set; }

        public ReactivateCardsCommand(Guid boardId, Guid boardListId, List<Guid> cards)
        {
            BoardId = boardId;
            BoardListId = boardListId;
            Cards = cards;
        }
    }
}
