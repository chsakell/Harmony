using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.Get;
using static Harmony.Application.Notifications.BoardListArchivedMessage;

namespace Harmony.Client.Infrastructure.Store.Kanban
{
    public class KanbanStore : IKanbanStore
    {
        private GetBoardResponse _board = new GetBoardResponse();

        private bool _boardLoading = true;
        public bool BoardLoading => _boardLoading;
        public GetBoardResponse Board => _board;
        public IEnumerable<BoardListDto> KanbanLists => _board.Lists.OrderBy(l => l.Position);
        public IEnumerable<CardDto> KanbanCards => _board.Lists.SelectMany(l => l.Cards).OrderBy(c => c.Position);
        public bool IsScrum => _board != null && _board.Type == Domain.Enums.BoardType.Scrum;

        public void LoadBoard(GetBoardResponse board)
        {
            _board = board;

            _boardLoading = false;
        }

        public void SetLoading(bool loading)
        {
            _boardLoading = loading;
        }

        public void AddListToBoard(BoardListDto list)
        {
            if (!_board.Lists.Any(l => l.Id == list.Id))
            {
                _board.Lists.Add(list);
            }
        }

        public void AddCardToList(CardDto card, BoardListDto list)
        {
            var boardList = _board.Lists.Find(l => l.Id == list.Id);

            if (boardList == null || boardList.Cards.Any(c => c.Id == card.Id))
            {
                return;
            }

            boardList.Cards.Add(card);
        }

        public void MoveCard(CardDto card, Guid previousListId, Guid nextListId, short previousPosition, short newPosition)
        {
            var currentList = _board.Lists.FirstOrDefault(l => l.Id == previousListId);
            var currentCard = currentList?.Cards.FirstOrDefault(c => c.Id == card.Id);

            if (currentCard != null)
            {
                if (previousListId != nextListId)
                {
                    currentList.Cards.Remove(currentCard);

                    var newBoardList = _board.Lists.Find(l => l.Id == card.BoardListId);
                    var cardsInNewBoardListGreaterOrEqualThanIndex = newBoardList.Cards.Where(c => c.Position >= card.Position);

                    // needs increasing by 1
                    foreach (var cardInNewBoardList in cardsInNewBoardListGreaterOrEqualThanIndex)
                    {
                        cardInNewBoardList.Position += 1;
                    }

                    var cardsInPreviousBoardListGreaterOrEqualThanIndex = currentList.Cards.Where(c => c.Position > previousPosition);

                    // needs decreasing by 1
                    foreach (var cardInPreviousBoardList in cardsInPreviousBoardListGreaterOrEqualThanIndex)
                    {
                        cardInPreviousBoardList.Position -= 1;
                    }

                    newBoardList.Cards.Add(card);
                }
                else
                {
                    // needs shifting
                    if (previousPosition != newPosition)
                    {
                        var offSet = newPosition < previousPosition ? 1 : -1;

                        List<CardDto> cardsToReOrder = null;

                        if (offSet == 1)
                        {
                            cardsToReOrder = currentList.Cards
                                .Where(c => c.Position >= newPosition
                                && c.Position < previousPosition 
                                && c.BoardListId == card.BoardListId
                                && c.Id != currentCard.Id).ToList();
                        }
                        else
                        {
                            cardsToReOrder = currentList.Cards
                                .Where(c => c.Position <= newPosition
                                && c.Position > previousPosition 
                                && c.BoardListId == card.BoardListId
                                && c.Id != currentCard.Id).ToList();
                        }

                        foreach (var cardToReorder in cardsToReOrder)
                        {
                            cardToReorder.Position = (short)(cardToReorder.Position + offSet);
                        }
                    }

                    currentCard.Position = newPosition;
                }
            }
        }

        public void ArchiveList(BoardListDto list)
        {
            _board.Lists.RemoveAll(l => l.Id == list.Id);
        }

        public void ArchiveListAndReorder(Guid listId, List<BoardListOrder> listPositions)
        {
            _board.Lists.RemoveAll(l => l.Id == listId);

            foreach (var listPosition in listPositions)
            {
                var list = _board.Lists.FirstOrDefault(l => l.Id == listPosition.Id);

                if (list != null)
                {
                    list.Position = listPosition.Position;
                }
            }
        }

