using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Events
{
    public class BoardCreatedEvent
    {
        public BoardCreatedEvent(string workspaceId, Guid boardId, string title, string description, BoardVisibility visibility)
        {
            WorkspaceId = workspaceId;
            BoardId = boardId;
            Title = title;
            Description = description;
            Visibility = visibility;
        }

        public string WorkspaceId { get; set; }
        public Guid BoardId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BoardVisibility Visibility { get; set; }
    }
}
