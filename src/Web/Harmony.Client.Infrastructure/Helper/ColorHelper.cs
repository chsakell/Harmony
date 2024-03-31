using Harmony.Domain.Enums;
using Harmony.Shared.Constants.Application;
using MudBlazor;

namespace Harmony.Client.Infrastructure.Helper
{
    public class ColorHelper
    {
        public static Color GetVisibilityColor(BoardVisibility visibility)
        {
            return visibility switch
            {
                BoardVisibility.Private => Color.Error,
                BoardVisibility.Public => Color.Success,
                BoardVisibility.Workspace => Color.Info,
                _ => Color.Info,
            };
        }

        public static Color GetBoardTypeColor(BoardType type)
        {
            return type switch
            {
                BoardType.Kanban => Color.Info,
                BoardType.Scrum => Color.Tertiary,
                _ => Color.Info,
            };
        }

        public static Color GetIssueTypeColor(string type)
        {
            return type switch
            {
                IssueTypesConstants.EPIC => Color.Surface,
                IssueTypesConstants.BUG => Color.Error,
                IssueTypesConstants.STORY => Color.Success,
                IssueTypesConstants.TASK => Color.Info,
                _ => Color.Info,
            };
        }

        public static string GetIssueTypeIcon(string type)
        {
            return type switch
            {
                IssueTypesConstants.EPIC => Icons.Material.Outlined.ElectricBolt,
                IssueTypesConstants.BUG => Icons.Material.Outlined.BugReport,
                IssueTypesConstants.STORY => Icons.Material.Outlined.LabelImportant,
                IssueTypesConstants.TASK => Icons.Material.Outlined.CheckBox,
                IssueTypesConstants.NEUTRAL => "fa-solid fa-face-meh-blank",
                IssueTypesConstants.LIKE => "fa-solid fa-thumbs-up",
                IssueTypesConstants.LOVE => "fa-solid fa-heart",
                IssueTypesConstants.SAD => "fa-solid fa-face-frown",
                IssueTypesConstants.ANGRY => "fa-solid fa-face-angry",
                _ => Icons.Material.Outlined.Task,
            };
        }
    }
}
