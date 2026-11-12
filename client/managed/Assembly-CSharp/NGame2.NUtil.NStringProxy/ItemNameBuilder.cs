using NShared;
using NVespa.NGlobal;

namespace NGame2.NUtil.NStringProxy;

public class ItemNameBuilder : IBuilder
{
	private ItemData _itemData;

	private EquipItemInfo _equipItemInfo;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public ItemNameBuilder(ItemData itemData, EquipItemInfo equipItemInfo, bool appendLine)
	{
		_itemData = itemData;
		_equipItemInfo = equipItemInfo;
		_appendLine = appendLine;
	}

	public string Build()
	{
		return GetItemName(_itemData, _equipItemInfo);
	}

	public static string GetItemName(ItemData itemData, EquipItemInfo equipItemInfo = null, bool isAppendIndex = true)
	{
		if (itemData == null)
		{
			return string.Empty;
		}
		int level = equipItemInfo?.Level ?? 0;
		string itemName = itemData.Name;
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
			level = 0;
			itemName = LocalizationManager.GetStringDirect("SOULWEAPON_NAME_" + equipItemInfo.ItemIndex);
		}
		return GetItemName(itemName, itemData.Grade, level, itemData.Index, isAppendIndex);
	}

	public static string GetItemName(string itemName, int grade, int level = 0, int index = 0, bool isAppendIndex = true)
	{
		ItemGradeData data = GameRoot.Instance.ItemGradeDataContainer.GetData(grade);
		string text = "FFFFFF";
		if (data != null)
		{
			text = data.NameColor;
		}
		if (!string.IsNullOrEmpty(text))
		{
			itemName = "[" + text + "]" + itemName + "[-]";
		}
		return itemName;
	}

	public static string GetBoosterItemName(BoosterItemData boosterItemData, EquipItemInfo equipItemInfo = null)
	{
		if (boosterItemData == null)
		{
			return string.Empty;
		}
		string text = LocalizationManager.GetStringDirect(boosterItemData.Name);
		if (equipItemInfo != null && equipItemInfo.Level > 0)
		{
			text = text + " [FFFFFF]+" + equipItemInfo.Level + "[-]";
		}
		return text;
	}
}
