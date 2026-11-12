using System.Collections.Generic;
using NVespa;

namespace NShared;

public class BattleMetaDataContainer
{
	private const int EQUIPTYPE_DICT_IS_NULL = -1;

	public Dictionary<EquipItemEquipType, int> EquipTypeCountDict { get; private set; }

	public BattleCreature Owner { get; private set; }

	public BattleMetaDataContainer()
	{
		EquipTypeCountDict = new Dictionary<EquipItemEquipType, int>();
	}

	public void Init(BattleCreature owner)
	{
		Owner = owner;
	}

	public void InitEquipTypeCount()
	{
		if (EquipTypeCountDict == null)
		{
			VespaDebug.LogError("EquipTypeCountDict is null.");
			return;
		}
		if (Owner == null || Owner.CreateInfo == null)
		{
			VespaDebug.LogError("Container owner is empty.");
			return;
		}
		CachedHeroInfo heroInfo = Owner.CreateInfo.HeroInfo;
		if (heroInfo == null)
		{
			VespaDebug.LogError("Container owner is empty.");
			return;
		}
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			EquipItemInfo equipItemInfoByPartIndex = heroInfo.GetEquipItemInfoByPartIndex(allEquipSlotDatum.Index);
			if (equipItemInfoByPartIndex != null && equipItemInfoByPartIndex.EquipItemData != null)
			{
				EquipItemEquipType equipType = equipItemInfoByPartIndex.EquipItemData.EquipType;
				IncreaseEquipTypeCount(equipType);
			}
		}
	}

	private void IncreaseEquipTypeCount(EquipItemEquipType equipType)
	{
		if (!EquipTypeCountDict.ContainsKey(equipType))
		{
			EquipTypeCountDict.Add(equipType, 0);
		}
		EquipTypeCountDict[equipType]++;
	}

	public int GetEquipTypeCount(EquipItemEquipType equipType)
	{
		if (EquipTypeCountDict == null)
		{
			return -1;
		}
		int value = 0;
		EquipTypeCountDict.TryGetValue(equipType, out value);
		return value;
	}
}
