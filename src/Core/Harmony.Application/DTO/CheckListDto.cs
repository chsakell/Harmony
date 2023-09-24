using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class CheckListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid CardId { get; set; }
        public List<CheckListItemDto> Items { get; set; }
        public byte Position { get; set; }

        // helper for add new items
        public CheckListItemDto NewItem { get; set; }
    }
}
