using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MazeClearRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ClearFloor { get; set; }

	[Key(2)]
	public int RewardIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "MazeClearRewardData Index=" + Index;
	}
}
