using Harmony.Application.Features.Boards.Commands.CreateList;
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

		#region Kanban

		private List<KanBanList> _kanbanLists = new();
		private List<KanbanListCard> _kanbanCards = new();
		#endregion

		protected async override Task OnInitializedAsync()
		{
			var result = await _boardManager.GetBoardAsync(Id);

			if (result.Succeeded)
			{
				_board = result.Data;

				foreach (var list in _board.Lists)
				{
					_kanbanLists.Add(new KanBanList(list.Name, false, string.Empty));

					foreach(var card in list.Cards)
					{
						_kanbanCards.Add(new KanbanListCard(card.Name, list.Name));
					}
				}
			}
		}

		#region helper
		private MudDropContainer<KanbanListCard> _dropContainer;

		private bool _addSectionOpen;
		/* handling board events */
		private void TaskUpdated(MudItemDropInfo<KanbanListCard> info)
		{
			info.Item.Status = info.DropzoneIdentifier;
		}

		public class KanBanList
		{
			public string Name { get; init; }
			public bool NewTaskOpen { get; set; }
			public string NewTaskName { get; set; }

			public KanBanList(string name, bool newTaskOpen, string newTaskName)
			{
				Name = name;
				NewTaskOpen = newTaskOpen;
				NewTaskName = newTaskName;
			}
		}
		public class KanbanListCard
		{
			public string Name { get; init; }
			public string Status { get; set; }

			public KanbanListCard(string name, string status)
			{
				Name = name;
				Status = status;
			}
		}

		KanBanNewForm newListModel = new KanBanNewForm();

		public class KanBanNewForm
		{
			[Required]
			[StringLength(10, ErrorMessage = "Name length can't be more than 10.")]
			public string Name { get; set; }
		}

		private async Task OnValidListSubmit(EditContext context)
		{
			var result = await _boardManager.CreateListAsync(new CreateListCommand(newListModel.Name, Guid.Parse(Id)));

			if(result.Succeeded)
			{

			}

			_kanbanLists.Add(new KanBanList(newListModel.Name, false, String.Empty));
			newListModel.Name = string.Empty;
			_addSectionOpen = false;
		}

		private void OpenAddNewList()
		{
			_addSectionOpen = true;
		}

		private void AddTask(KanBanList section)
		{
			_kanbanCards.Add(new KanbanListCard(section.NewTaskName, section.Name));
			section.NewTaskName = string.Empty;
			section.NewTaskOpen = false;
			_dropContainer.Refresh();
		}

		private void DeleteSection(KanBanList section)
		{
			if (_kanbanLists.Count == 1)
			{
				_kanbanCards.Clear();
				_kanbanLists.Clear();
			}
			else
			{
				int newIndex = _kanbanLists.IndexOf(section) - 1;
				if (newIndex < 0)
				{
					newIndex = 0;
				}

				_kanbanLists.Remove(section);

				var tasks = _kanbanCards.Where(x => x.Status == section.Name);
				foreach (var item in tasks)
				{
					item.Status = _kanbanLists[newIndex].Name;
				}
			}
		}
		#endregion
	}
}
