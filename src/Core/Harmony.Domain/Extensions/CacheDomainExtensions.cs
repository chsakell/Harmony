using Harmony.Domain.Entities;
using System.Text.Json;

namespace Harmony.Domain.Extensions
{
    public static class CacheDomainExtensions
    {
        public static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        public static Dictionary<string, string> ToDictionary(this Board board)
        {
            var dictionary = new Dictionary<string, string>();

            var details = new
            {
                board.Id,
                board.Title,
                board.Key,
                board.WorkspaceId,
                board.Visibility
            };

            var workspace = new
            {
                board.Workspace.Id,
                board.Workspace.Name,
                board.Workspace.Status,
                board.Workspace.IsPublic
            };

            dictionary.Add($"board-details-{board.Id}", JsonSerializer.Serialize(details, _jsonSerializerOptions));
            dictionary.Add($"board-workspace-{board.Id}", JsonSerializer.Serialize(workspace, _jsonSerializerOptions));
            dictionary.Add($"board-lists-{board.Id}", board.SerializeLists());
            dictionary.Add($"board-labels-{board.Id}", board.SerializeLabels());
            dictionary.Add($"board-issue-types-{board.Id}", board.SerializeIssueTypes());

            return dictionary;
        }

        public static Board FromDictionary(Guid boardId, Dictionary<string, string> dictionary)
        {
            if(!dictionary.Keys.Any())
            {
                return null;
            }

            var board = new Board();

            if(dictionary.TryGetValue($"board-details-{boardId}", out  var details))
            {
                board = JsonSerializer.Deserialize<Board>(details);
            }

            if (dictionary.TryGetValue($"board-workspace-{boardId}", out var workspace))
            {
                board.Workspace = JsonSerializer.Deserialize<Workspace>(workspace);
            }

            if (dictionary.TryGetValue($"board-lists-{boardId}", out var boardLists))
            {
                var lists = JsonSerializer.Deserialize<List<BoardList>>(boardLists);

                board.Lists = lists;
            }

            if (dictionary.TryGetValue($"board-labels-{boardId}", out var boardLabels))
            {
                var labels = JsonSerializer.Deserialize<List<Label>>(boardLabels);

                board.Labels = labels;
            }

            if (dictionary.TryGetValue($"board-issue-types-{boardId}", out var boardIssueTypes))
            {
                var issueTypes = JsonSerializer.Deserialize<List<IssueType>>(boardIssueTypes);

                board.IssueTypes = issueTypes;
            }

            return board;
        }

        public static string SerializeLists(this Board board)
        {

            var serializedLists = JsonSerializer.Serialize(
                board.Lists.Select(boardList =>
            {
                return new
                {
                    boardList.Id,
                    boardList.Title,
                    boardList.Position,
                    boardList.Status,
                    boardList.CardStatus
                };
            }), _jsonSerializerOptions);

            return serializedLists;
        }

        public static string SerializeLabels(this Board board)
        {

            var serializedLabels = JsonSerializer.Serialize(
                board.Labels.Select(label =>
                {
                    return new
                    {
                        label.Id,
                        label.Title,
                        label.Colour,
                    };
                }), _jsonSerializerOptions);

            return serializedLabels;
        }

        public static string SerializeIssueTypes(this Board board)
        {

            var serializedIssueTypes = JsonSerializer.Serialize(
                board.IssueTypes.Select(type =>
                {
                    return new
                    {
                        type.Id,
                        type.Summary
                    };
                }), _jsonSerializerOptions);

            return serializedIssueTypes;
        }

        public static string SerializeLists(this List<BoardList> boardLists)
        {

            var serializedLists = JsonSerializer.Serialize(
                boardLists.Select(boardList =>
                {
                    return new
                    {
                        boardList.Id,
                        boardList.Title,
                        boardList.Position,
                        boardList.Status,
                        boardList.CardStatus
                    };
                }), _jsonSerializerOptions);

            return serializedLists;
        }
    }
}
