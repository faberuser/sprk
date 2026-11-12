using NShared;

namespace NGame2.NContentsDefine;

public struct VisualTierRank
{
	private enum ETableType
	{
		MatchReward,
		MatchTier
	}

	public static readonly VisualTierRank NoRank = new VisualTierRank(0);

	public int Value { get; private set; }

	public VisualTierRank(int rank)
	{
		Value = rank;
	}

	public static VisualTierRank CreateFromTableMinRank(MatchRewardData tableData, MatchArenaType arenaType)
	{
		return new VisualTierRank(GetVisualRankFromTable(tableData.GetShowMinRank(), ETableType.MatchReward, arenaType));
	}

	public static VisualTierRank CreateFromTableMaxRank(MatchRewardData tableData, MatchArenaType arenaType)
	{
		return new VisualTierRank(GetVisualRankFromTable(tableData.GetShowMaxRank(), ETableType.MatchReward, arenaType));
	}

	public static VisualTierRank CreateFromTableMinRank(MatchTierData tableData, MatchArenaType arenaType)
	{
		return new VisualTierRank(GetVisualRankFromTable(tableData.MinRank, ETableType.MatchTier, arenaType));
	}

	public static VisualTierRank CreateFromTableMaxRank(MatchTierData tableData, MatchArenaType arenaType)
	{
		return new VisualTierRank(GetVisualRankFromTable(tableData.MaxRank, ETableType.MatchTier, arenaType));
	}

	private static int GetVisualRankFromTable(int tableRank, ETableType tableType, MatchArenaType arenaType)
	{
		return tableRank;
	}

	public static VisualTierRank CreateFromServer(int rank)
	{
		return new VisualTierRank(rank + 1);
	}
}
