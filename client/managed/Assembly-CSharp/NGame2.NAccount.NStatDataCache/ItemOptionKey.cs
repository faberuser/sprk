using NShared;

namespace NGame2.NAccount.NStatDataCache;

public struct ItemOptionKey
{
	public int OptionIndex1 { get; private set; }

	public int OptionIndex2 { get; private set; }

	public int OptionIndex3 { get; private set; }

	public int OptionIndex4 { get; private set; }

	public int OptionIndex5 { get; private set; }

	public int OptionStep1 { get; private set; }

	public int OptionStep2 { get; private set; }

	public int OptionStep3 { get; private set; }

	public int OptionStep4 { get; private set; }

	public int OptionStep5 { get; private set; }

	public int RuneSlotCount { get; private set; }

	public int RuneItemIndex1 { get; private set; }

	public int RuneItemIndex2 { get; private set; }

	public int RuneItemIndex3 { get; private set; }

	public int EnchantOptionIndex1 { get; set; }

	public int EnchantOptionIndex2 { get; set; }

	public int EnchantOptionIndex3 { get; set; }

	public int EnchantOptionStep1 { get; set; }

	public int EnchantOptionStep2 { get; set; }

	public int EnchantOptionStep3 { get; set; }

	public ItemOptionKey(EquipItemInfo info)
	{
		OptionIndex1 = info.OptionIndex1;
		OptionIndex2 = info.OptionIndex2;
		OptionIndex3 = info.OptionIndex3;
		OptionIndex4 = info.OptionIndex4;
		OptionIndex5 = info.OptionIndex5;
		OptionStep1 = info.OptionStep1;
		OptionStep2 = info.OptionStep2;
		OptionStep3 = info.OptionStep3;
		OptionStep4 = info.OptionStep4;
		OptionStep5 = info.OptionStep5;
		RuneSlotCount = info.RuneSlotCount;
		RuneItemIndex1 = info.RuneItemIndex1;
		RuneItemIndex2 = info.RuneItemIndex2;
		RuneItemIndex3 = info.RuneItemIndex3;
		EnchantOptionIndex1 = info.EnchantOptionIndex1;
		EnchantOptionIndex2 = info.EnchantOptionIndex2;
		EnchantOptionIndex3 = info.EnchantOptionIndex3;
		EnchantOptionStep1 = info.EnchantOptionStep1;
		EnchantOptionStep2 = info.EnchantOptionStep2;
		EnchantOptionStep3 = info.EnchantOptionStep3;
	}

	public static bool IsEquals(ItemOptionKey x, ItemOptionKey y)
	{
		if (x.OptionIndex1 != y.OptionIndex1)
		{
			return false;
		}
		if (x.OptionIndex2 != y.OptionIndex2)
		{
			return false;
		}
		if (x.OptionIndex3 != y.OptionIndex3)
		{
			return false;
		}
		if (x.OptionIndex4 != y.OptionIndex4)
		{
			return false;
		}
		if (x.OptionIndex5 != y.OptionIndex5)
		{
			return false;
		}
		if (x.OptionStep1 != y.OptionStep1)
		{
			return false;
		}
		if (x.OptionStep2 != y.OptionStep2)
		{
			return false;
		}
		if (x.OptionStep3 != y.OptionStep3)
		{
			return false;
		}
		if (x.OptionStep4 != y.OptionStep4)
		{
			return false;
		}
		if (x.OptionStep5 != y.OptionStep5)
		{
			return false;
		}
		if (x.RuneSlotCount != y.RuneSlotCount)
		{
			return false;
		}
		if (x.RuneItemIndex1 != y.RuneItemIndex1)
		{
			return false;
		}
		if (x.RuneItemIndex2 != y.RuneItemIndex2)
		{
			return false;
		}
		if (x.RuneItemIndex3 != y.RuneItemIndex3)
		{
			return false;
		}
		if (x.EnchantOptionIndex1 != y.EnchantOptionIndex1)
		{
			return false;
		}
		if (x.EnchantOptionIndex2 != y.EnchantOptionIndex2)
		{
			return false;
		}
		if (x.EnchantOptionIndex3 != y.EnchantOptionIndex3)
		{
			return false;
		}
		if (x.EnchantOptionStep1 != y.EnchantOptionStep1)
		{
			return false;
		}
		if (x.EnchantOptionStep2 != y.EnchantOptionStep2)
		{
			return false;
		}
		if (x.EnchantOptionStep3 != y.EnchantOptionStep3)
		{
			return false;
		}
		return true;
	}

	public static int GetHashCode(ItemOptionKey obj)
	{
		return (((((((((((((((((((31 * 17 + obj.OptionIndex1) * 17 + obj.OptionIndex2) * 17 + obj.OptionIndex3) * 17 + obj.OptionIndex4) * 17 + obj.OptionIndex5) * 17 + obj.OptionStep1) * 17 + obj.OptionStep2) * 17 + obj.OptionStep3) * 17 + obj.OptionStep4) * 17 + obj.OptionStep5) * 17 + obj.RuneSlotCount) * 17 + obj.RuneItemIndex1) * 17 + obj.RuneItemIndex2) * 17 + obj.RuneItemIndex3) * 17 + obj.EnchantOptionIndex1) * 17 + obj.EnchantOptionIndex2) * 17 + obj.EnchantOptionIndex3) * 17 + obj.EnchantOptionStep1) * 17 + obj.EnchantOptionStep2) * 17 + obj.EnchantOptionStep3;
	}
}
