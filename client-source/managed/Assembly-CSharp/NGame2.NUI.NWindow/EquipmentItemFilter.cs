using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipmentItemFilter : BaseWindow, EquipmentItemFilterMenuComponent.Listener, EquipFilterOptionComponent.IListener
{
	public delegate void DelSortCallback(EquipmentItemFilterManager.SortCategory category, EquipmentItemFilterManager.SortType type);

	public interface Listener
	{
		void OnClickOK();
	}

	public enum Mode
	{
		HeroList,
		Inventory
	}

	public enum CategoryToggle
	{
		FILTER,
		DETAIL_OPTION
	}

	public enum FilterCategory
	{
		CreatureTag,
		EquipPart,
		Grade,
		Tier,
		Star,
		Lock
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Inventory/EquipmentItemFilter";

	public const string PATH_PREFAB_EQUIP_FILTER_OPTION_GROUP = "Resources/Prefab2/Component/Inventory/EquipFilterOptionGroupComponent";

	public const string GROUP_KEY_CREATURE_TAG = "CreatureTag";

	public const string GROUP_KEY_EQUIP_PART = "EquipPart";

	public UITable TableOptionGroups;

	public UIScrollView TableScrollView;

	[SerializeField]
	private UISimpleGrid SortGrid;

	[SerializeField]
	private UIScrollGrid DetailOptionGrid;

	[SerializeField]
	private GameObject GoFilter;

	[SerializeField]
	private GameObject GoDetailOption;

	[SerializeField]
	private UIButton BtnAllCancel;

	[SerializeField]
	private UIButton BtnAllCancelDetailOption;

	[SerializeField]
	private UIGrid GridCategoryFilter;

	[SerializeField]
	private UIGrid GridCategoryDetailOption;

	private List<EquipmentItemButtonComponent> _sortButtonList = new List<EquipmentItemButtonComponent>();

	private List<EquipFilterOptionGroupComponent> _optionGroupCompList = new List<EquipFilterOptionGroupComponent>();

	private List<EquipmentItemFilterMenuComponent> _listDetailOption = new List<EquipmentItemFilterMenuComponent>();

	private Dictionary<int, Dictionary<int, bool>> _FilterValueForRevert = new Dictionary<int, Dictionary<int, bool>>();

	private Dictionary<int, bool> _detailOptionValueTableForRevert = new Dictionary<int, bool>();

	private Dictionary<EquipmentItemFilterManager.SortCategory, EquipmentItemFilterManager.SortType> _sortValueTableForRevert = new Dictionary<EquipmentItemFilterManager.SortCategory, EquipmentItemFilterManager.SortType>();

	private PrefabAuto<EquipFilterOptionGroupComponent> _equipFilterOptionGroupComponentPrefab = new PrefabAuto<EquipFilterOptionGroupComponent>("Resources/Prefab2/Component/Inventory/EquipFilterOptionGroupComponent");

	private Mode _mode = Mode.Inventory;

	private bool _isAccepted;

	private EquipmentItemFilterManager _equipmentItemFilterManager;

	private Listener _listener;

	private int _selectedTagType;

	private int _selectedEquipPart;

	private int _tempSelectedEquipPart;

	private HashSet<int> _exceptGroupIndexTable = new HashSet<int>();

	public override void OnClosing()
	{
		if (!_isAccepted)
		{
			RevertAllChanges();
		}
		base.OnClosing();
	}

	public void Open(Listener listener, ref EquipmentItemFilterManager equipmentItemFilterManager, Mode mode = Mode.Inventory)
	{
		_isAccepted = false;
		_exceptGroupIndexTable.Clear();
		Open();
		ChangeMode(mode);
		_listener = listener;
		_equipmentItemFilterManager = equipmentItemFilterManager;
		_equipmentItemFilterManager.ChangeMode(mode);
		Init();
		UpdateUI();
	}

	public void OpenInHeroList(Listener listener, ref EquipmentItemFilterManager equipmentItemFilterManager, HashSet<int> exceptGroupIndexTable)
	{
		Open(listener, ref equipmentItemFilterManager, Mode.HeroList);
		_exceptGroupIndexTable = exceptGroupIndexTable;
	}

	public void Init()
	{
		_FilterValueForRevert.Clear();
		_detailOptionValueTableForRevert.Clear();
		_sortValueTableForRevert.Clear();
		_selectedTagType = (int)_equipmentItemFilterManager.SelectedTagType;
		_selectedEquipPart = (int)_equipmentItemFilterManager.SelectedPartType;
		_tempSelectedEquipPart = _selectedEquipPart;
		InitSortGrid();
		InitCategoryTab();
		InitDetailOptionGrid();
		int selectedCount = _equipmentItemFilterManager.DetailOptionFilter.GetSelectedCount();
		UIHelper.EnableGameObject(BtnAllCancel, IsFilterApplied());
		UIHelper.EnableGameObject(BtnAllCancelDetailOption, enable: true);
		UIHelper.EnableButton(BtnAllCancelDetailOption.transform.parent.GetComponent<UIButton>(), enable: false);
		UIHelper.EnableButton(BtnAllCancelDetailOption, selectedCount > 0);
	}

	public void UpdateEquipFilterState()
	{
		int i = 0;
		for (int count = _optionGroupCompList.Count; i < count; i++)
		{
			EquipFilterOptionGroupComponent equipFilterOptionGroupComponent = _optionGroupCompList[i];
			if (!(equipFilterOptionGroupComponent == null))
			{
				UpdateClassAndEquipPartValue(equipFilterOptionGroupComponent.Data);
				equipFilterOptionGroupComponent.UpdateFilterActiveState();
			}
		}
	}

	public void SetSortStatus(EquipmentItemFilterManager.SortCategory category, EquipmentItemFilterManager.SortType type)
	{
		EquipmentItemFilterManager.SortType sortType = (_equipmentItemFilterManager.IsAscending(type) ? EquipmentItemFilterManager.SortType.Descending : EquipmentItemFilterManager.SortType.Ascending);
		if (category == EquipmentItemFilterManager.SortCategory.All)
		{
			foreach (EquipmentItemButtonComponent sortButton in _sortButtonList)
			{
				if (sortButton != null && sortButton.SortCategory != EquipmentItemFilterManager.SortCategory.All)
				{
					sortButton.SetChangeSort(sortType);
					_equipmentItemFilterManager.SetSortStatus(sortButton.SortCategory, sortType);
				}
			}
			_equipmentItemFilterManager.SetSortStatus(category, type);
		}
		else
		{
			_equipmentItemFilterManager.SetSortStatus(category, type);
		}
		if (_equipmentItemFilterManager.IsDefaultSort())
		{
			_equipmentItemFilterManager.SetSortStatus(EquipmentItemFilterManager.SortCategory.All, sortType);
			EquipmentItemButtonComponent equipmentItemButtonComponent = _sortButtonList.Find((EquipmentItemButtonComponent comp) => comp.SortCategory == EquipmentItemFilterManager.SortCategory.All);
			if (equipmentItemButtonComponent != null)
			{
				equipmentItemButtonComponent.SetChangeSort(sortType);
			}
		}
	}

	public bool IsFilterApplied()
	{
		return _equipmentItemFilterManager.IsOptionFilterApplied(_exceptGroupIndexTable);
	}

	public void OnToggleChangedFilter()
	{
		UIHelper.EnableGameObject(GoFilter, enable: false);
		UIHelper.EnableGameObject(GoDetailOption, enable: true);
		_equipmentItemFilterManager.CurrentTab = CategoryToggle.DETAIL_OPTION;
		foreach (Transform child in GridCategoryFilter.GetChildList())
		{
			child.GetComponent<UIToggle>().Set(GridCategoryFilter.GetIndex(child) == 0);
		}
	}

	public void OnToggleChangedDetailOption()
	{
		UIHelper.EnableGameObject(GoFilter, enable: true);
		UIHelper.EnableGameObject(GoDetailOption, enable: false);
		_equipmentItemFilterManager.CurrentTab = CategoryToggle.FILTER;
		foreach (Transform child in GridCategoryDetailOption.GetChildList())
		{
			child.GetComponent<UIToggle>().Set(1 == GridCategoryDetailOption.GetIndex(child));
		}
	}

	public void OnClickConfirm()
	{
		_isAccepted = true;
		_selectedEquipPart = _tempSelectedEquipPart;
		_equipmentItemFilterManager.SelectedPartType = (EquipPartType)_selectedEquipPart;
		if (_listener != null)
		{
			_listener.OnClickOK();
			Close();
		}
	}

	public void OnClickCancel()
	{
		Close();
	}

	public void OnClickCancelFilter()
	{
		InitFilterStatus();
		UIHelper.EnableGameObject(BtnAllCancel, enable: false);
	}

	public void OnClickAllFilter()
	{
		AllChangeFilterStatus(isFilter: true);
		UIHelper.EnableGameObject(BtnAllCancel, enable: true);
	}

	public void OnClickCancelDetailOption()
	{
		AllChangeFilterStatusDetailOption(isFilter: false);
		UIHelper.EnableButton(BtnAllCancelDetailOption, enable: false);
	}

	private void UpdateUI()
	{
		VespaCoroutine.Start(this, UpdateTableListUI());
	}

	private IEnumerator UpdateTableListUI()
	{
		FactoryBase.ReleaseAndClearList(_optionGroupCompList);
		EquipFilterOptionGroupData[] array = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetAllData().ToArray();
		if (array == null)
		{
			yield break;
		}
		Array.Sort(array, (EquipFilterOptionGroupData group1, EquipFilterOptionGroupData group2) => group1.Step.CompareTo(group2.Step));
		int num = 0;
		for (int num2 = array.Length; num < num2; num++)
		{
			EquipFilterOptionGroupData equipFilterOptionGroupData = array[num];
			if (equipFilterOptionGroupData != null && equipFilterOptionGroupData.Show)
			{
				EquipFilterOptionGroupComponent equipFilterOptionGroupComponent = _equipFilterOptionGroupComponentPrefab.Factory.Create(TableOptionGroups.transform);
				if (!(equipFilterOptionGroupComponent == null))
				{
					int[] lockedEquipParts = GetLockedEquipParts(equipFilterOptionGroupData);
					equipFilterOptionGroupComponent.Open(equipFilterOptionGroupData, this, GetSelectModeWithCurrentState(equipFilterOptionGroupData), lockedEquipParts);
					_optionGroupCompList.Add(equipFilterOptionGroupComponent);
					UpdateClassAndEquipPartValue(equipFilterOptionGroupData);
				}
			}
		}
		yield return new WaitForSeconds(0.01f);
		TableOptionGroups.Reposition();
		UpdateEquipFilterState();
		if (TableScrollView != null)
		{
			TableScrollView.ResetPosition();
		}
	}

	private void UpdateClassAndEquipPartValue(EquipFilterOptionGroupData groupData)
	{
		if (_mode == Mode.HeroList)
		{
			if (groupData.Key == "CreatureTag")
			{
				_equipmentItemFilterManager.SetFilterValue(groupData.Index, _selectedTagType, filterValue: true);
			}
			if (groupData.Key == "EquipPart")
			{
				_equipmentItemFilterManager.SetFilterValue(groupData.Index, _selectedEquipPart, filterValue: true);
			}
		}
	}

	private EquipFilterOptionGroupComponent.SelectMode GetSelectModeWithCurrentState(EquipFilterOptionGroupData groupData)
	{
		if (groupData == null)
		{
			return EquipFilterOptionGroupComponent.SelectMode.MultiSelect;
		}
		if (_mode == Mode.Inventory)
		{
			return EquipFilterOptionGroupComponent.SelectMode.MultiSelect;
		}
		if (groupData.Key == "CreatureTag")
		{
			return EquipFilterOptionGroupComponent.SelectMode.Disable;
		}
		if (groupData.Key == "EquipPart")
		{
			return EquipFilterOptionGroupComponent.SelectMode.SingleSelect;
		}
		return EquipFilterOptionGroupComponent.SelectMode.MultiSelect;
	}

	private void ClearList<T>(List<T> list) where T : Component
	{
		FactoryBase.ReleaseAndClearList(list);
	}

	private void InitSortGrid()
	{
		ClearList(_sortButtonList);
		foreach (KeyValuePair<EquipmentItemFilterManager.SortCategory, EquipmentItemFilterManager.SortType> item in _equipmentItemFilterManager.DicSort)
		{
			EquipmentItemButtonComponent equipmentItemButtonComponent = UIHelper.AddGridItem(SortGrid, EquipmentItemButtonComponent.PrefabAuto);
			equipmentItemButtonComponent.Init(new KeyValuePair<EquipmentItemFilterManager.SortCategory, EquipmentItemFilterManager.SortType>(item.Key, item.Value), SetSortStatus, EquipmentItemFilterManager.Helper.GetLocalizeKeySort(item.Key, item.Value));
			_sortButtonList.Add(equipmentItemButtonComponent);
			_sortValueTableForRevert[item.Key] = item.Value;
		}
		UIHelper.RepositionGrid(SortGrid);
	}

	private void InitCategoryTab()
	{
		if (_equipmentItemFilterManager.CurrentTab == CategoryToggle.FILTER)
		{
			OnToggleChangedDetailOption();
		}
		else
		{
			OnToggleChangedFilter();
		}
	}

	private void InitDetailOptionGrid()
	{
		ClearList(_listDetailOption);
		foreach (KeyValuePair<int, bool> item in _equipmentItemFilterManager.DetailOptionFilter.Table)
		{
			OperationType key = (OperationType)item.Key;
			EquipmentItemFilterMenuComponent equipmentItemFilterMenuComponent = DetailOptionGrid.AddGridItem(EquipmentItemFilterMenuComponent.PrefabAutoDetailOption);
			if (equipmentItemFilterMenuComponent != null)
			{
				equipmentItemFilterMenuComponent.Open(this, item.Key, item.Value, EquipmentItemFilterManager.Helper.GetLocalizeKeyDetailOption(key.ToString()));
				_listDetailOption.Add(equipmentItemFilterMenuComponent);
			}
			_detailOptionValueTableForRevert[item.Key] = item.Value;
		}
		DetailOptionGrid.Grid.sorting = UIGrid.Sorting.Custom;
		DetailOptionGrid.Grid.onCustomSort = OnSortOption;
		UIHelper.RepositionGrid(DetailOptionGrid);
	}

	private int OnSortOption(Transform l, Transform r)
	{
		EquipmentItemFilterMenuComponent component = l.GetComponent<EquipmentItemFilterMenuComponent>();
		EquipmentItemFilterMenuComponent component2 = r.GetComponent<EquipmentItemFilterMenuComponent>();
		if (component != null && component2 != null)
		{
			OperationType filterType = (OperationType)component.FilterType;
			OperationType filterType2 = (OperationType)component2.FilterType;
			EquipOptionData dataByOptionName = GameRoot.Instance.EquipOptionDataContainer.GetDataByOptionName(filterType.ToString());
			EquipOptionData dataByOptionName2 = GameRoot.Instance.EquipOptionDataContainer.GetDataByOptionName(filterType2.ToString());
			if (dataByOptionName == null)
			{
				return 1;
			}
			if (dataByOptionName2 == null)
			{
				return -1;
			}
			return dataByOptionName.ShowOptionIndex.CompareTo(dataByOptionName2.ShowOptionIndex);
		}
		if (component == null)
		{
			return 1;
		}
		if (component2 == null)
		{
			return -1;
		}
		return 0;
	}

	private void AllChangeFilterStatus(bool isFilter)
	{
		_equipmentItemFilterManager.SetAllFilterValue(isFilter, _exceptGroupIndexTable);
		UpdateEquipFilterState();
	}

	private void InitFilterStatus()
	{
		_equipmentItemFilterManager.InitializeFilterValueTable(_exceptGroupIndexTable);
		UpdateEquipFilterState();
	}

	private void AllChangeFilterStatusDetailOption(bool isFilter)
	{
		ChangeFilterValue(_listDetailOption, isFilter);
		_equipmentItemFilterManager.SetAllFilterValueDetailOption(isFilter);
	}

	private void ChangeFilterValue(List<EquipmentItemFilterMenuComponent> list, bool isFilter)
	{
		foreach (EquipmentItemFilterMenuComponent item in list)
		{
			if (item != null)
			{
				item.SetFilter(isFilter);
			}
		}
	}

	private void ChangeFilterStatusByFilterType(List<EquipmentItemFilterMenuComponent> list, int filerType, bool isFilter)
	{
		foreach (EquipmentItemFilterMenuComponent item in list)
		{
			if (item != null && item.FilterType == filerType)
			{
				item.SetFilter(isFilter);
				break;
			}
		}
	}

	private void RevertAllChanges()
	{
		foreach (KeyValuePair<int, Dictionary<int, bool>> item in _FilterValueForRevert)
		{
			foreach (KeyValuePair<int, bool> item2 in item.Value)
			{
				_equipmentItemFilterManager.SetFilterValue(item.Key, item2.Key, item2.Value);
			}
		}
		foreach (KeyValuePair<int, bool> item3 in _detailOptionValueTableForRevert)
		{
			_equipmentItemFilterManager.SetFilterValueDetailOption(item3.Key, item3.Value);
		}
		foreach (KeyValuePair<EquipmentItemFilterManager.SortCategory, EquipmentItemFilterManager.SortType> item4 in _sortValueTableForRevert)
		{
			_equipmentItemFilterManager.SetSortStatus(item4.Key, item4.Value, useAllCategory: false);
		}
		_equipmentItemFilterManager.SelectedTagType = (CreatureTagType)_selectedTagType;
		_equipmentItemFilterManager.SelectedPartType = (EquipPartType)_selectedEquipPart;
	}

	private void BackupOriginFilterValue(int groupKey, int filterType, bool value)
	{
		if (!_FilterValueForRevert.ContainsKey(groupKey))
		{
			_FilterValueForRevert[groupKey] = new Dictionary<int, bool>();
		}
		if (!_FilterValueForRevert[groupKey].ContainsKey(filterType))
		{
			_FilterValueForRevert[groupKey][filterType] = value;
		}
	}

	private void UpdateSelectedEquipPartType(int groupIndex, int filterType)
	{
		if (!(GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData(groupIndex).Key != "EquipPart"))
		{
			_tempSelectedEquipPart = filterType;
		}
	}

	private void ChangeMode(Mode mode)
	{
		_mode = mode;
		if (mode != Mode.HeroList)
		{
			_ = 1;
		}
	}

	private int[] GetLockedEquipParts(EquipFilterOptionGroupData groupData)
	{
		if (groupData == null)
		{
			return null;
		}
		if (groupData.Key != "EquipPart")
		{
			return null;
		}
		List<int> list = new List<int>();
		int i = 0;
		for (int num = 8; i < num; i++)
		{
			if (_equipmentItemFilterManager.IsLockedEquipPartType((EquipPartType)i))
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	void EquipmentItemFilterMenuComponent.Listener.OnClickFilter(int filterType, bool isFilter)
	{
		_equipmentItemFilterManager.SetFilterValue(-1, filterType, isFilter, isDetailOption: true);
		if (!_equipmentItemFilterManager.IsDetailOptionSelectable())
		{
			_equipmentItemFilterManager.SetFilterValueDetailOption(filterType, isFilter: false);
			ChangeFilterStatusByFilterType(_listDetailOption, filterType, isFilter: false);
		}
		UIHelper.EnableButton(BtnAllCancelDetailOption, _equipmentItemFilterManager.IsDetailOptionFilterApplied());
	}

	void EquipFilterOptionComponent.IListener.OnClickOption(EquipFilterOptionComponent comp, int groupIndex, int filterType, bool isFilter)
	{
		BackupOriginFilterValue(groupIndex, filterType, _equipmentItemFilterManager.GetFilterValue(groupIndex, filterType));
		_equipmentItemFilterManager.SetFilterValue(groupIndex, filterType, isFilter);
		if (isFilter)
		{
			UpdateSelectedEquipPartType(groupIndex, filterType);
		}
		UIHelper.EnableGameObject(BtnAllCancel, IsFilterApplied());
	}

	bool EquipFilterOptionComponent.IListener.IsFilterActive(int groupIndex, int filterType)
	{
		return _equipmentItemFilterManager.GetFilterValue(groupIndex, filterType);
	}
}
