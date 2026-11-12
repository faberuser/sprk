using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipEnchantOptionItemData : IItemSubData, IDataKeySelector<string>
{
	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int ReqCount { get; set; }

	[Key(2)]
	public int ReqGold { get; set; }

	[Key(3)]
	public int[] EnchantOptionIndex1 { get; set; }

	[Key(4)]
	public int[] EnchantOptionIndex2 { get; set; }

	[Key(5)]
	public int[] EnchantOptionIndex3 { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "itemCode=" + ItemCode;
	}
}
