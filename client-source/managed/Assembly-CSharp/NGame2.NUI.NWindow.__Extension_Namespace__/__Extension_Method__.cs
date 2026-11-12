using NShared;

namespace NGame2.NUI.NWindow.__Extension_Namespace__;

public static class __Extension_Method__
{
	public static bool EXT_IsDefault(this EquipFilterOptionData This)
	{
		return This?.IsDefault ?? false;
	}

	public static bool EXT_GetChangeOnStartValue(this EquipFilterOptionData This, EquipmentItemFilter.Mode mode)
	{
		if (This == null)
		{
			return false;
		}
		bool result = This.IsDefault;
		switch (mode)
		{
		case EquipmentItemFilter.Mode.Inventory:
			result = This.IsDefault;
			break;
		case EquipmentItemFilter.Mode.HeroList:
			result = This.ChangeOnOpenInHeroEquip;
			break;
		}
		return result;
	}

	public static object EXT_GetItemDataByDataTypeName(this EquipItemInfo This, string typeName)
	{
		object result = null;
		switch (typeName)
		{
		case "NShared.EquipItemInfo":
			result = This;
			break;
		case "NShared.EquipItemData":
			result = This.EquipItemData;
			break;
		case "NShared.ItemData":
			result = GameRoot.Instance.ItemDataContainer.GetData(This.ItemIndex);
			break;
		case "NShared.EquipItemDetailData":
			result = GameRoot.Instance.EquipItemDetailDataContainer.GetData(This.EquipItemData.DetailIndex);
			break;
		}
		return result;
	}
}
