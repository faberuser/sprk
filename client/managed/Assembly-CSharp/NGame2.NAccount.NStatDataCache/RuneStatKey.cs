using NShared;

namespace NGame2.NAccount.NStatDataCache;

public struct RuneStatKey
{
	public int RunePage { get; set; }

	public int ItemIndex { get; set; }

	public int SlotNum { get; set; }

	public int EquipSlotIndex { get; set; }

	public RuneStatKey(HeroRunePageInfo runePageInfo)
	{
		if (runePageInfo != null)
		{
			RunePage = runePageInfo.RunePage;
			ItemIndex = runePageInfo.ItemIndex;
			SlotNum = runePageInfo.SlotNum;
			EquipSlotIndex = runePageInfo.EquipItemSlotIndex;
		}
		else
		{
			RunePage = 0;
			ItemIndex = 0;
			SlotNum = 0;
			EquipSlotIndex = 0;
		}
	}

	public static bool IsEquals(RuneStatKey x, RuneStatKey y)
	{
		if (x.RunePage != y.RunePage)
		{
			return false;
		}
		if (x.ItemIndex != y.ItemIndex)
		{
			return false;
		}
		if (x.EquipSlotIndex != y.EquipSlotIndex)
		{
			return false;
		}
		if (x.SlotNum != y.SlotNum)
		{
			return false;
		}
		return true;
	}

	public static int GetHashCode(RuneStatKey obj)
	{
		return (((31 * 17 + obj.RunePage) * 17 + obj.ItemIndex) * 17 + obj.SlotNum) * 17 + obj.EquipSlotIndex;
	}
}
