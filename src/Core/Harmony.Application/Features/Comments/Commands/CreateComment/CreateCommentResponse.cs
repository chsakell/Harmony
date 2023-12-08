using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Comments.Commands.CreateComment
{
    public class CreateCommentResponse
    {
        public Guid Id { get; set; }
        public UserPublicInfo User { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
