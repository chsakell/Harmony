using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class SprintDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Goal { get; set; }
        public Guid BoardId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SprintStatus Status { get; set; }
    }
}
