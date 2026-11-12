using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NWindow.__Extension_Namespace__;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow;

public class EquipmentItemFilterManager
{
	public enum SortCategory
	{
		Tier,
		Grade,
		Lock,
		Star,
		All
	}

	public enum SortType
	{
		Ascending,
		Descending
	}

	public static class Helper
	{
		public static bool IsExceptionOperationType(OperationType operationType)
		{
			if (operationType != OperationType.ModMaxMp && operationType != OperationType.ModMpOnDamage && operationType != OperationType.UseSkill && operationType != OperationType.ModSkillLevelAllSlot && operationType != OperationType.ModSkillLevelSlot1 && operationType != OperationType.ModSkillLevelSlot2 && operationType != OperationType.ModSkillLevelSlot3 && operationType != OperationType.ModSkillLevelSlot4 && operationType != OperationType.ModSoulWeaponMaxUseCount)
			{
				return true;
			}
			return false;
		}

		public static string GetLocalizeKeySort(SortCategory category, SortType type)
		{
			string arg = "";
			if (category != SortCategory.Lock)
			{
				arg = ((type != SortType.Ascending) ? "_Down" : "_UP");
			}
			return $"EQUIP_SORT_{category.ToString()}{arg}".ToUpper();
		}

		public static string GetLocalizeKeyDetailOption(string filterType)
		{
			string text = "";
			for (int i = 0; i < filterType.Length; i++)
			{
				char c = filterType[i];
				text = (('@' >= c || '[' <= c) ? (text + c.ToString().ToUpper()) : (text + "_" + c));
			}
			return "OPERATION_TYPE." + text.Substring(1, text.Length - 1);
		}
	}

	public class OptionFilterTable
	{
		private Dictionary<int, bool> _table = new Dictionary<int, bool>();

		private Dictionary<int, int> _redirections = new Dictionary<int, int>();

		public void InitTable(EquipFilterOptionGroupData optionGroupData, bool isOnStart, EquipmentItemFilter.Mode mode)
		{
			_table.Clear();
			_redirections = null;
			EquipFilterOptionData[] equipFilterOptionDataListByGroup = GameRoot.Instance.EquipFilterOptionDataContainer.GetEquipFilterOptionDataListByGroup(optionGroupData.Key);
			int i = 0;
			for (int num = equipFilterOptionDataListByGroup.Length; i < num; i++)
			{
				EquipFilterOptionData equipFilterOptionData = equipFilterOptionDataListByGroup[i];
				if (equipFilterOptionData != null)
				{
					int filterType = EquipFilterOptionGroupData.GetFilterType(optionGroupData, equipFilterOptionData.ReferenceValue);
					bool value = ((!isOnStart) ? equipFilterOptionData.EXT_IsDefault() : equipFilterOptionData.EXT_GetChangeOnStartValue(mode));
					_table[filterType] = value;
					InitRedirectionValues(optionGroupData, equipFilterOptionData, filterType);
				}
			}
		}

		private void InitRedirectionValues(EquipFilterOptionGroupData optionGroupData, EquipFilterOptionData optionData, int origFilterType)
		{
			if (optionData != null && __Extension_Method_ICollection__.COUNT(optionData.RedirectionValues) != 0)
			{
				_redirections = new Dictionary<int, int>();
				string[] redirectionValues = optionData.RedirectionValues;
				foreach (string dataStr in redirectionValues)
				{
					int filterType = EquipFilterOptionGroupData.GetFilterType(optionGroupData, dataStr);
					_redirections[filterType] = origFilterType;
				}
			}
		}

		public IEnumerable<int> GetKeys()
		{
			return _table.Keys;
		}

		public void SetValueIfExist(int filterType, bool value)
		{
			if (_table.ContainsKey(filterType))
			{
				_table[filterType] = value;
			}
		}

		public void SetAllValue(bool value)
		{
			foreach (int key in _table.Keys)
			{
				_table[key] = value;
			}
		}

		public bool IsAllUnselected()
		{
			foreach (KeyValuePair<int, bool> item in _table)
			{
				if (item.Value)
				{
					return false;
				}
			}
			return true;
		}

		public void Clear()
		{
			_table.Clear();
		}

		public bool TryGetValue(int filterType, out bool value)
		{
			return _table.TryGetValue(filterType, out value);
		}

