using NGame2.NContentsInfo;
using NShared;

public static class __Extension_Method_ContentsInfo_Item_EquipItemInfo__
{
	public static bool CLI_IsLocked(this EquipItemInfo This)
	{
		return Item_EquipItemInfo.IsLocked(This);
	}

	public static bool CLI_IsSavedInChest(this EquipItemInfo This)
	{
		return Item_EquipItemInfo.IsSavedInChest(This);
	}

	public static bool CLI_IsDispatchedHeroItem(this EquipItemInfo This)
	{
		return Item_EquipItemInfo.IsDispatchedHeroItem(This);
	}
}