        public void ArchiveCard(Guid cardId)
        {
            var card = _board.Lists.SelectMany(l => l.Cards)
                .FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                var archivedCardPosistion = card.Position;

                var list = _board.Lists.FirstOrDefault(l => l.Id == card.BoardListId);
                if (list != null)
                {
                    list.Cards.Remove(card);
                }

                var listCardsToReposition = list.Cards.Where(c => c.Position > archivedCardPosistion);
                foreach(var listCard in listCardsToReposition)
                {
                    listCard.Position -= 1;
                }
            }
        }

        public void UpdateTodalCardItemsCompleted(Guid cardId, bool increase)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                switch (increase)
                {
                    case true:
                        card.TotalItemsCompleted++;
                        break;
                    case false:
                        card.TotalItemsCompleted--;
                        break;
                }
            }
        }

        public void UpdateTodalCardChildren(Guid cardId, bool increase)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                switch (increase)
                {
                    case true:
                        card.TotalChildren++;
                        break;
                    case false:
                        card.TotalChildren--;
                        break;
                }
            }
        }

        public void UpdateTodalCardItems(Guid cardId, bool increase)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                switch (increase)
                {
                    case true:
                        card.TotalItems++;
                        break;
                    case false:
                        card.TotalItems--;
                        break;
                }
            }
        }

        public void UpdateCardDescription(Guid cardId, string description)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.Description = description;
            }
        }

        public void UpdateCardStoryPoints(Guid cardId, short? storyPoints)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.StoryPoints = storyPoints;
            }
        }

        public void UpdateCardTitle(Guid cardId, string title)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.Title = title;
            }
        }

        public void UpdateCardIssueType(Guid cardId, IssueTypeDto issueType)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.IssueType = issueType;
            }
        }

        public void ToggleCardLabel(Guid cardId, LabelDto label)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);
            var labelExists = card.Labels.Any(l => l.Id == label.Id);
            if (card != null)
            {
                switch (label.IsChecked)
                {
                    case true:
                        if (!labelExists)
                        {
                            card.Labels.Add(label);
                        }
                        break;
                    case false:
                        var labelToRemove = card.Labels.FirstOrDefault(c => c.Id == label.Id);
                        if (labelToRemove != null)
                        {
                            card.Labels.Remove(labelToRemove);
                        }
                        break;
                }
            }
        }

        public void UpdateCardDates(Guid cardId, DateTime? startDate, DateTime? dueDate)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.StartDate = startDate;
                card.DueDate = dueDate;
            }
        }

        public void ChangeTotalCardAttachments(Guid cardId, bool increase)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.TotalAttachments += increase ? +1 : -1;
            }
        }

        public void UpdateBoardListTitle(Guid listId, string title)
        {
            var list = _board.Lists.FirstOrDefault(l => l.Id == listId);

            if (list != null)
            {
                list.Title = title;
            }
        }

        public void UpdateBoardListCards(Guid listId, List<CardDto> cards)
        {
            var list = _board.Lists.FirstOrDefault(l => l.Id == listId);

            if (list != null)
            {
                list.Cards = cards;
            }
        }

        public void RemoveCardLabel(Guid labelId)
        {
            var cards = _board.Lists.SelectMany(l => l.Cards);

            foreach (var card in cards)
            {
                var totalRemoved = card.Labels.RemoveAll(l => l.Id == labelId);
            }
        }

        public void ReorderLists(Dictionary<Guid, short> listPositions)
        {
            foreach (var keyPair in listPositions)
            {
                var list = _board.Lists.FirstOrDefault(l => l.Id == keyPair.Key);

                if (list != null)
                {
                    list.Position = keyPair.Value;
                }
            }
        }

        public void AddCardMember(Guid cardId, CardMemberDto cardMember)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.Members.Add(cardMember);
            }
        }

        public void RemoveCardMember(Guid cardId, CardMemberDto cardMember)
        {
            var card = _board.Lists.SelectMany(l => l.Cards).FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                var memberToRemove = card.Members.FirstOrDefault(l => l.Id == cardMember.Id); ;
                if (memberToRemove != null)
                {
                    card.Members.Remove(memberToRemove);
                }
            }
        }

        public void ReduceCardProgress(Guid cardId, int totalItems, int totalItemsCompleted)
        {
            var card = _board.Lists
                .SelectMany(l => l.Cards)
                .FirstOrDefault(c => c.Id == cardId);

            if (card != null)
            {
                card.TotalItems -= totalItems;
                card.TotalItemsCompleted -= totalItemsCompleted;
            }
        }

        public void Dispose()
        {
            _boardLoading = true;
            _board = new GetBoardResponse();
        }
    }
}
