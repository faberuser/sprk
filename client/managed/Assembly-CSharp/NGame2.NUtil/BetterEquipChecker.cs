using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil;

public class BetterEquipChecker
{
	private static List<EquipSlotData> EquipSlotDataList;

	private static bool _initialized;

	private Dictionary<int, CachedHeroInfo> _heroOfEquipItemSlotDic;

	static BetterEquipChecker()
	{
		EquipSlotDataList = new List<EquipSlotData>();
		Init();
	}

	private static void Init()
	{
		if (_initialized || !GameRoot.isValidInstance)
		{
			return;
		}
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			EquipSlotDataList.Add(allEquipSlotDatum);
		}
		_initialized = true;
	}

	public bool Exist(CachedHeroInfo heroInfo)
	{
		for (int i = 0; i < EquipSlotDataList.Count; i++)
		{
			EquipSlotData equipSlotData = EquipSlotDataList[i];
			if (!heroInfo.IsEquipSlotLocked(equipSlotData.Index) && GetBetterEquipItemInfo(heroInfo, equipSlotData) != null)
			{
				return true;
			}
		}
		return false;
	}

	public EquipItemInfo GetBetterEquipItemInfo(CachedHeroInfo heroInfo, EquipSlotData equipSlotData, List<int> duplicateCheckIgnoreSlotIndices = null)
	{
		if (heroInfo == null)
		{
			return null;
		}
		EquipItemInfo equipItemInfoByPartIndex = heroInfo.GetEquipItemInfoByPartIndex(equipSlotData.Index);
		long num = equipItemInfoByPartIndex?.GetEquipItemScore(heroInfo.CreatureData) ?? 0;
		foreach (EquipItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager)
		{
			if (CheckValid(heroInfo, equipSlotData, item, duplicateCheckIgnoreSlotIndices))
			{
				long equipItemScore = item.GetEquipItemScore(heroInfo.CreatureData);
				if (equipItemInfoByPartIndex == null || equipItemScore > num)
				{
					return item;
				}
			}
		}
		return null;
	}

	public bool CheckValid(CachedHeroInfo heroInfo, EquipSlotData equipSlotData, EquipItemInfo toEquipItemInfo, List<int> duplicateCheckIgnoreSlotIndices = null)
	{
		string duplicationItemname = string.Empty;
		EquipItemInfo equipItemInfoByPartIndex = heroInfo.GetEquipItemInfoByPartIndex(equipSlotData.Index);
		if (equipItemInfoByPartIndex == toEquipItemInfo)
		{
			return false;
		}
		if (toEquipItemInfo.GetInventoryType() == InventoryType.Chest)
		{
			return false;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(toEquipItemInfo.ItemIndex);
		if (data == null)
		{
			return false;
		}
		if (data.SubData.PartType != equipSlotData.EquipPartType)
		{
			return false;
		}
		if (GetHeroOfEquipItem(toEquipItemInfo.SlotIndex) != null)
		{
			return false;
		}
		if (!equipSlotData.IsValidSubType(data.SubData.SubType))
		{
			return false;
		}
		if (equipItemInfoByPartIndex != null && equipSlotData.EquipPartType == EquipPartType.Accessory)
		{
			ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfoByPartIndex.ItemIndex);
			if (data2 != null && data2.SubData.SubType != data.SubData.SubType)
			{
				return false;
			}
		}
		if (heroInfo.CheckEquipResult(data.SubData) != CachedHeroInfo.EquipResult.Success)
		{
			return false;
		}
		if (EquipSlotHelper.CanEquip(toEquipItemInfo, equipSlotData.Index, heroInfo, ref duplicationItemname, duplicateCheckIgnoreSlotIndices) != EquipSlotHelper.CanEquipResult.CanEquip)
		{
			return false;
		}
		return true;
	}

	private CachedHeroInfo GetHeroOfEquipItem(int equipItemSlotIndex)
	{
		if (_heroOfEquipItemSlotDic == null)
		{
			_heroOfEquipItemSlotDic = new Dictionary<int, CachedHeroInfo>();
			foreach (CachedHeroInfo item in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
			{
				for (int i = 0; i < EquipSlotDataList.Count; i++)
				{
					int index = EquipSlotDataList[i].Index;
					int equipItemSlotIndex2 = item.GetEquipItemSlotIndex(index);
					if (equipItemSlotIndex2 > 0)
					{
						if (_heroOfEquipItemSlotDic.ContainsKey(equipItemSlotIndex2))
						{
							CachedHeroInfo vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_heroOfEquipItemSlotDic, equipItemSlotIndex2, null);
							VespaDebug.LogError($"Already Added: Slot={equipItemSlotIndex2}, PartIndex={index}, OldHero={vALUE.HeroIndex}, NewHero={item.HeroIndex}");
						}
						else
						{
							_heroOfEquipItemSlotDic.Add(equipItemSlotIndex2, item);
						}
					}
				}
			}
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_heroOfEquipItemSlotDic, equipItemSlotIndex, null);
	}
}
