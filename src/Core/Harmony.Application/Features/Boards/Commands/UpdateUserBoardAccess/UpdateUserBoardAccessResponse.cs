using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess
{
    public class UpdateUserBoardAccessResponse
    {
        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public UserBoardAccess Access { get; set; }
    }
}
