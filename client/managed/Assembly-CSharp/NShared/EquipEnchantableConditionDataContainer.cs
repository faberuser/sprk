using NShared.NJit;

namespace NShared;

public class EquipEnchantableConditionDataContainer : JitDataContainerDefault<EquipEnchantableConditionKey, EquipEnchantableConditionData>
{
	public const string TABLENAME = "EquipEnchantableConditionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipEnchantableConditionTable");

	public EquipEnchantableConditionData GetData(int TierIndex, int DetailIndex)
	{
		return GetData(new EquipEnchantableConditionKey(TierIndex, DetailIndex));
	}

	public bool EnchantOptionItemCodeExists(int TierIndex, int DetailIndex)
	{
		EquipEnchantableConditionData data = GetData(TierIndex, DetailIndex);
		if (data != null && data.EnchantOptionItemCodes != null)
		{
			return data.EnchantOptionItemCodes.Length != 0;
		}
		return false;
	}
}
