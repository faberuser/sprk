namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class PartyMemberDungeonInfoRes
{
	public bool CanJoin { get; set; }

	public long RoomNo { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
