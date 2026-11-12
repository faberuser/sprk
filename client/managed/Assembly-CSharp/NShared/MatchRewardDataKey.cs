namespace NShared;

public class MatchRewardDataKey : Tuple<int, int>
{
	public int TierIndex => Item1;

	public int MinRank => Item2;

	public MatchRewardDataKey()
		: base(0, 0)
	{
	}

	public MatchRewardDataKey(int tierIndex, int minRank)
		: base(tierIndex, minRank)
	{
	}
}
