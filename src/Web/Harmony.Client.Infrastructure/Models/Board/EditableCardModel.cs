using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class EditableCardModel
    {
        [Required]
        [MaxLength(10)]
        public string Title { get; set; }
        public bool TitleEditing { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; } // User created the card
        public EditableBoardListModel BoardList { get; set; }
        public CardStatus Status { get; set; }
        public List<EditableCheckListModel> CheckLists { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
    }
}
