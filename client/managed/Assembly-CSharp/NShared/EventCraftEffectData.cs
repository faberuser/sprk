using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EventCraftEffectData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ItemIndex { get; set; }

	int IDataKeySelector<int>.Key => ItemIndex;

	public override string ToString()
	{
		return "EventCraftData Index=" + Index;
	}
}
