namespace NShared;

[JsonSerializable(Javascript = true)]
public class RaidRoomInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => RoomNo;

	public long RoomNo { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int RaidMonsterLevel { get; set; }

	public byte GoalIndex { get; set; }

	public PartyDungeonType DungeonType { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public PartyDungeonDifficultyType DungeonDifficulty { get; set; }

	public byte Opened { get; set; }

	public long MasterAccountId { get; set; }

	public string MasterNick { get; set; }

	public byte CurPartyMember { get; set; }

	public bool IsAutoRoot { get; set; }

	public int[] AffixIndices { get; set; }
}
