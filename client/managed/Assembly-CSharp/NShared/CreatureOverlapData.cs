using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureOverlapData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int OverlapRewardIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
