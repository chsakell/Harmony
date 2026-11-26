namespace Harmony.Application.UnitTests.Mapping
{
    /// <summary>
    /// Destination members that AutoMapper leaves unmapped today: 28 members across 12 type maps,
    /// recorded on 2026-11-26. Remove an entry when its member gets mapped; never add one to make
    /// a new mapping pass. This list goes away with AutoMapper (R-14).
    /// </summary>
    public static class UnmappedMemberBaseline
    {
        public static readonly IReadOnlyList<(string TypeMap, string Member)> Entries =
        [
            ("UserResponse -> UserWorkspaceResponse", "IsMember"),
            ("Board -> GetBoardResponse", "ActiveSprints"),
            ("Board -> GetBoardResponse", "SelectedSprint"),
            ("BoardList -> BoardListDto", "TitleEditing"),
            ("Card -> CardDto", "BoardKey"),
            ("Card -> CardDto", "IsUpdating"),
            ("Card -> CardDto", "Hidden"),
            ("Card -> GetBacklogItemResponse", "BoardKey"),
            ("Card -> GetBacklogItemResponse", "SerialKey"),
            ("CardDto -> CardCreatedMessage", "BoardId"),
            ("CardDto -> CardCreatedMessage", "Card"),
            ("CardDto -> CardCreatedMessage", "UserId"),
            ("Card -> LoadCardResponse", "IssueTypes"),
            ("Card -> LoadCardResponse", "BoardLists"),
            ("Label -> LabelDto", "IsChecked"),
            ("CardLabel -> LabelDto", "Id"),
            ("CardLabel -> LabelDto", "Colour"),
            ("CardLabel -> LabelDto", "IsChecked"),
            ("CardActivity -> CardActivityDto", "Actor"),
            ("UserCard -> CardMemberDto", "FirstName"),
            ("UserCard -> CardMemberDto", "LastName"),
            ("UserCard -> CardMemberDto", "UserName"),
            ("UserCard -> CardMemberDto", "ProfilePicture"),
            ("Link -> LinkDetailsDto", "SourceCardBoard"),
            ("Link -> LinkDetailsDto", "SourceCardSerialKey"),
            ("Link -> LinkDetailsDto", "TargetCardBoard"),
            ("Link -> LinkDetailsDto", "TargetCardTitle"),
            ("Link -> LinkDetailsDto", "TargetCardSerialKey"),
        ];
    }
}
