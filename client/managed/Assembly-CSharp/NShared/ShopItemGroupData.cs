using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShopItemGroupData : IDataKeySelector<ShopItemGroupKey>
{
	[Key(0)]
	public int ShopIndex { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public string GroupNameKey { get; set; }

	[Key(3)]
	public bool IsDefault { get; set; }

	ShopItemGroupKey IDataKeySelector<ShopItemGroupKey>.Key => new ShopItemGroupKey(ShopIndex, GroupIndex);

	public override string ToString()
	{
		return "GroupIndex=" + GroupIndex;
	}
}
