using System.Collections.Generic;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class MatchInfo
{
	public long Uid { get; set; }

	public MatchArenaType ArenaType { get; set; }

	public int SeasonIndex { get; set; }

	public MatchedAccountInfo[] AccountInfos { get; set; }

	public string BattleServerAddress { get; set; }

	public int BattleServerPort { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public DungeonDifficultyType DungeonDifficulty { get; set; }

	public int OnlineGameSpeedRatio { get; set; }

	public BattleInstanceExtraData ExtraData { get; set; }

	public bool CanCheat { get; set; }

	public long[] GetAccountIds()
	{
		if (AccountInfos == null)
		{
			return null;
		}
		List<long> list = new List<long>();
		MatchedAccountInfo[] accountInfos = AccountInfos;
		foreach (MatchedAccountInfo matchedAccountInfo in accountInfos)
		{
			list.Add(matchedAccountInfo.AccountId);
		}
		return list.ToArray();
	}

	public virtual int GetDungeonDifficulty()
	{
		return (int)DungeonDifficulty;
	}
}
