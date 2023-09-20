using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Management
{
	public interface ICardService
	{
		Task<bool> PositionCard(Card card, Guid listId, byte position);
	}
}
