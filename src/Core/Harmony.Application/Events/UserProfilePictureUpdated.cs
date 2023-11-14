using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class UserProfilePictureUpdated
    {
        public UserProfilePictureUpdated(string profilePicture)
        {
            ProfilePicture = profilePicture;
        }

        public string ProfilePicture { get; set; }
    }
}
