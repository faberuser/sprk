using System;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount.NExtension;

public static class EquipItemInfo_Extension
{
	public static long GetEquipItemScore(this EquipItemInfo This, CreatureData creature)
	{
		return This?.EquipScore.GetEquipItemScore(creature) ?? 0;
	}

	public static bool IsEquipStorageSlot(this EquipItemInfo This)
	{
		if (This.GetEquipPresetInfo() == null)
		{
			return false;
		}
		return true;
	}

	public static EquipPresetInfo GetEquipPresetInfo(this EquipItemInfo This)
	{
		if (This == null)
		{
			return null;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(This.ItemIndex);
		if (data == null)
		{
			return null;
		}
		if (Array.IndexOf(EquipStorageSlotInfo.SaveEquipParts, data.SubData.PartType) < 0)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.GetConatinStorageSlotInfo(This.Uid, (int)data.SubData.PartType);
	}

	public static void LinkSoulWeaponInfo(this EquipItemInfo This)
	{
		if (This != null && AccountManager.IsLogined())
		{
			SoulWeaponInfo soulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Get(This.SlotIndex);
			if (soulWeaponInfo != null && soulWeaponInfo.ItemIndex == This.ItemIndex)
			{
				This.SetSoulWeaponInfo(soulWeaponInfo);
			}
		}
	}
}
