using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ItemGridSort : UIGrid.IGridSort
{
	private class ItemSortData
	{
		public Transform tm;

		public ItemIconComponent component;

		public bool EquipItemInfo_IsEquipped;

		public bool EquipItemInfo_IsSavedSlot;

		public short EquipData_Tier;

		public byte ItemData_Grade;

		public EquipPartType EquipData_PartType;

		public EquipItemSubType EquipData_SubType;

		public int ItemData_Index;

		public byte EquipItemInfo_Locked;

		public int MatchedOptionCount;

		public int LowestShowIndexCount;

		public int EquipItemInfo_Star;

		public int EquipItemInfo_Level;

		public DateTime EquipItemInfo_CreateDateTime;

		public EquipItemSetGroupType EquipData_SetGroupType;

		public bool EquipItemInfo_Identified;

		public bool TechnoMagic_Advance;
	}

	private EquipListManager _equipListManager;

	private EquipmentItemFilterManager _equipItemFilterManager;

	private bool _isAscending_IsEquipped;

	private bool _isAscending_IsSavedSlot;

	private bool _isAscending_EquipData_Tier;

	private bool _isAscending_ItemData_Grade;

	private bool _isAscending_EquipData_PartType;

	private bool _isAscending_EquipData_SubType;

	private bool _isAscending_ItemData_Index;

	private bool _isAscending_EquipItemInfo_Locked;

	private bool _isAscending_MatchedOptionCount;

	private bool _isAscending_LowestShowIndexCount;

	private bool _isAscending_EquipItemInfo_Star;

	private bool _isAscending_EquipItemInfo_Level;

	private bool _isAscending_EquipItemInfo_CreateDateTime;

	private bool _isAscending_EquipData_SetGroupType;

	private bool _isAscending_EquipItemInfo_Identified;

	private bool _isAscending_TechnoMagic_Advance;

	public ItemGridSort(EquipListManager equipListManager, EquipmentItemFilterManager equipItemFilterManager)
	{
		_equipListManager = equipListManager;
		_equipItemFilterManager = equipItemFilterManager;
	}

	void UIGrid.IGridSort.Sort(List<Transform> list)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		_isAscending_IsEquipped = false;
		_isAscending_IsSavedSlot = false;
		_isAscending_EquipData_Tier = _equipItemFilterManager.GetSortType(EquipmentItemFilterManager.SortCategory.Tier);
		_isAscending_ItemData_Grade = _equipItemFilterManager.GetSortType(EquipmentItemFilterManager.SortCategory.Grade);
		_isAscending_EquipData_PartType = true;
		_isAscending_EquipData_SubType = true;
		_isAscending_ItemData_Index = true;
		_isAscending_EquipItemInfo_Locked = _equipItemFilterManager.GetSortType(EquipmentItemFilterManager.SortCategory.Lock);
		_isAscending_MatchedOptionCount = false;
		_isAscending_LowestShowIndexCount = false;
		_isAscending_EquipItemInfo_Star = _equipItemFilterManager.GetSortType(EquipmentItemFilterManager.SortCategory.Star);
		_isAscending_EquipItemInfo_Level = false;
		_isAscending_EquipItemInfo_CreateDateTime = true;
		_isAscending_EquipData_SetGroupType = true;
		_isAscending_EquipItemInfo_Identified = true;
		_isAscending_TechnoMagic_Advance = false;
		List<int> checkOptionList = _equipItemFilterManager.DetailOptionFilter.MakeCheckOptionList();
		using PoolingList<ItemSortData> poolingList = default(PoolingList<ItemSortData>);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			Transform tm = list[i];
			ItemSortData itemSortData = new ItemSortData();
			itemSortData.tm = tm;
			itemSortData.component = _equipListManager.GetItemIconComponent(tm);
			if (itemSortData.component == null)
			{
				poolingList.Add(itemSortData);
				continue;
			}
			ItemData itemData = itemSortData.component.ItemData;
			EquipItemInfo equipItemInfo = itemSortData.component.EquipItemInfo;
			EquipItemData equipItemData = itemSortData.component.EquipItemData;
			int matchedDetailOptionCount = _equipItemFilterManager.GetMatchedDetailOptionCount(checkOptionList, equipItemInfo);
			int matchedLowestShowOptionIndexCount = _equipItemFilterManager.GetMatchedLowestShowOptionIndexCount(checkOptionList, equipItemInfo);
			itemSortData.EquipItemInfo_IsEquipped = equipItemInfo.IsEquipped;
			itemSortData.EquipData_Tier = equipItemData.Tier;
			itemSortData.ItemData_Grade = itemData.Grade;
			itemSortData.EquipData_PartType = equipItemData.PartType;
			itemSortData.EquipData_SubType = equipItemData.SubType;
			itemSortData.ItemData_Index = itemData.Index;
			itemSortData.EquipItemInfo_Locked = equipItemInfo.Locked;
			itemSortData.MatchedOptionCount = matchedDetailOptionCount;
			itemSortData.LowestShowIndexCount = matchedLowestShowOptionIndexCount;
			itemSortData.EquipItemInfo_Star = equipItemInfo.Star;
			itemSortData.EquipItemInfo_Level = equipItemInfo.Level;
			itemSortData.EquipItemInfo_CreateDateTime = equipItemInfo.CreateDateTime;
			itemSortData.EquipItemInfo_Identified = equipItemInfo.IsIdentified;
			itemSortData.EquipData_SetGroupType = equipItemData.SetGroupType;
			itemSortData.TechnoMagic_Advance = equipItemInfo.IsTechnoMagic && equipItemInfo.IsAdvanced;
			poolingList.Add(itemSortData);
		}
		poolingList.Sort(Compare);
		for (int j = 0; j < poolingList.Count; j++)
		{
			list[j] = poolingList[j].tm;
		}
	}

	private int Compare(ItemSortData lhs, ItemSortData rhs)
	{
		if (lhs.component == rhs.component)
		{
			return 0;
		}
		if (lhs.component == null || rhs.component == null)
		{
			if (lhs.component != null)
			{
				return -1;
			}
			if (rhs.component != null)
			{
				return 1;
			}
			return 0;
		}
		if (lhs.component.ItemData != null && lhs.component.ItemData.Tutorial)
		{
			return -1;
		}
		if (rhs.component.ItemData != null && rhs.component.ItemData.Tutorial)
		{
			return 1;
		}
		int num = ItemComparer.NullCheck(lhs.component, rhs.component);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.EquipItemInfo_IsEquipped, rhs.EquipItemInfo_IsEquipped, _isAscending_IsEquipped);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.EquipData_Tier, rhs.EquipData_Tier, _isAscending_EquipData_Tier);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare_ItemGrade(lhs.ItemData_Grade, rhs.ItemData_Grade, _isAscending_ItemData_Grade);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare_Identified(lhs.EquipItemInfo_Identified, rhs.EquipItemInfo_Identified, _isAscending_EquipItemInfo_Identified);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare_TechnoMagic_Advance(lhs.TechnoMagic_Advance, rhs.TechnoMagic_Advance, _isAscending_TechnoMagic_Advance);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare_WeaponType(lhs.EquipData_PartType, rhs.EquipData_PartType, _isAscending_EquipData_PartType);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.EquipData_SubType, rhs.EquipData_SubType, _isAscending_EquipData_SubType);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.ItemData_Index, rhs.ItemData_Index, _isAscending_ItemData_Index);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.EquipItemInfo_Locked, rhs.EquipItemInfo_Locked, _isAscending_EquipItemInfo_Locked);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.MatchedOptionCount, rhs.MatchedOptionCount, _isAscending_MatchedOptionCount);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.LowestShowIndexCount, rhs.LowestShowIndexCount, _isAscending_LowestShowIndexCount);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.EquipItemInfo_Star, rhs.EquipItemInfo_Star, _isAscending_EquipItemInfo_Star);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare(lhs.EquipItemInfo_Level, rhs.EquipItemInfo_Level, _isAscending_EquipItemInfo_Level);
		if (num != 0)
		{
			return num;
		}
		num = ItemComparer.Compare_CreateTime(lhs.EquipItemInfo_CreateDateTime, rhs.EquipItemInfo_CreateDateTime, _isAscending_EquipItemInfo_CreateDateTime);
		if (num != 0)
		{
			return num;
		}
		return 0;
	}
}
