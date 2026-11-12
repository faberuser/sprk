using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NHelper.NEquip;

public class AutoEquip
{
	private CachedHeroInfo _heroInfo;

	private BetterEquipChecker _betterEquipChecker;

	public readonly List<int> PartIndexList;

	public readonly List<int> EquipItemSlotIndexList;

	public readonly List<int> ItemIndexList;

	public AutoEquip(CachedHeroInfo heroInfo)
	{
		_heroInfo = heroInfo;
		_betterEquipChecker = new BetterEquipChecker();
		PartIndexList = new List<int>();
		EquipItemSlotIndexList = new List<int>();
		ItemIndexList = new List<int>();
	}

	public bool HasEquipment()
	{
		return __Extension_Method_ICollection__.COUNT(EquipItemSlotIndexList) > 0;
	}

	public void CollectEquipment()
	{
		if (_heroInfo == null)
		{
			VespaDebug.LogError("heroInfo is null");
			return;
		}
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			if (!_heroInfo.IsEquipSlotLocked(allEquipSlotDatum.Index))
			{
				EquipItemInfo bestEquip = GetBestEquip(allEquipSlotDatum);
				if (bestEquip != null && (!bestEquip.IsTechnoMagic || bestEquip.IsIdentified))
				{
					PartIndexList.Add(allEquipSlotDatum.Index);
					EquipItemSlotIndexList.Add(bestEquip.SlotIndex);
					ItemIndexList.Add(bestEquip.ItemIndex);
				}
			}
		}
	}

	public EquipItemInfo GetBestEquip(EquipSlotData equipSlotData)
	{
		if (_heroInfo == null)
		{
			return null;
		}
		EquipItemInfo equipItemInfoByPartIndex = _heroInfo.GetEquipItemInfoByPartIndex(equipSlotData.Index);
		long num = equipItemInfoByPartIndex?.GetEquipItemScore(_heroInfo.CreatureData) ?? 0;
		EquipItemInfo equipItemInfo = null;
		long num2 = 0L;
		foreach (EquipItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager)
		{
			if (_betterEquipChecker.CheckValid(_heroInfo, equipSlotData, item, PartIndexList) && (!item.IsTechnoMagic || item.IsIdentified))
			{
				long equipItemScore = item.GetEquipItemScore(_heroInfo.CreatureData);
				if ((equipItemInfoByPartIndex == null || equipItemScore > num) && (EquipItemSlotIndexList == null || !EquipItemSlotIndexList.Contains(item.SlotIndex)) && (ItemIndexList == null || !ItemIndexList.Contains(item.ItemIndex)) && (equipItemInfo == null || equipItemScore > num2))
				{
					equipItemInfo = item;
					num2 = equipItemScore;
				}
			}
		}
		return equipItemInfo;
	}
}
