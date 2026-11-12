using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceEnchantData : IDataKeySelector<ValanceEnchantDataKey>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public string PartType { get; set; }

	[Key(2)]
	public string SubType { get; set; }

	[Key(3)]
	public int ReqGold { get; set; }

	[Key(4)]
	public int OptionGroupIndex { get; set; }

	ValanceEnchantDataKey IDataKeySelector<ValanceEnchantDataKey>.Key => new ValanceEnchantDataKey(ItemCode, PartType, SubType);
}
