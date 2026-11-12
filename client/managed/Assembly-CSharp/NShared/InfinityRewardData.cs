using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class InfinityRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public RewardType RewardType { get; set; }

	[Key(2)]
	public int ItemIndex { get; set; }

	[Key(3)]
	public bool IsRatio { get; set; }

	[Key(4)]
	public int InfinityIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
