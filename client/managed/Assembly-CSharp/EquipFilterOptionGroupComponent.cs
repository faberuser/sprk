using System;
using System.Collections.Generic;
using NGame2.NUI;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

public class EquipFilterOptionGroupComponent : LinkFactoryComponent, EquipFilterOptionComponent.IListener
{
	public enum SelectMode
	{
		Disable,
		SingleSelect,
		MultiSelect
	}

	public const string PATH_PREFAB_EQUIP_FILTER_OPTION = "Resources/Prefab2/Component/Inventory/EquipFilterOptionComponent";

	public UILabel LabelGroupName;

	public UISimpleGrid GridOptionList;

	public UISprite SpriteCheckImg;

	public GameObject AllSelectRoot;

	public float GapBetweenOption = 20f;

	private PrefabAuto<EquipFilterOptionComponent> _equipFilterOptionComponentPrefab = new PrefabAuto<EquipFilterOptionComponent>("Resources/Prefab2/Component/Inventory/EquipFilterOptionComponent");

	private SelectMode _selectMode = SelectMode.MultiSelect;

	private bool _isAllChecked;

	private EquipFilterOptionGroupData _data;

	private EquipFilterOptionComponent.IListener _listener;

	private List<EquipFilterOptionComponent> _optionCompList = new List<EquipFilterOptionComponent>();

	private HashSet<int> _lockedOptionTable = new HashSet<int>();

	public bool IsAllChecked
	{
		get
		{
			return _isAllChecked;
		}
		set
		{
			_isAllChecked = value;
			if (SpriteCheckImg != null)
			{
				SpriteCheckImg.gameObject.SetActive(_isAllChecked);
			}
		}
	}

	public EquipFilterOptionGroupData Data => _data;

	public void OnClickAllSelectCheckBox()
	{
		IsAllChecked = !IsAllChecked;
		SetSelectAllOption(IsAllChecked);
	}

	public void Open(EquipFilterOptionGroupData data, EquipFilterOptionComponent.IListener listener, SelectMode mode = SelectMode.MultiSelect, int[] lockedOptions = null)
	{
		_data = data;
		_listener = listener;
		InitLockOptionTable(lockedOptions);
		ChangeMode(mode);
	}

	public void SetSelectAllOption(bool isSelect)
	{
		int i = 0;
		for (int count = _optionCompList.Count; i < count; i++)
		{
			EquipFilterOptionComponent equipFilterOptionComponent = _optionCompList[i];
			if (!(equipFilterOptionComponent == null))
			{
				equipFilterOptionComponent.IsActive = isSelect;
			}
		}
	}

	public void UpdateFilterActiveState()
	{
		int i = 0;
		for (int count = _optionCompList.Count; i < count; i++)
		{
			EquipFilterOptionComponent equipFilterOptionComponent = _optionCompList[i];
			if (!(equipFilterOptionComponent == null))
			{
				equipFilterOptionComponent.UpdateFilterActive();
			}
		}
	}

	private void UpdateUI()
	{
		FactoryBase.ReleaseAndClearList(_optionCompList);
		if (_data != null)
		{
			if (LabelGroupName != null)
			{
				LabelGroupName.text = LocalizationManager.GetStringDirect(_data.LocalKey);
			}
			UpdateOptionList();
		}
	}

