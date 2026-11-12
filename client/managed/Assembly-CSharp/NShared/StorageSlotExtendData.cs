using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class StorageSlotExtendData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int SlotCount { get; set; }

	[Key(2)]
	public int ExtendPrice { get; set; }

	[Key(3)]
	public bool IsDefaultOpen { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index + " Slotcount = " + SlotCount;
	}
}
