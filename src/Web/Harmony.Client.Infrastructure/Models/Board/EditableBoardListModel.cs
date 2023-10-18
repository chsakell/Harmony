using Harmony.Domain.Enums;

namespace Harmony.Client.Infrastructure.Models.Board
{
    public class EditableBoardListModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte Position { get; set; } // position on the board
        //public List<CardDto> Cards { get; set; }

        // helpers for kanban
        public string NewCardName { get; set; }
        public bool NewTaskOpen { get; set; }
        public BoardListStatus Status { get; set; }
    }
}
