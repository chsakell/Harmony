using Harmony.Application.Responses;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Queries.SearchBoardUsers
{
    public class SearchBoardUserResponse : UserResponse
    {
        public bool IsMember { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public UserBoardAccess? Access { get; set; }
    }
}
