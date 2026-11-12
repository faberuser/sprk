using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceEnchantOptionCountData : IDataKeySelector<ValanceEnchantOptionCountDataKey>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int Rate { get; set; }

	[Key(2)]
	public int OptionCount { get; set; }

	ValanceEnchantOptionCountDataKey IDataKeySelector<ValanceEnchantOptionCountDataKey>.Key => new ValanceEnchantOptionCountDataKey(ItemCode, OptionCount);
}
