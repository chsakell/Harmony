﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
    public class CardCommentsDto
    {
        public Guid CardId { get; set; }
        public int TotalComments { get; set; }
    }
}
