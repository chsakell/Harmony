using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Application.Features.Boards.Queries.GetAllForUser;
using Harmony.Shared.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Client.Pages.Management
{
    public partial class Board
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Name { get; set; }

        private GetBoardResponse _board = new GetBoardResponse();
        protected async override Task OnInitializedAsync()
        {
            var result = await _boardManager.GetBoardAsync(Id);

            if(result.Succeeded)
            {
                _board = result.Data;
            }
        }

        private void NavigateToBoard(GetAllForUserBoardResponse board)
        {
            var slug = StringUtilities.SlugifyString(board.Id.ToString());

            _navigationManager.NavigateTo($"boards/{board.Id}/{slug}");
        }

		#region helper
		private MudDropContainer<KanbanTaskItem> _dropContainer;

		private bool _addSectionOpen;
		/* handling board events */
		private void TaskUpdated(MudItemDropInfo<KanbanTaskItem> info)
		{
			info.Item.Status = info.DropzoneIdentifier;
		}

		/* Setup for board  */
		private List<KanBanSections> _sections = new()
		{
			new KanBanSections("To Do", false, String.Empty),
			new KanBanSections("In Process", false, String.Empty),
			new KanBanSections("Done", false, String.Empty),
		};

		public class KanBanSections
		{
			public string Name { get; init; }
			public bool NewTaskOpen { get; set; }
			public string NewTaskName { get; set; }

			public KanBanSections(string name, bool newTaskOpen, string newTaskName)
			{
				Name = name;
				NewTaskOpen = newTaskOpen;
				NewTaskName = newTaskName;
			}
		}
		public class KanbanTaskItem
		{
			public string Name { get; init; }
			public string Status { get; set; }

			public KanbanTaskItem(string name, string status)
			{
				Name = name;
				Status = status;
			}
		}

		private List<KanbanTaskItem> _tasks = new()
		{
			new KanbanTaskItem("Write unit test", "To Do"),
			new KanbanTaskItem("Some docu stuff", "To Do"),
			new KanbanTaskItem("Walking the dog", "To Do"),
		};

		KanBanNewForm newSectionModel = new KanBanNewForm();

		public class KanBanNewForm
		{
			[Required]
			[StringLength(10, ErrorMessage = "Name length can't be more than 10.")]
			public string Name { get; set; }
		}

		private void OnValidSectionSubmit(EditContext context)
		{
			_sections.Add(new KanBanSections(newSectionModel.Name, false, String.Empty));
			newSectionModel.Name = string.Empty;
			_addSectionOpen = false;
		}

		private void OpenAddNewSection()
		{
			_addSectionOpen = true;
		}

		private void AddTask(KanBanSections section)
		{
			_tasks.Add(new KanbanTaskItem(section.NewTaskName, section.Name));
			section.NewTaskName = string.Empty;
			section.NewTaskOpen = false;
			_dropContainer.Refresh();
		}

		private void DeleteSection(KanBanSections section)
		{
			if (_sections.Count == 1)
			{
				_tasks.Clear();
				_sections.Clear();
			}
			else
			{
				int newIndex = _sections.IndexOf(section) - 1;
				if (newIndex < 0)
				{
					newIndex = 0;
				}

				_sections.Remove(section);

				var tasks = _tasks.Where(x => x.Status == section.Name);
				foreach (var item in tasks)
				{
					item.Status = _sections[newIndex].Name;
				}
			}
		}
		#endregion
	}
}
