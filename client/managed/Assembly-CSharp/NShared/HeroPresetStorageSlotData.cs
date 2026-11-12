using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroPresetStorageSlotData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string NameKey { get; set; }

	[Key(2)]
	public bool IsPaid { get; set; }

	[Key(3)]
	public string StorageKey { get; set; }

	[Key(4)]
	public int BuyGem { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
