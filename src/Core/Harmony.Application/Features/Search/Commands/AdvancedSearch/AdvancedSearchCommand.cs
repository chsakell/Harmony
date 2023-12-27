using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Search.Commands.AdvancedSearch
{
    public class AdvancedSearchCommand : IRequest<IResult<List<SearchableCard>>>
    {
        [Required]
        [MinLength(5)]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        public List<string> AssignedMembers { get; set; }

        public Guid BoardId { get; set; }

        public Guid ListId { get; set; }

        public bool HasAttachments { get; set; }

        public CardStatus CardStatus { get; set; }

        public DateTime DueDate { get; set; }
    }
}
