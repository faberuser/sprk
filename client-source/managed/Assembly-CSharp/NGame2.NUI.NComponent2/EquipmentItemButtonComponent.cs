using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EquipmentItemButtonComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Resources/Prefab2/Component/Inventory/EquipItemButtonComponent";

	public static PrefabAuto<EquipmentItemButtonComponent> PrefabAuto = new PrefabAuto<EquipmentItemButtonComponent>("Resources/Prefab2/Component/Inventory/EquipItemButtonComponent");

	[SerializeField]
	private GameObject _incGameObj;

	[SerializeField]
	private GameObject _decGameObj;

	[SerializeField]
	private UILabel LbAscending;

	[SerializeField]
	private UILabel LbDescending;

	private EquipmentItemFilterManager.SortCategory _sortCategory;

	private EquipmentItemFilterManager.SortType _sortType;

	private EquipmentItemFilter.DelSortCallback _sortCallback;

	public EquipmentItemFilterManager.SortCategory SortCategory => _sortCategory;

	public EquipmentItemFilterManager.SortType SortType => _sortType;

	public void Init(KeyValuePair<EquipmentItemFilterManager.SortCategory, EquipmentItemFilterManager.SortType> sortPair, EquipmentItemFilter.DelSortCallback sortCallback, string localizeKey)
	{
		_sortCategory = sortPair.Key;
		_sortCallback = sortCallback;
		SetLocalize(localizeKey);
		SetChangeSort(sortPair.Value);
	}

	private void SetLocalize(string localizeKey)
	{
		UIHelper.SetLabelText(LbAscending, LocalizationManager.GetStringDirect(localizeKey));
		UIHelper.SetLabelText(LbDescending, LocalizationManager.GetStringDirect(localizeKey));
	}

	private void SetChangeButtonStatus()
	{
		UIHelper.EnableGameObject(_incGameObj, _sortType == EquipmentItemFilterManager.SortType.Ascending);
		UIHelper.EnableGameObject(_decGameObj, EquipmentItemFilterManager.SortType.Descending == _sortType);
	}

	public void SetChangeSort(EquipmentItemFilterManager.SortType sortType)
	{
		_sortType = sortType;
		SetChangeButtonStatus();
	}

	private void SetChangeSort()
	{
		if (_sortType == EquipmentItemFilterManager.SortType.Ascending)
		{
			_sortType = EquipmentItemFilterManager.SortType.Descending;
		}
		else
		{
			_sortType = EquipmentItemFilterManager.SortType.Ascending;
		}
		SetChangeButtonStatus();
	}

	public void OnClickSort()
	{
		SetChangeSort();
		_sortCallback(_sortCategory, _sortType);
	}
}
