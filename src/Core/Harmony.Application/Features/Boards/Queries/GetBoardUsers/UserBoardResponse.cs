using Harmony.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Queries.GetBoardUsers
{
    public class UserBoardResponse : UserResponse
    {
        public bool IsMember { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
