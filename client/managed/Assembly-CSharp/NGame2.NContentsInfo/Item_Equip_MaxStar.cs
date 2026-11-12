using NShared;

namespace NGame2.NContentsInfo;

public class Item_Equip_MaxStar
{
	public static bool Is_LimitBreaked(EquipItemInfo targetInfo)
	{
		if (targetInfo == null)
		{
			return false;
		}
		int maxStar_Awaken = GetMaxStar_Awaken(targetInfo.ItemData);
		return targetInfo.Star > maxStar_Awaken;
	}

	public static bool CanUse_as_AwakenMaterial(EquipItemInfo info)
	{
		if (info != null && info.Locked != 0)
		{
			return false;
		}
		if (info == null)
		{
			return false;
		}
		if (info.IsSoulWeapon())
		{
			return false;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(info.ItemIndex);
		if (data == null)
		{
			return false;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (data2 == null)
		{
			return false;
		}
		int reqAwakeMaterialLevel = data2.ReqAwakeMaterialLevel;
		return info.Level >= reqAwakeMaterialLevel;
	}

	public static bool CanAwaken(EquipItemInfo info)
	{
		int maxStar_Awaken = GetMaxStar_Awaken(info.ItemData);
		return info.Star < maxStar_Awaken;
	}

	public static bool CanTierUpgrade(EquipItemInfo info)
	{
		int maxStar_Awaken = GetMaxStar_Awaken(info.ItemData);
		return info.Star == maxStar_Awaken;
	}

	public static bool CanTechnoAwaken(EquipItemInfo info)
	{
		if (info.IsTechnoMagic && info.IsIdentified)
		{
			return info.CLI_CanAwaken();
		}
		return false;
	}

	public static bool CanTechnoTierUpgrade(EquipItemInfo info)
	{
		int maxStar_Awaken = GetMaxStar_Awaken(info.ItemData);
		if (info.IsTechnoMagic && info.IsIdentified && !info.IsAdvanced)
		{
			return info.Star == maxStar_Awaken;
		}
		return false;
	}

	public static int GetMaxStar_Available(ItemData itemData)
	{
		if (TryGetMaxStar_LimitBreak(itemData, out var maxStar))
		{
			return maxStar;
		}
		return GetMaxStar_Awaken(itemData);
	}

	public static int GetMaxStar_Awaken(ItemData itemData)
	{
		if (itemData == null)
		{
			return 0;
		}
		EquipItemData subData = itemData.GetSubData<EquipItemData>();
		if (subData == null)
		{
			return 0;
		}
		return GameRoot.Instance.EquipItemDetailDataContainer.GetData(subData.DetailIndex)?.MaxStar ?? 0;
	}

	public static bool TryGetMaxStar_LimitBreak(ItemData itemData, out int maxStar)
	{
		EquipItemData subData = itemData.GetSubData<EquipItemData>();
		if (subData == null)
		{
			maxStar = 0;
			return false;
		}
		if (ContentsDefine.SoulWeaponLimitBreak.TryGetMaxStar(subData.DetailIndex, out maxStar))
		{
			return true;
		}
		maxStar = 0;
		return false;
	}
}
