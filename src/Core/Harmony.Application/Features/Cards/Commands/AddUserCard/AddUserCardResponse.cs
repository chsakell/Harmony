using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Cards.Commands.AddUserCard
{
    public class AddUserCardResponse 
    {
        public AddUserCardResponse(Guid cardId, string userId)
        {
            CardId = cardId;
            UserId = userId;
        }

        public Guid CardId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
