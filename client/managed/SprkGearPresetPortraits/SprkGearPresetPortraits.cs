using System.Collections.Generic;
using NGame2;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NSingleton;

public static class SprkGearPresetPortraits
{
	public static int FindWearer(EquipPresetInfo preset, int selectedHeroIndex)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || PresetStorageHelper.IsEmptySlotEquipPresetInfo(preset))
		{
			return 0;
		}
		if (PresetStorageHelper.IsEquippedItemExist(preset, selectedHeroIndex))
		{
			return selectedHeroIndex;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		EquipPartType[] saveEquipParts = PresetStorageHelper.SaveEquipParts;
		foreach (EquipPartType partIndex in saveEquipParts)
		{
			string equipItemUid = preset.GetEquipItemUid((int)partIndex);
			if (string.IsNullOrEmpty(equipItemUid))
			{
				continue;
			}
			EquipItemInfo equipItemInfoByUid = instance.EquipItemManager.GetEquipItemInfoByUid(equipItemUid);
			if (equipItemInfoByUid != null && equipItemInfoByUid.GetInventoryType() != InventoryType.Chest)
			{
				int equippedHeroIndex = equipItemInfoByUid.GetEquippedHeroIndex();
				if (equippedHeroIndex > 0 && instance.GetHeroInfo(equippedHeroIndex) != null && PresetStorageHelper.IsEquippedItemExist(preset, equippedHeroIndex))
				{
					dictionary.TryGetValue(equippedHeroIndex, out var value);
					dictionary[equippedHeroIndex] = value + 1;
				}
			}
		}
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			if (item.Value > num2 || (item.Value == num2 && item.Key < num))
			{
				num = item.Key;
				num2 = item.Value;
			}
		}
		return num;
	}

	public static void Update(EquipStorageSlotComponent slot, int selectedHeroIndex)
	{
		int num = FindWearer(slot.EquipPresetInfo, selectedHeroIndex);
		UIHelper.EnableGameObject(slot.BundleLoader_HeroTexture, num > 0);
		if (num > 0)
		{
			slot.SetPortraitTexture(num);
		}
	}
}
