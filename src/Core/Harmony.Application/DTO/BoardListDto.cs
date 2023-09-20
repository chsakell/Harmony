﻿using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO
{
	public class BoardListDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string NewCardName { get; set; }
		public byte Position { get; set; } // position on the board
		public List<CardDto> Cards { get; set; }
	}
}
