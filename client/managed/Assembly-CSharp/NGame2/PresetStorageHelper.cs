using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2;

public static class PresetStorageHelper
{
	public static readonly EquipPartType[] SaveEquipParts = new EquipPartType[5]
	{
		EquipPartType.Armor,
		EquipPartType.SecondGear,
		EquipPartType.Accessory,
		EquipPartType.Orb,
		EquipPartType.Artifact
	};

	public static int GetEquippedItemExsitSlotIndex(int heroIndex)
	{
		foreach (EquipPresetInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger)
		{
			if (IsEquippedItemExist(item, heroIndex))
			{
				return item.EquipStorageSlotIndex;
			}
		}
		return -1;
	}

	public static bool IsEquippedItemExist(EquipPresetInfo equipPresetInfo, int heroIndex)
	{
		if (IsEmptySlotEquipPresetInfo(equipPresetInfo))
		{
			return false;
		}
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo == null)
		{
			return false;
		}
		EquipPartType[] saveEquipParts = SaveEquipParts;
		foreach (EquipPartType equipPartType in saveEquipParts)
		{
			string equipItemUid = equipPresetInfo.GetEquipItemUid((int)equipPartType);
			EquipItemInfo equipItemInfoByPartType = heroInfo.GetEquipItemInfoByPartType(equipPartType);
			if (equipItemInfoByPartType != null && equipItemInfoByPartType.Uid == equipItemUid)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsEmptySlotEquipPresetInfo(EquipPresetInfo equipPresetInfo)
	{
		if (equipPresetInfo == null)
		{
			return true;
		}
		if (equipPresetInfo.IsEmptySlot())
		{
			return true;
		}
		EquipPartType[] saveEquipParts = SaveEquipParts;
		foreach (EquipPartType partIndex in saveEquipParts)
		{
			string equipItemUid = equipPresetInfo.GetEquipItemUid((int)partIndex);
			if (!String_Extension_Method.IsNullOrEmpty(equipItemUid))
			{
				EquipItemInfo equipItemInfoByUid = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemInfoByUid(equipItemUid);
				if (equipItemInfoByUid != null && equipItemInfoByUid.GetInventoryType() != InventoryType.Chest)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void ApplyUnEquipItems(int[] equipItemSlotIndices)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equipItemSlotIndices); i++)
		{
			int num = equipItemSlotIndices[i];
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(num);
			if (equipItemInfo == null)
			{
				continue;
			}
			int equippedHeroIndex = equipItemInfo.GetEquippedHeroIndex();
			if (equippedHeroIndex != -1)
			{
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(equippedHeroIndex);
				int partIndexOfEquipItem = heroInfo.GetPartIndexOfEquipItem(num);
				if (partIndexOfEquipItem == 0)
				{
					VespaDebug.LogError("Part index not found : equipItemSlotIndex=" + num + " HeroIndex=" + equippedHeroIndex);
					continue;
				}
				heroInfo.SetEquipItemInfo_InMainThread(partIndexOfEquipItem, null);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(equipItemInfo, equippedHeroIndex, 0);
			}
			equipItemInfo.SetEquippedHeroIndex_InMainThread(-1);
		}
	}

	public static void ApplyHeroPresetStorage(HeroInfo[] heroInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfos); i++)
		{
			ApplyHeroPresetStorage(heroInfos[i]);
		}
	}

	private static void ApplyHeroPresetStorage(HeroInfo heroInfo)
	{
		ApplyEquipItemsByHeroInfo(heroInfo);
		ApplyTranscnedSkillPage(heroInfo);
		ApplyRunePage(heroInfo);
	}

	private static void ApplyTranscnedSkillPage(HeroInfo heroInfo)
	{
		if (heroInfo != null && heroInfo.HeroIndex != 0)
		{
			CachedHeroInfo heroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroInfo.HeroIndex);
			if (heroInfo2 == null)
			{
				VespaDebug.LogError("[SetHeroStarogeSlot]Not Match Hero :" + heroInfo.HeroIndex);
			}
			else
			{
				heroInfo2.SetTranscendSkillPage(heroInfo.SkillPage);
			}
		}
	}

	public static void ApplyEquipPresetStorage(HeroInfo heroInfo)
	{
		ApplyEquipItemsByHeroInfo(heroInfo);
	}

	private static void ApplyRunePage(HeroInfo heroInfo)
	{
		if (heroInfo != null && heroInfo.HeroIndex != 0)
		{
			CachedHeroInfo heroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroInfo.HeroIndex);
			if (heroInfo2 == null)
			{
				VespaDebug.LogError("[ApplyRunePageStorage]Not Match Hero :" + heroInfo.HeroIndex);
			}
			else
			{
				heroInfo2.ApplyRunePage = heroInfo.ApplyRunePage;
			}
		}
	}

	private static void ApplyEquipItemsByHeroInfo(HeroInfo heroInfo)
	{
		if (heroInfo == null || heroInfo.HeroIndex == 0)
		{
			return;
		}
		CachedHeroInfo heroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroInfo.HeroIndex);
		if (heroInfo2 == null)
		{
			VespaDebug.LogError("[SetHeroStarogeSlot]Not Match Hero :" + heroInfo.HeroIndex);
			return;
		}
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			int index = allEquipSlotDatum.Index;
			if (heroInfo2.IsEquipSlotLocked(index))
			{
				continue;
			}
			int equipItemSlotIndex = heroInfo.GetEquipItemSlotIndex(index);
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
			if (equipItemInfo != null)
			{
				if (equipItemInfo.GetInventoryType() == InventoryType.Chest)
				{
					continue;
				}
				int equippedHeroIndex = equipItemInfo.GetEquippedHeroIndex();
				equippedHeroIndex = ((equippedHeroIndex != -1) ? equippedHeroIndex : 0);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(equipItemInfo, equippedHeroIndex, heroInfo.HeroIndex);
			}
			heroInfo2.SetEquipItemInfo_InMainThread(index, equipItemInfo);
		}
	}
}
