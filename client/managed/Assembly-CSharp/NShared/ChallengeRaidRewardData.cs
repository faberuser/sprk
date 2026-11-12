using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ChallengeRaidRewardData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ChallengeRaidIndex { get; set; }

	[Key(2)]
	public int MinRank { get; set; }

	[Key(3)]
	public int MaxRank { get; set; }

	[Key(4)]
	public int Percentage { get; set; }

	[Key(5)]
	public int RewardIndex { get; set; }
}
