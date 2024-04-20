using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.SourceControl
{
    public class RepositoryUser
    {
        public RepositoryUser(string login, string avatarUrl, string htmlUrl)
        {
            Login = login;
            AvatarUrl = avatarUrl;
            HtmlUrl = htmlUrl;
        }

        public string Login { get; set; }
        public string AvatarUrl { get; set; }
        public string HtmlUrl { get; set; }
    }
}
