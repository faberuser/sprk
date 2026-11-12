using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MatchBanPickSeasonRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int MinRank { get; set; }

	[Key(2)]
	public int MaxRank { get; set; }

	[Key(3)]
	public int Percentage { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "[Index=" + Index + "]";
	}
}
