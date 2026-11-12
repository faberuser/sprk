using NGame2.NDispatchBattle;
using NShared;

namespace NGame2.NContentsInfo;

public class Item_EquipItemInfo
{
	public static bool IsLocked(EquipItemInfo info)
	{
		if (info == null)
		{
			return false;
		}
		return info.Locked != 0;
	}

	public static bool IsSavedInChest(EquipItemInfo info)
	{
		if (info == null)
		{
			return false;
		}
		return info.GetInventoryType() == InventoryType.Chest;
	}

	public static bool IsDispatchedHeroItem(EquipItemInfo info)
	{
		if (info == null)
		{
			return false;
		}
		return DispatchHelper.IsDispatchedHero(info.EquippedHeroInfo);
	}
}
