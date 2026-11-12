using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MatchRewardData : IDataKeySelector<MatchRewardDataKey2>
{
	MatchRewardDataKey2 IDataKeySelector<MatchRewardDataKey2>.Key => new MatchRewardDataKey2(TierIndex, MinRank);

	[Key(0)]
	public int TierIndex { get; set; }

	[Key(1)]
	public int MinRank { get; set; }

	[Key(2)]
	public int MaxRank { get; set; }

	[Key(3)]
	public int DailyReward { get; set; }

	[Key(4)]
	public int SeasonReward { get; set; }

	[Key(5)]
	public int FirstReward { get; set; }

	[Key(6)]
	public int ShowMinRank { get; set; }

	[Key(7)]
	public int ShowMaxRank { get; set; }

	[Key(8)]
	public string DailyRewardExpItemCode { get; set; }

	[Key(9)]
	public int DailyRewardExpItemCount { get; set; }

	[Key(10)]
	public string WeeklyRewardExpItemCode { get; set; }

	[Key(11)]
	public int WeeklyRewardExpItemCount { get; set; }

	[Key(12)]
	public string SeasonRewardExpItemCode { get; set; }

	[Key(13)]
	public int SeasonRewardExpItemCount { get; set; }

	public override string ToString()
	{
		return "TierIndex=" + TierIndex + " (MinRank=" + MinRank + ", MaxRank=" + MaxRank + ")";
	}

	public int GetShowMinRank()
	{
		if (ShowMinRank != 0)
		{
			return ShowMinRank;
		}
		return MinRank;
	}

	public int GetShowMaxRank()
	{
		if (ShowMaxRank != 0)
		{
			return ShowMaxRank;
		}
		return MaxRank;
	}
}
