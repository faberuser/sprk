using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NItem;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public static class ValanceHelper
{
	public class EquipItemInfoCustomComparer : IComparer<EquipItemInfo>
	{
		private EquipItemInfo _firstInfo;

		public EquipItemInfoCustomComparer(EquipItemInfo firstInfo)
		{
			_firstInfo = firstInfo;
		}

		public int Compare(EquipItemInfo lhs, EquipItemInfo rhs)
		{
			if (_firstInfo != null)
			{
				if (lhs == _firstInfo)
				{
					return -1;
				}
				if (rhs == _firstInfo)
				{
					return 1;
				}
			}
			return ItemComparer.Compare_EquipItemInfo(lhs, rhs);
		}
	}

	public class CraftComparer : IComparer<string>
	{
		private static CraftComparer _default;

		public static CraftComparer Default
		{
			get
			{
				if (_default == null)
				{
					_default = new CraftComparer();
				}
				return _default;
			}
		}

		public int Compare(string lhs, string rhs)
		{
			int num = ValidCompare(lhs, rhs);
			if (num != 0)
			{
				return num;
			}
			EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(lhs);
			EquipItemData data2 = GameRoot.Instance.EquipItemDataContainer.GetData(rhs);
			return EquipItemDataComparer.Default.Compare(data, data2);
		}

		private int ValidCompare(string lhs, string rhs)
		{
			bool flag = String_Extension_Method.IsNullOrEmpty(lhs);
			bool flag2 = String_Extension_Method.IsNullOrEmpty(rhs);
			if (flag && flag2)
			{
				return -1;
			}
			if (flag && !flag2)
			{
				return -1;
			}
			if (!flag && flag2)
			{
				return 1;
			}
			return 0;
		}
	}

	private class EquipItemDataComparer : IComparer<EquipItemData>
	{
		private static EquipItemDataComparer _default;

		public static EquipItemDataComparer Default
		{
			get
			{
				if (_default == null)
				{
					_default = new EquipItemDataComparer();
				}
				return _default;
			}
		}

		public int Compare(EquipItemData lhs, EquipItemData rhs)
		{
			int num = ValidCompare(lhs, rhs);
			if (num != 0)
			{
				return num;
			}
			num = EquipGradeCompare(lhs.EquipGrade, rhs.EquipGrade);
			if (num != 0)
			{
				return num;
			}
			return EquipFilterInfoComparer.Default.Compare(lhs, rhs);
		}

		public int ValidCompare(EquipItemData lhs, EquipItemData rhs)
		{
			bool flag = lhs == null;
			bool flag2 = rhs == null;
			if (flag && flag2)
			{
				return -1;
			}
			if (flag && !flag2)
			{
				return -1;
			}
			if (!flag && flag2)
			{
				return 1;
			}
			return 0;
		}

		public int EquipGradeCompare(EquipItemEquipGrade lhs, EquipItemEquipGrade rhs)
		{
			return -lhs.CompareTo(rhs);
		}
	}

	private class EquipFilterInfoComparer : IComparer<IValanceEquipFilterInfo>
	{
		private static EquipFilterInfoComparer _default;

		public static EquipFilterInfoComparer Default
		{
			get
			{
				if (_default == null)
				{
					_default = new EquipFilterInfoComparer();
				}
				return _default;
			}
		}

		public int Compare(IValanceEquipFilterInfo lhs, IValanceEquipFilterInfo rhs)
		{
			int num = CompareSetType(lhs, rhs);
			if (num != 0)
			{
				return num;
			}
			num = CompareCreatureTagType(lhs, rhs);
			if (num != 0)
			{
				return num;
			}
			return CompareEquipPart(lhs, rhs);
		}

		public int CompareSortingOrder(int lhs, int rhs)
		{
			return lhs.CompareTo(rhs);
		}

		public int GetSortingOrderBySetType(IValanceEquipFilterInfo info)
		{
			return GameRoot.Instance.ValanceEquipFilterDataContainer.GetSortingOrder(info.SetType);
		}

		public int CompareSetType(IValanceEquipFilterInfo lhs, IValanceEquipFilterInfo rhs)
		{
			return CompareSortingOrder(GetSortingOrderBySetType(lhs), GetSortingOrderBySetType(lhs));
		}

		public int GetSortingOrderByCreatureTagType(IValanceEquipFilterInfo info)
		{
			return GameRoot.Instance.ValanceEquipFilterDataContainer.GetSortingOrder(GetCreatureTagType(info));
		}

		public int CompareCreatureTagType(IValanceEquipFilterInfo lhs, IValanceEquipFilterInfo rhs)
		{
			return CompareSortingOrder(GetSortingOrderByCreatureTagType(lhs), GetSortingOrderByCreatureTagType(rhs));
		}

		public int GetSortingOdrderByEquipPart(IValanceEquipFilterInfo info)
		{
			return GameRoot.Instance.ValanceEquipFilterDataContainer.GetSortingOrder(info.EquipPartType, info.EquipItemSubType);
		}

		public int CompareEquipPart(IValanceEquipFilterInfo lhs, IValanceEquipFilterInfo rhs)
		{
			return CompareSortingOrder(GetSortingOdrderByEquipPart(lhs), GetSortingOdrderByEquipPart(rhs));
		}

		private CreatureTagType GetCreatureTagType(IValanceEquipFilterInfo info)
		{
			if (info.IsCreatureTagTypeArray)
			{
				if (__Extension_Method_ICollection__.IsNullOrEmpty(info.CreatureTagTypes))
				{
					return CreatureTagType.None;
				}
				return info.CreatureTagTypes[0];
			}
			return info.CreatureTagType;
		}
	}

	public class HeroInfoCustomComparer : IComparer<CachedHeroInfo>
	{
		private int _firstHeroIndex;

		public HeroInfoCustomComparer(int firstHero)
		{
			_firstHeroIndex = firstHero;
		}

		public int Compare(CachedHeroInfo lhs, CachedHeroInfo rhs)
		{
			if (_firstHeroIndex > 0)
			{
				int num = 0;
				if (lhs.HeroIndex == _firstHeroIndex)
				{
					num = -1;
				}
				if (rhs.HeroIndex == _firstHeroIndex)
				{
					num = 1;
				}
				if (num != 0)
				{
					return num;
				}
			}
			return HeroComparer.Default.Compare(lhs, rhs);
		}
	}

	public static bool IsCraftNoticeOn()
	{
		IList<ValanceCraftData> noticeCheckList = GameRoot.Instance.ValanceCraftDataContainer.GetNoticeCheckList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(noticeCheckList); i++)
		{
			ValanceCraftData valanceCraftData = noticeCheckList[i];
			if (valanceCraftData != null)
			{
				int materialItemIndex = valanceCraftData.MaterialItemIndex;
				if (materialItemIndex != 0 && GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(materialItemIndex) >= valanceCraftData.MaterialItemCount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsIdentifiedNoticeOn()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.HasUnIdentifiedItem();
	}

	private static bool IsIdentified(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo.GetInventoryType() != InventoryType.Inventory)
		{
			return false;
		}
		if (equipItemInfo.GetEquippedHeroIndex() != -1)
		{
			return false;
		}
		return equipItemInfo.IsIdentified;
	}

	private static bool IsUnIdentified(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo.GetInventoryType() != InventoryType.Inventory)
		{
			return false;
		}
		return !equipItemInfo.IsIdentified;
	}

	public static List<EquipItemInfo> GetUnIdentifiedTechnoMagicItem()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetTechnoMagicItem(IsUnIdentified, null);
	}

	public static List<EquipItemInfo> GetIdentifiedTechnoMagicItem()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetTechnoMagicItem(IsIdentified, null);
	}

	public static List<EquipItemInfo> GetFiltedTechnoMagicItems(Func<EquipItemInfo, bool> filter, EquipItemInfo firstItem)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		EquipItemInfoCustomComparer comparer = new EquipItemInfoCustomComparer(firstItem);
		return GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetTechnoMagicItem(filter, comparer);
	}

	public static List<CachedHeroInfo> GetSortedHeroInfoList(Func<CreatureData, bool> filter, int firstHero)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		List<CachedHeroInfo> list = ReservePoolUtil.CreateList<CachedHeroInfo>();
		CreatureData creatureData = null;
		foreach (CachedHeroInfo item in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			creatureData = item.CreatureData;
			if (filter == null || filter(creatureData))
			{
				list.Add(item);
			}
		}
		list.Sort(new HeroInfoCustomComparer(firstHero));
		return list;
	}

	public static int GetEquipOptionIndex(ValanceCraftData craftData, int[] equipOptionIndices, int groupArrayIndex = 0)
	{
		if ((craftData == null && __Extension_Method_ICollection__.IsNullOrEmpty(craftData.EquipOptionGroupIndex)) || __Extension_Method_ICollection__.COUNT(craftData.EquipOptionGroupIndex) <= groupArrayIndex || __Extension_Method_ICollection__.IsNullOrEmpty(equipOptionIndices))
		{
			return 0;
		}
		int key = craftData.EquipOptionGroupIndex[groupArrayIndex];
		EquipOptionGroupData data = GameRoot.Instance.EquipOptionGroupDataContainer.GetData(key);
		if (data == null)
		{
			return 0;
		}
		HashSet<int> hashSet = new HashSet<int>(equipOptionIndices);
		int[] optionIndex = data.OptionIndex;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(optionIndex); i++)
		{
			int num = optionIndex[i];
			if (hashSet.Contains(num))
			{
				return num;
			}
		}
		return 0;
	}
}