		public bool IsPassFilter(EquipFilterOptionGroupData groupData, EquipItemInfo equipItemInfo)
		{
			object obj = equipItemInfo.EXT_GetItemDataByDataTypeName(groupData.DataTypeName);
			if (obj == null)
			{
				return false;
			}
			try
			{
				object value = obj.GetType().GetProperty(groupData.PropertyInfo.Name).GetValue(obj, null);
				if (value == null)
				{
					return false;
				}
				if (value.GetType().HasElementType)
				{
					if (!(value is IEnumerable enumerable))
					{
						return false;
					}
					foreach (object item in enumerable)
					{
						int filterType = EquipFilterOptionGroupData.GetFilterType(groupData, item);
						if (TryGetValue_with_Redirection(filterType, out var value2) && value2)
						{
							return true;
						}
					}
					return false;
				}
				int filterType2 = EquipFilterOptionGroupData.GetFilterType(groupData, value);
				if (TryGetValue_with_Redirection(filterType2, out var value3))
				{
					return value3;
				}
				if (_table.TryGetValue(-1, out value3))
				{
					return value3;
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError($"EquipmentItemFilterManager::CheckGroupFilter - (GroupIndex:{groupData.Index})Try convert failed (ErrorMsg : {ex.Message})");
				return false;
			}
			return false;
		}

		private bool TryGetValue_with_Redirection(int filterType, out bool value)
		{
			if (_redirections != null && _redirections.TryGetValue(filterType, out var value2))
			{
				filterType = value2;
			}
			return _table.TryGetValue(filterType, out value);
		}
	}

	public class GroupFilterContainer
	{
		private static HashSet<int> __emptyGroupTable = new HashSet<int>();

		public Dictionary<int, OptionFilterTable> Table { get; private set; }

		public GroupFilterContainer()
		{
			Table = new Dictionary<int, OptionFilterTable>();
		}

		public void Clear()
		{
			Table.Clear();
		}

		public void InitTable(HashSet<int> exceptGroupTable, bool isOnStart, EquipmentItemFilter.Mode mode)
		{
			IList<EquipFilterOptionGroupData> allData = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetAllData();
			if (allData == null)
			{
				return;
			}
			exceptGroupTable = exceptGroupTable ?? __emptyGroupTable;
			foreach (EquipFilterOptionGroupData item in allData)
			{
				if (item != null && !exceptGroupTable.Contains(item.Index))
				{
					if (!Table.TryGetValue(item.Index, out var value))
					{
						value = new OptionFilterTable();
						Table.Add(item.Index, value);
					}
					value.InitTable(item, isOnStart, mode);
				}
			}
		}

		public void SetAllValue(bool value, HashSet<int> exceptGroupTable = null)
		{
			exceptGroupTable = exceptGroupTable ?? __emptyGroupTable;
			foreach (KeyValuePair<int, OptionFilterTable> item in Table)
			{
				if (!exceptGroupTable.Contains(item.Key))
				{
					item.Value.SetAllValue(value);
				}
			}
		}

		public bool IsAllUnselected(int groupIndex)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(Table, groupIndex, null)?.IsAllUnselected() ?? false;
		}

		public bool IsOptionFilterApplied(HashSet<int> exceptGroupTable = null)
		{
			IList<EquipFilterOptionGroupData> allData = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetAllData();
			if (allData == null)
			{
				return false;
			}
			exceptGroupTable = exceptGroupTable ?? __emptyGroupTable;
			int i = 0;
			for (int count = allData.Count; i < count; i++)
			{
				EquipFilterOptionGroupData equipFilterOptionGroupData = allData[i];
				if (equipFilterOptionGroupData != null && !exceptGroupTable.Contains(equipFilterOptionGroupData.Index))
				{
					OptionFilterTable tableByGroupIndex = GetTableByGroupIndex(equipFilterOptionGroupData.Index);
					if (IsOptionFilterChecked(equipFilterOptionGroupData, tableByGroupIndex))
					{
						return true;
					}
				}
			}
			return false;
		}

		public OptionFilterTable GetTableByGroupIndex(int groupIndex)
		{
			if (GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData(groupIndex) == null)
			{
				return null;
			}
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(Table, groupIndex, null);
		}

