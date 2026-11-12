namespace NShared;

public class RaidMatchInfo : MatchInfo
{
	public RaidData RaidData;

	public PartyDungeonData PartyDungeonData;

	public new int DungeonDifficulty;

	public long BossHp;

	public override int GetDungeonDifficulty()
	{
		return DungeonDifficulty;
	}
}
