﻿using Harmony.Application.DTO;
using Harmony.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class EditableCardModel
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        public string Description { get; set; }
        public string UserId { get; set; } // User created the card
        public EditableBoardListModel BoardList { get; set; }
        public CardStatus Status { get; set; }
        public List<EditableCheckListModel> CheckLists { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public bool UploadingAttachment { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public List<CardActivityDto> Activities { get; set; } = new List<CardActivityDto>();
        public List<CardMemberDto> Members { get; set; }
        public List<LabelDto> Labels { get; set; }
        public SprintDto Sprint { get; set; }
    }

    public class FluentValueValidator<T> : AbstractValidator<T>
    {
        public FluentValueValidator(Action<IRuleBuilderInitial<T, T>> rule)
        {
            rule(RuleFor(x => x));
        }

        private IEnumerable<string> ValidateValue(T arg)
        {
            var result = Validate(arg);
            if (result.IsValid)
                return new string[0];
            return result.Errors.Select(e => e.ErrorMessage);
        }

        public Func<T, IEnumerable<string>> Validation => ValidateValue;
    }
}
