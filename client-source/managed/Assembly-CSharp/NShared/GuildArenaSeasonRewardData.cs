using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GuildArenaSeasonRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int TierIndex { get; set; }

	[Key(2)]
	public GuildArenaTierType ShowTierUI { get; set; }

	[Key(3)]
	public int ShowScoreUI { get; set; }

	[Key(4)]
	public int MinRank { get; set; }

	[Key(5)]
	public int MaxRank { get; set; }

	[Key(6)]
	public int SeasonPointReward { get; set; }

	[Key(7)]
	public int SeasonRubyReward { get; set; }

	[Key(8)]
	public int FreeSeasonPointReward { get; set; }

	[Key(9)]
	public int FreeSeasonRubyReward { get; set; }

	[Key(10)]
	public string FlagCode { get; set; }

	[Key(11)]
	public int[] RewardItemIndices { get; set; }

	[Key(12)]
	public int[] RewardItemCounts { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index + " (" + ShowTierUI.ToString() + ")";
	}
}
