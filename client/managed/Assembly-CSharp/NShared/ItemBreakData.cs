using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ItemBreakData : IDataKeySelector<int>
{
	[Key(0)]
	public int ItemIndex { get; set; }

	[Key(1)]
	public int RewardIndex { get; set; }

	int IDataKeySelector<int>.Key => ItemIndex;
}
