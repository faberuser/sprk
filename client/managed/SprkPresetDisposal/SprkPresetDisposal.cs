using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

public static class SprkPresetDisposal
{
	public static bool Clear(int[] slots, string[] uids, int[] pages, int removedSlot)
	{
		bool result = false;
		if (slots == null || removedSlot <= 0)
		{
			return false;
		}
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i] == removedSlot)
			{
				slots[i] = 0;
				if (uids != null && i < uids.Length)
				{
					uids[i] = "";
				}
				if (pages != null && i < pages.Length)
				{
					pages[i] = 0;
				}
				result = true;
			}
		}
		return result;
	}

	public static bool AfterRemove(bool removed, int slot)
	{
		if (!removed || slot <= 0 || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return removed;
		}
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		NGame2.NAccount.UserEquipPresetStorageManager equipPresetSotrageManger = instance.EquipPresetSotrageManger;
		if (equipPresetSotrageManger != null)
		{
			EquipPresetInfo[] array = equipPresetSotrageManger.ToArray();
			foreach (EquipPresetInfo equipPresetInfo in array)
			{
				if (equipPresetInfo != null && Clear(equipPresetInfo.EquipItemSlotIndex, equipPresetInfo.EquipItemUid, null, slot))
				{
					equipPresetSotrageManger.Apply(equipPresetInfo);
				}
			}
		}
		NGame2.NAccount.UserHeroPresetStorageManager heroPresetStorageManager = instance.HeroPresetStorageManager;
		if (heroPresetStorageManager != null)
		{
			HeroPresetStorageInfo[] array2 = heroPresetStorageManager.ToArray();
			foreach (HeroPresetStorageInfo heroPresetStorageInfo in array2)
			{
				bool flag = false;
				if (heroPresetStorageInfo == null || heroPresetStorageInfo.HeroPresetInfos == null)
				{
					continue;
				}
				HeroPresetInfo[] heroPresetInfos = heroPresetStorageInfo.HeroPresetInfos;
				foreach (HeroPresetInfo heroPresetInfo in heroPresetInfos)
				{
					if (heroPresetInfo != null)
					{
						flag |= Clear(heroPresetInfo.EquipItemSlotIndex, heroPresetInfo.EquipItemUid, heroPresetInfo.EquipItemRunePage, slot);
					}
				}
				if (flag)
				{
					heroPresetStorageManager.Apply(heroPresetStorageInfo);
				}
			}
		}
		return removed;
	}
}
