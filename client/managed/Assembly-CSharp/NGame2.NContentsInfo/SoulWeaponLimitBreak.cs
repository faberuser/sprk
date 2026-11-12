using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NShared;
using NShared.NContentsDefine;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NContentsInfo;

public class SoulWeaponLimitBreak : SingletonAuto<SoulWeaponLimitBreak>
{
	public int GetSoulStoneIndex(EquipItemInfo targetInfo)
	{
		if (!AccountManager.IsLogin())
		{
			return 0;
		}
		return ItemRelation.GetHeroRelation_by_UniqueWeaponItemIndex(targetInfo.ItemIndex).SoulStoneItemIndex;
	}

	public bool CanSelect_LimitBreakTarget(EquipItemInfo targetInfo)
	{
		if (!AccountManager.IsLogin())
		{
			return false;
		}
		if (targetInfo == null)
		{
			return false;
		}
		_ = targetInfo.EquipItemData.DetailIndex;
		if (ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(targetInfo) == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Get(targetInfo.SlotIndex) == null)
		{
			return false;
		}
		return true;
	}

	public bool CanUse_as_Material(EquipItemInfo materialInfo, EquipItemInfo targetInfo)
	{
		if (materialInfo.SlotIndex == targetInfo.SlotIndex)
		{
			return false;
		}
		if (materialInfo.ItemIndex != targetInfo.ItemIndex)
		{
			return false;
		}
		if (materialInfo.IsEquipped)
		{
			return false;
		}
		if (materialInfo.Locked == 1)
		{
			return false;
		}
		return true;
	}

	public bool CanRequest_LimitBreak(EquipItemInfo targetInfo, List<EquipItemInfo> mainMaterialList)
	{
		if (!CanSelect_LimitBreakTarget(targetInfo))
		{
			return false;
		}
		NShared.NContentsDefine.SoulWeaponLimitBreak.LimitBreakData afterDataByCurrent = ContentsDefine.SoulWeaponLimitBreak.GetAfterDataByCurrent(targetInfo);
		if (afterDataByCurrent == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(mainMaterialList) != afterDataByCurrent.WeaponUniqueCount)
		{
			return false;
		}
		foreach (EquipItemInfo mainMaterial in mainMaterialList)
		{
			if (!CanUse_as_Material(mainMaterial, targetInfo))
			{
				return false;
			}
		}
		int soulStoneIndex = GetSoulStoneIndex(targetInfo);
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(soulStoneIndex) < afterDataByCurrent.SoulStoneCount)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(afterDataByCurrent.TransStoneIndex) < afterDataByCurrent.TransStoneCount)
		{
			return false;
		}
		return true;
	}
}
