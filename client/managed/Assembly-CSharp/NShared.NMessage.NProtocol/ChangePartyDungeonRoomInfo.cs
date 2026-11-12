namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class ChangePartyDungeonRoomInfo
{
	public PartyDungeonType DungeonType { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public PartyDungeonDifficultyType Difficulty { get; set; }

	public bool IsResetSquard { get; set; }
}
