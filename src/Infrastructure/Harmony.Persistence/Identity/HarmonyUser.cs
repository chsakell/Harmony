using Harmony.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Harmony.Persistence.Identity
{
    /// <summary>
    /// Extended Identity user for CMS custom needs
    /// </summary>
    public class HarmonyUser : IdentityUser, IAuditableEntity<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsActive { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public List<Board> Boards { get; set; } // Boards created by the user
        public List<UserBoard> AccessBoards { get; set; } // Boards that has access to
        public List<Comment> Comments { get; set; }
        public List<UserCard> AccessCards { get; set; }
        public List<CardActivity> CardActivities { get; set; }
    }
}
