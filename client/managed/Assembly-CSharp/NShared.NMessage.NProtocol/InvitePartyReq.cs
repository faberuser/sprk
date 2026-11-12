namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class InvitePartyReq
{
	public string InvitorName { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public long[] BannedIds { get; set; }

	public long RoomNo { get; set; }

	public bool IsSendOne { get; set; }

	public PartyDungeonType dungeonType { get; set; }

	public int chapterIndex { get; set; }

	public int dungeonIndex { get; set; }

	public PartyDungeonDifficultyType difficulty { get; set; }

	public int[] AffixIndices { get; set; }

	public bool IsRaid()
	{
		if (RaidIndex != 0)
		{
			return RaidLevel != 0;
		}
		return false;
	}

	public bool IsPartyDungeon()
	{
		if (chapterIndex != 0)
		{
			return dungeonIndex != 0;
		}
		return false;
	}
}
