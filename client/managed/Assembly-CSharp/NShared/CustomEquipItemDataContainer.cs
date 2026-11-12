using NShared.NJit;

namespace NShared;

public class CustomEquipItemDataContainer : JitDataContainerDefault<int, CustomEquipItemData>
{
	public const string TABLENAME = "CustomEquipItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CustomEquipItemTable");

	public EquipItemInfo GetCustomEquipItem(int customItemIndex, int itemSlotIndex, int forceItemIndex = 0)
	{
		EquipItemInfo equipItemInfo = new EquipItemInfo();
		CustomEquipItemData data = GetData(customItemIndex);
		if (data != null)
		{
			equipItemInfo.ItemIndex = ((forceItemIndex == 0) ? data.ItemIndex : forceItemIndex);
			equipItemInfo.Star = data.Star;
			equipItemInfo.Level = data.Level;
			equipItemInfo.Exp = data.Exp;
			equipItemInfo.OptionIndex1 = data.OptionIndex1;
			equipItemInfo.OptionStep1 = data.OptionStep1;
			equipItemInfo.OptionRenewCount1 = data.OptionRenewCount1;
			equipItemInfo.IsRenewedOption1 = data.IsRenewedOption1;
			equipItemInfo.OptionIndex2 = data.OptionIndex2;
			equipItemInfo.OptionStep2 = data.OptionStep2;
			equipItemInfo.OptionRenewCount2 = data.OptionRenewCount2;
			equipItemInfo.IsRenewedOption2 = data.IsRenewedOption2;
			equipItemInfo.OptionIndex3 = data.OptionIndex3;
			equipItemInfo.OptionStep3 = data.OptionStep3;
			equipItemInfo.OptionRenewCount3 = data.OptionRenewCount3;
			equipItemInfo.IsRenewedOption3 = data.IsRenewedOption3;
			equipItemInfo.OptionIndex4 = data.OptionIndex4;
			equipItemInfo.OptionStep4 = data.OptionStep4;
			equipItemInfo.OptionRenewCount4 = data.OptionRenewCount4;
			equipItemInfo.IsRenewedOption4 = data.IsRenewedOption4;
			equipItemInfo.OptionIndex5 = data.OptionIndex5;
			equipItemInfo.OptionStep5 = data.OptionStep5;
			equipItemInfo.OptionRenewCount5 = data.OptionRenewCount5;
			equipItemInfo.IsRenewedOption5 = data.IsRenewedOption5;
			equipItemInfo.RuneSlotCount = data.RuneSlotCount;
			equipItemInfo.RuneItemIndex1 = data.RuneItemIndex1;
			equipItemInfo.RuneItemIndex2 = data.RuneItemIndex2;
			equipItemInfo.RuneItemIndex3 = data.RuneItemIndex3;
			equipItemInfo.UpgradeStarFailBonus = data.UpgradeStarFailBonus;
			equipItemInfo.Locked = data.Locked;
			equipItemInfo.EnchantOptionIndex1 = data.EnchantOptionIndex1;
			equipItemInfo.EnchantOptionStep1 = data.EnchantOptionStep1;
			equipItemInfo.EnchantOptionIndex2 = data.EnchantOptionIndex2;
			equipItemInfo.EnchantOptionStep2 = data.EnchantOptionStep2;
			equipItemInfo.EnchantOptionIndex3 = data.EnchantOptionIndex3;
			equipItemInfo.EnchantOptionStep3 = data.EnchantOptionStep3;
			equipItemInfo.RenewEnchantOptionSlotIndex = data.RenewEnchantOptionSlotIndex;
			equipItemInfo.SlotIndex = ((itemSlotIndex == 0) ? 9999 : itemSlotIndex);
			return equipItemInfo;
		}
		return null;
	}
}
