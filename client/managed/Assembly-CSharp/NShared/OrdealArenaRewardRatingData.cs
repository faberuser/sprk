using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class OrdealArenaRewardRatingData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public MatchTierType TierType { get; set; }

	[Key(2)]
	public int[] RatingArrange { get; set; }

	[Key(3)]
	public int RewardIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public int GetMinRating()
	{
		return ArgParser.GetValueInt(RatingArrange, 0);
	}

	public int GetMaxRating()
	{
		return ArgParser.GetValueInt(RatingArrange, 1);
	}
}
