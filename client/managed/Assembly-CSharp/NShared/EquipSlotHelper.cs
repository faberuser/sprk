using System;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public static class EquipSlotHelper
{
	public enum CanEquipResult
	{
		NullData,
		SubtypeDuplication,
		CanEquip
	}

	private static EquipSlotData[] _allSlotData;

	public static IEnumerable<EquipSlotData> GetAllEquipSlotData()
	{
		return GameRoot.Instance.EquipSlotDataContainer.GetAllData();
	}

	public static int EquipPartTypeToEquipSlotIndex(EquipPartType partType)
	{
		int equipSlotIndexFor = GameRoot.Instance.EquipSlotDataContainer.GetEquipSlotIndexFor(partType);
		VespaDebug.LogError("equipSlotIndex: " + equipSlotIndexFor);
		return equipSlotIndexFor;
	}

	public static bool IsValidSlot(int equipSlotIndex, EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("equipItemInfo is null");
			return false;
		}
		EquipSlotData data = GameRoot.Instance.EquipSlotDataContainer.GetData(equipSlotIndex);
		if (data == null)
		{
			VespaDebug.LogError("EquipSlotData is null : index " + equipSlotIndex);
			return false;
		}
		ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("EquipItemData is null : index " + equipItemInfo.ItemIndex);
			return false;
		}
		if (!IsValidEquipItem(data, data2.SubData))
		{
			return false;
		}
		return true;
	}

	private static bool IsValidEquipItem(EquipSlotData equipSlotData, EquipItemData equipItemData)
	{
		if (equipSlotData == null || equipItemData == null)
		{
			VespaDebug.LogError("Data is null");
			return false;
		}
		if (equipSlotData.EquipPartType != equipItemData.PartType)
		{
			return false;
		}
		if (equipSlotData.EquipItemSubType != null && !Array.Exists(equipSlotData.EquipItemSubType, (EquipItemSubType element) => element == equipItemData.SubType))
		{
			return false;
		}
		return true;
	}

	public static CanEquipResult CanEquip(EquipItemInfo equipItemInfo, int equipSloIndex, CachedHeroInfo cachedHero, ref string duplicationItemname, List<int> igenoreSlotIndices = null)
	{
		EquipSlotData data = GameRoot.Instance.EquipSlotDataContainer.GetData(equipSloIndex);
		if (equipItemInfo == null || data == null || cachedHero == null)
		{
			VespaDebug.LogError("Data is null");
			return CanEquipResult.NullData;
		}
		int[] equipSlotIndicesFor = GameRoot.Instance.EquipSlotDataContainer.GetEquipSlotIndicesFor(data.EquipPartType);
		if (equipSlotIndicesFor == null)
		{
			VespaDebug.LogError("indices is null");
			return CanEquipResult.NullData;
		}
		for (int i = 0; i < equipSlotIndicesFor.Length; i++)
		{
			if ((igenoreSlotIndices != null && igenoreSlotIndices.Contains(equipSlotIndicesFor[i])) || equipSlotIndicesFor[i] == data.Index || GameRoot.Instance.EquipSlotDataContainer.GetData(equipSlotIndicesFor[i]).GroupIndex != data.GroupIndex)
			{
				continue;
			}
			EquipItemInfo equipItemInfoByPartIndex = cachedHero.GetEquipItemInfoByPartIndex(equipSlotIndicesFor[i]);
			if (equipItemInfoByPartIndex == null)
			{
				continue;
			}
			ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfoByPartIndex.ItemIndex);
			ItemData<EquipItemData> data3 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
			if (data2 != null)
			{
				if (data3 == null)
				{
					return CanEquipResult.NullData;
				}
				if (data2.SubData.SubType == data3.SubData.SubType)
				{
					duplicationItemname = data2.Name;
					return CanEquipResult.SubtypeDuplication;
				}
			}
		}
		return CanEquipResult.CanEquip;
	}
}
