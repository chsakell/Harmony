using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class LabelDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Colour { get; set; }
    }
}
