using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Shared.Constants.Application
{
    public class BoardListsConstants
    {
        public static List<BoardList> GetDefaultLists(string userId)
        {
            return new List<BoardList>
                {
                    new BoardList()
                    {
                        Title = "TODO",
                        Position = 0,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.TODO
                    },
                    new BoardList()
                    {
                        Title = "IN PROGRESS",
                        Position = 1,
                        UserId = userId,
                        CardStatus= BoardListCardStatus.IN_PROGRESS
                    },
                    new BoardList()
                    {
                        Title = "DONE",
                        Position = 2,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.DONE
                    }
                };
        }
        public static List<BoardList> GetRetrospectiveLists(RetrospectiveType type, string userId)
        => type switch
        {
            RetrospectiveType.WentWell_ToImprove_ActionItems => new List<BoardList>
                {
                    new BoardList()
                    {
                        Title = "WENT WELL",
                        Position = 0,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.TODO
                    },
                    new BoardList()
                    {
                        Title = "TO IMPROVE",
                        Position = 1,
                        UserId = userId,
                        CardStatus= BoardListCardStatus.TODO
                    },
                    new BoardList()
                    {
                        Title = "ACTION ITEMS",
                        Position = 2,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.TODO
                    }
                },
            RetrospectiveType.Start_Stop_Continue => new List<BoardList>
                {
                    new BoardList()
                    {
                        Title = "START",
                        Position = 0,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.TODO
                    },
                    new BoardList()
                    {
                        Title = "STOP",
                        Position = 1,
                        UserId = userId,
                        CardStatus= BoardListCardStatus.TODO
                    },
                    new BoardList()
                    {
                        Title = "CONTINUE",
                        Position = 2,
                        UserId = userId,
                        CardStatus = BoardListCardStatus.TODO
                    }
                },
            _ => throw new ArgumentOutOfRangeException("Retrospective type not implemented")
        };
    }
}