	private void UpdateOptionList()
	{
		EquipFilterOptionData[] equipFilterOptionDataListByGroup = GameRoot.Instance.EquipFilterOptionDataContainer.GetEquipFilterOptionDataListByGroup(_data.Key);
		if (equipFilterOptionDataListByGroup == null)
		{
			return;
		}
		Array.Sort(equipFilterOptionDataListByGroup, (EquipFilterOptionData option1, EquipFilterOptionData option2) => option1.Step.CompareTo(option2.Step));
		FactoryBase.ReleaseAndClearList(_optionCompList);
		int num = 0;
		for (int num2 = equipFilterOptionDataListByGroup.Length; num < num2; num++)
		{
			EquipFilterOptionData equipFilterOptionData = equipFilterOptionDataListByGroup[num];
			if (equipFilterOptionData == null)
			{
				continue;
			}
			EquipFilterOptionComponent equipFilterOptionComponent = GridOptionList.AddGridItem(_equipFilterOptionComponentPrefab);
			if (!(equipFilterOptionComponent == null))
			{
				equipFilterOptionComponent.Open(equipFilterOptionData, this, _selectMode);
				_optionCompList.Add(equipFilterOptionComponent);
				if (_selectMode == SelectMode.SingleSelect && _lockedOptionTable.Contains(equipFilterOptionComponent.FilterType))
				{
					equipFilterOptionComponent.IsLocked = true;
				}
			}
		}
		UIHelper.RepositionGrid(GridOptionList);
	}

	private void UpdateAllOptionSelected()
	{
		int i = 0;
		for (int count = _optionCompList.Count; i < count; i++)
		{
			EquipFilterOptionComponent equipFilterOptionComponent = _optionCompList[i];
			if (!(equipFilterOptionComponent == null) && !equipFilterOptionComponent.IsActive)
			{
				IsAllChecked = false;
				return;
			}
		}
		IsAllChecked = true;
	}

	private void SetLockOptions(bool IsLock)
	{
		int i = 0;
		for (int count = _optionCompList.Count; i < count; i++)
		{
			EquipFilterOptionComponent equipFilterOptionComponent = _optionCompList[i];
			if (!(equipFilterOptionComponent == null))
			{
				equipFilterOptionComponent.IsLocked = IsLock;
			}
		}
	}

	private void ChangeMode(SelectMode mode)
	{
		_selectMode = mode;
		OnChangeMode();
	}

	private void OnChangeMode()
	{
		switch (_selectMode)
		{
		case SelectMode.Disable:
			if (AllSelectRoot != null)
			{
				AllSelectRoot.gameObject.SetActive(value: false);
			}
			break;
		case SelectMode.SingleSelect:
			if (AllSelectRoot != null)
			{
				AllSelectRoot.gameObject.SetActive(value: false);
			}
			break;
		case SelectMode.MultiSelect:
			if (AllSelectRoot != null)
			{
				AllSelectRoot.gameObject.SetActive(value: true);
			}
			break;
		}
		UpdateUI();
	}

	private void InitLockOptionTable(int[] lockedList)
	{
		_lockedOptionTable.Clear();
		if (__Extension_Method_ICollection__.COUNT(lockedList) != 0)
		{
			int i = 0;
			for (int num = lockedList.Length; i < num; i++)
			{
				_lockedOptionTable.Add(lockedList[i]);
			}
		}
	}

	void EquipFilterOptionComponent.IListener.OnClickOption(EquipFilterOptionComponent targetComp, int groupIndex, int filterType, bool value)
	{
		if (_listener != null)
		{
			_listener.OnClickOption(targetComp, groupIndex, filterType, value);
		}
		if (_selectMode == SelectMode.SingleSelect && !value)
		{
			return;
		}
		UpdateAllOptionSelected();
		if (_selectMode != SelectMode.SingleSelect || !value)
		{
			return;
		}
		int i = 0;
		for (int count = _optionCompList.Count; i < count; i++)
		{
			EquipFilterOptionComponent equipFilterOptionComponent = _optionCompList[i];
			if (!(equipFilterOptionComponent == null) && !(equipFilterOptionComponent == targetComp))
			{
				equipFilterOptionComponent.IsActive = false;
			}
		}
	}

	bool EquipFilterOptionComponent.IListener.IsFilterActive(int groupIndex, int filterType)
	{
		if (_listener != null)
		{
			return _listener.IsFilterActive(groupIndex, filterType);
		}
		return false;
	}
}
