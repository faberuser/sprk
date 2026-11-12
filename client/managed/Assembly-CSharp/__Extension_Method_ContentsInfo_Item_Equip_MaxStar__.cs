using NGame2.NContentsInfo;
using NShared;

public static class __Extension_Method_ContentsInfo_Item_Equip_MaxStar__
{
	public static bool CLI_IsLimitBreaked(this EquipItemInfo This)
	{
		if (This == null)
		{
			return false;
		}
		return Item_Equip_MaxStar.Is_LimitBreaked(This);
	}

	public static int CLI_GetMaxStar_for_Awaken(this EquipItemInfo This)
	{
		if (This == null)
		{
			return 0;
		}
		return Item_Equip_MaxStar.GetMaxStar_Awaken(This.ItemData);
	}

	public static bool CLI_CanUse_as_AwakenMaterial(this EquipItemInfo This)
	{
		return Item_Equip_MaxStar.CanUse_as_AwakenMaterial(This);
	}

	public static bool CLI_CanAwaken(this EquipItemInfo This)
	{
		return Item_Equip_MaxStar.CanAwaken(This);
	}

	public static bool CLI_CanTierUpgrade(this EquipItemInfo This)
	{
		return Item_Equip_MaxStar.CanTierUpgrade(This);
	}

	public static bool CLI_CanTechnoAwaken(this EquipItemInfo This)
	{
		return Item_Equip_MaxStar.CanTechnoAwaken(This);
	}

	public static bool CLI_CanTechnoTierUpgrade(this EquipItemInfo This)
	{
		return Item_Equip_MaxStar.CanTechnoTierUpgrade(This);
	}
}