		private bool IsOptionFilterChecked(EquipFilterOptionGroupData groupData, OptionFilterTable filterTable)
		{
			if (groupData == null)
			{
				return false;
			}
			EquipFilterOptionData[] equipFilterOptionDataListByGroup = GameRoot.Instance.EquipFilterOptionDataContainer.GetEquipFilterOptionDataListByGroup(groupData.Key);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equipFilterOptionDataListByGroup); i++)
			{
				EquipFilterOptionData equipFilterOptionData = equipFilterOptionDataListByGroup[i];
				if (equipFilterOptionData != null)
				{
					int filterType = EquipFilterOptionGroupData.GetFilterType(equipFilterOptionData);
					if (filterTable.TryGetValue(filterType, out var value) && value != equipFilterOptionData.EXT_IsDefault())
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsPassFilter(EquipItemInfo equipItemInfo)
		{
			bool result = true;
			foreach (KeyValuePair<int, OptionFilterTable> item in Table)
			{
				EquipFilterOptionGroupData data = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData(item.Key);
				if (item.Value == null)
				{
					VespaDebug.LogError($"EquipmentItemFilterManager::CheckFilter - groupTable is null (groupIndex : {item.Key})");
					return false;
				}
				if (!IsAllUnselected(data.Index) && !IsFiltered_by_GroupData(data, equipItemInfo))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public bool IsFiltered_by_GroupData(EquipFilterOptionGroupData groupData, EquipItemInfo equipItemInfo)
		{
			if (groupData == null)
			{
				return false;
			}
			if (equipItemInfo == null)
			{
				return false;
			}
			if (!Table.TryGetValue(groupData.Index, out var value))
			{
				return false;
			}
			return value.IsPassFilter(groupData, equipItemInfo);
		}
	}

	public class DetailOptionFilterTable
	{
		public Dictionary<int, bool> Table { get; private set; }

		public DetailOptionFilterTable()
		{
			Table = new Dictionary<int, bool>();
		}

		public void InitTable()
		{
			Table.Clear();
			foreach (EquipOptionData allDatum in GameRoot.Instance.EquipOptionDataContainer.GetAllData())
			{
				if (allDatum != null && !Table.ContainsKey((int)allDatum.OptionType) && Helper.IsExceptionOperationType(allDatum.OptionType))
				{
					Table[(int)allDatum.OptionType] = false;
				}
			}
		}

		public void Clear()
		{
			Table.Clear();
		}

		public void SetValueIfExist(int filterType, bool value)
		{
			if (Table.ContainsKey(filterType))
			{
				Table[filterType] = value;
			}
		}

		public void SetAllValue(bool value)
		{
			int[] array = Table.Keys.ToArray();
			foreach (int key in array)
			{
				Table[key] = value;
			}
		}

		public int GetSelectedCount()
		{
			int num = 0;
			foreach (bool value in Table.Values)
			{
				if (value)
				{
					num++;
				}
			}
			return num;
		}

		public List<int> MakeCheckOptionList()
		{
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, bool> item in Table)
			{
				if (item.Value)
				{
					list.Add(item.Key);
				}
			}
			return list;
		}

		public bool IsFilterDetailOption(EquipItemInfo equipItemInfo)
		{
			int selectedCount = GetSelectedCount();
			if (selectedCount == 0)
			{
				return true;
			}
			int num = 0;
			foreach (KeyValuePair<int, bool> item in Table)
			{
				if (!item.Value)
				{
					continue;
				}
				OperationType key = (OperationType)item.Key;
				foreach (EquipOptionData allDatum in GameRoot.Instance.EquipOptionDataContainer.GetAllData())
				{
					if (key == allDatum.OptionType && equipItemInfo.GetOptionSlotIndex(allDatum.Index) != 0)
					{
						num++;
						break;
					}
				}
			}
			if (selectedCount > 0 && num > 0)
			{
				return true;
			}
			return false;
		}

		public bool IsAnyFilterApplied()
		{
			foreach (KeyValuePair<int, bool> item in Table)
			{
				if (item.Value)
				{
					return true;
				}
			}
			return false;
		}
	}

	public class DetailOptionCountTable
	{
		public Dictionary<int, int> Table { get; private set; }

		public DetailOptionCountTable()
		{
			Table = new Dictionary<int, int>();
		}
	}

	private const int MAX_DETAIL_OPTION_SELECTABLE_COUNT = 4;

	private const string KEY_LOCALIZE_DETAIL_OPTION = "OPERATION_TYPE.";

	private EquipmentItemFilter.CategoryToggle _currentTab;

	private EquipmentItemFilter.Mode _mode = EquipmentItemFilter.Mode.Inventory;

	private CreatureTagType _selectedTagType;

	private EquipPartType _selectedPartType;

	private List<EquipPartType> _equipablePartTypeList = new List<EquipPartType>();

	private Dictionary<SortCategory, SortType> _dicSort = new Dictionary<SortCategory, SortType>();

	private GroupFilterContainer _filterValueTable = new GroupFilterContainer();

	private DetailOptionFilterTable _detailOptionFilterTable = new DetailOptionFilterTable();

	public EquipmentItemFilter.CategoryToggle CurrentTab
	{
		get
		{
			return _currentTab;
		}
		set
		{
			_currentTab = value;
		}
	}

	public CreatureTagType SelectedTagType
	{
		get
		{
			return _selectedTagType;
		}
		set
		{
			_selectedTagType = value;
		}
	}

	public EquipPartType SelectedPartType
	{
		get
		{
			return _selectedPartType;
		}
		set
		{
			_selectedPartType = value;
		}
	}

	public Dictionary<SortCategory, SortType> DicSort => _dicSort;

	public DetailOptionFilterTable DetailOptionFilter => _detailOptionFilterTable;

	public void Init(EquipmentItemFilter.Mode mode, CreatureTagType tagType = CreatureTagType.None, EquipPartType partType = EquipPartType.None)
	{
		ChangeMode(mode);
		_equipablePartTypeList.Clear();
		_filterValueTable.Clear();
		_selectedTagType = tagType;
		_selectedPartType = partType;
		InitEquipmentItemSort();
		InitializeFilterValueTable(null, isOnStart: true);
		_detailOptionFilterTable.InitTable();
	}

	public void InitializeFilterValueTable(HashSet<int> exceptGroupTable = null, bool isOnStart = false)
	{
		_filterValueTable.InitTable(exceptGroupTable, isOnStart, _mode);
	}

	public void ResetFilterStatus()
	{
		InitializeFilterValueTable();
		_detailOptionFilterTable.InitTable();
	}

	public int GetMatchedDetailOptionCount(List<int> checkOptionList, EquipItemInfo info)
	{
		if (__Extension_Method_ICollection__.COUNT(checkOptionList) == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (int checkOption in checkOptionList)
		{
			if (checkOption == 0)
			{
				continue;
			}
			IList<EquipOptionData> equipOptionList = GameRoot.Instance.EquipOptionDataContainer.GetEquipOptionList((OperationType)checkOption);
			if (__Extension_Method_ICollection__.COUNT(equipOptionList) == 0)
			{
				continue;
			}
			foreach (EquipOptionData item in equipOptionList)
			{
				for (int i = 1; i <= 5; i++)
				{
					if (info.GetOptionIndex(i) == item.Index)
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public int GetMatchedLowestShowOptionIndexCount(List<int> checkOptionList, EquipItemInfo info)
	{
		if (__Extension_Method_ICollection__.COUNT(checkOptionList) == 0)
		{
			return 0;
		}
		int lowstShowIndex = int.MaxValue;
		int lowstShowIndexCount = 0;
		Action<int> action = delegate(int showIndex)
		{
			if (lowstShowIndex > showIndex)
			{
				lowstShowIndex = showIndex;
				lowstShowIndexCount = 0;
			}
		};
		foreach (int checkOption in checkOptionList)
		{
			if (checkOption == 0)
			{
				continue;
			}
			IList<EquipOptionData> equipOptionList = GameRoot.Instance.EquipOptionDataContainer.GetEquipOptionList((OperationType)checkOption);
			if (__Extension_Method_ICollection__.COUNT(equipOptionList) == 0)
			{
				continue;
			}
			foreach (EquipOptionData item in equipOptionList)
			{
				for (int num = 1; num <= 5; num++)
				{
					if (info.GetOptionIndex(num) != item.Index)
					{
						continue;
					}
					EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(info.GetOptionIndex(num));
					if (data != null)
					{
						action(data.ShowOptionIndex);
						if (lowstShowIndex == data.ShowOptionIndex)
						{
							lowstShowIndexCount++;
						}
					}
				}
			}
		}
		return lowstShowIndexCount;
	}

	public OptionFilterTable GetFilterTableByGroup(int groupIndex)
	{
		return _filterValueTable.GetTableByGroupIndex(groupIndex);
	}

	public bool GetFilterValue(int groupIndex, int filterType)
	{
		OptionFilterTable tableByGroupIndex = _filterValueTable.GetTableByGroupIndex(groupIndex);
		bool value;
		if (tableByGroupIndex != null)
		{
			if (!tableByGroupIndex.TryGetValue(filterType, out value))
			{
				return false;
			}
			return value;
		}
		if (_detailOptionFilterTable.Table.TryGetValue(filterType, out value))
		{
			return value;
		}
		return false;
	}

	public void ChangeMode(EquipmentItemFilter.Mode mode)
	{
		_mode = mode;
	}

	public void SetSortStatus(SortCategory category, SortType type, bool useAllCategory = true)
	{
		if (category == SortCategory.All && useAllCategory)
		{
			AllChangeSort(IsAscending(type) ? SortType.Descending : SortType.Ascending);
		}
		_dicSort[category] = type;
	}

	public void SetFilterValue(int groupIndex, int filterType, bool filterValue, bool isDetailOption = false)
	{
		if (isDetailOption)
		{
			_detailOptionFilterTable.SetValueIfExist(filterType, filterValue);
		}
		else
		{
			_filterValueTable.GetTableByGroupIndex(groupIndex)?.SetValueIfExist(filterType, filterValue);
		}
	}

	public void SetAllFilterValue(bool filterValue, HashSet<int> exceptGroupTable = null)
	{
		_filterValueTable.SetAllValue(filterValue, exceptGroupTable);
	}

	public void SetFilterValueDetailOption(int filterType, bool isFilter)
	{
		_detailOptionFilterTable.SetValueIfExist(filterType, isFilter);
	}

	public void SetAllFilterValueDetailOption(bool filterValue)
	{
		_detailOptionFilterTable.SetAllValue(filterValue);
	}

	public void SetEquipablePartList(List<EquipPartType> list)
	{
		_equipablePartTypeList.Clear();
		_equipablePartTypeList = list;
	}

	public bool IsAscending(SortType type)
	{
		if (type != SortType.Ascending)
		{
			return false;
		}
		return true;
	}

	public bool IsDefaultSort()
	{
		SortType sortType = (_dicSort.ContainsKey(SortCategory.Tier) ? _dicSort[SortCategory.Tier] : SortType.Ascending);
		foreach (KeyValuePair<SortCategory, SortType> item in _dicSort)
		{
			if (sortType != item.Value)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsFilterApplied(HashSet<int> exceptGroupTable = null)
	{
		if (IsOptionFilterApplied(exceptGroupTable))
		{
			return true;
		}
		if (IsDetailOptionFilterApplied())
		{
			return true;
		}
		return false;
	}

	public bool IsOptionFilterApplied(HashSet<int> exceptGroupTable = null)
	{
		return _filterValueTable.IsOptionFilterApplied(exceptGroupTable);
	}

	public bool IsFiltered(EquipItemInfo equipItemInfo)
	{
		if (!_filterValueTable.IsPassFilter(equipItemInfo))
		{
			return false;
		}
		if (!_detailOptionFilterTable.IsFilterDetailOption(equipItemInfo))
		{
			return false;
		}
		return true;
	}

	public bool IsDetailOptionSelectable()
	{
		if (4 >= _detailOptionFilterTable.GetSelectedCount())
		{
			return true;
		}
		return false;
	}

	public bool IsDetailOptionFilterApplied()
	{
		return _detailOptionFilterTable.IsAnyFilterApplied();
	}

	public bool IsLockedEquipPartType(EquipPartType partType)
	{
		if (_equipablePartTypeList.Count() == 0)
		{
			return true;
		}
		return !_equipablePartTypeList.Contains(partType);
	}

	private void InitEquipmentItemSort()
	{
		_dicSort.Clear();
		SortType value = SortType.Descending;
		foreach (SortCategory value2 in Enum.GetValues(typeof(SortCategory)))
		{
			_dicSort[value2] = value;
		}
		_dicSort[SortCategory.All] = SortType.Ascending;
	}

	private void AllChangeSort(SortType type)
	{
		foreach (SortCategory value in Enum.GetValues(typeof(SortCategory)))
		{
			if (value != SortCategory.All)
			{
				_dicSort[value] = type;
			}
		}
	}

	public bool GetSortType(SortCategory sortCategory)
	{
		if (_dicSort.ContainsKey(sortCategory))
		{
			return IsAscending(_dicSort[sortCategory]);
		}
		return true;
	}
}
