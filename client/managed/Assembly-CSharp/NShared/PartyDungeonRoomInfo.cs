namespace NShared;

[JsonSerializable(Javascript = true)]
public class PartyDungeonRoomInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => RoomNo;

	public long RoomNo { get; set; }

	public PartyDungeonType DungeonType { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public byte Opened { get; set; }

	public long MasterAccountId { get; set; }

	public string MasterNick { get; set; }

	public byte CurPartyMember { get; set; }

	public bool IsAutoRoot { get; set; }
}
