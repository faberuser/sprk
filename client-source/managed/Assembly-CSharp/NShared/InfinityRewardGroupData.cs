using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class InfinityRewardGroupData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int[] RewardIndices { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
