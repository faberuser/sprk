using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipEnchantableConditionData : IDataKeySelector<EquipEnchantableConditionKey>
{
	[Key(0)]
	public int TierIndex { get; set; }

	[Key(1)]
	public int DetailIndex { get; set; }

	[Key(2)]
	public string[] EnchantOptionItemCodes { get; set; }

	EquipEnchantableConditionKey IDataKeySelector<EquipEnchantableConditionKey>.Key => new EquipEnchantableConditionKey(TierIndex, DetailIndex);

	public override string ToString()
	{
		return "TierIndex=" + TierIndex + ", DetailIndex=" + DetailIndex;
	}
}
