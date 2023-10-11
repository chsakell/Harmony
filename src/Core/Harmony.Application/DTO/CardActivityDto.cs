using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class CardActivityDto
    {
        public string Activity { get; set; }
        public Guid CardId { get; set; }
        public CardActivityType Type { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
