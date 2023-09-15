using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Extended Identity user for CMS custom needs
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Board> Boards { get; set; } // Boards created by the user
        public List<UserBoard> AccessBoards { get; set; } // Boards that has access to
        public List<Comment> Comments { get; set; }
        public List<UserCard> AccessCards { get; set; }
        public List<CardActivity> CardActivities { get; set; }
    }
}
