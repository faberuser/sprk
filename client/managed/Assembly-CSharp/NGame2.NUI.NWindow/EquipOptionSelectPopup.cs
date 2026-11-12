using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class EquipOptionSelectPopup : BaseWindow, EquipOptionSelectSlotComponent.Listener, EquipOptionComponent.Listener
{
	public interface Listener
	{
		void OnOptionChange(Dictionary<int, int> slotOptionDic, bool isExtraOption);
	}

	public const string PrefabPath = "Prefab2/Window/Inventory/EquipOptionSelectPopup";

	public UISimpleGrid Grid_Item;

	public UILabel Label_ItemName;

	public UIButton Btn_Confirm;

	public UIScrollGrid Grid_OptionSlot;

	private List<EquipOptionSelectSlotComponent> _selectedOptionComponentList = new List<EquipOptionSelectSlotComponent>();

	public UIScrollGrid Grid_SelectAbleOption;

	private List<EquipOptionComponent> _selectAblepOptionComponentList = new List<EquipOptionComponent>();

	public UILabel Lbl_Title;

	private ItemIconComponent _selectedItemIcon;

	private Listener _listener;

	private ItemData _itemData;

	private ItemData<EquipItemData> _equipItemData;

	private int _selectedSlotIndex;

	private int _selectedSlotCount;

	private int _selectMaxCount;

	private bool _isExtraOption;

	private Dictionary<int, int> _slotOptionDic = new Dictionary<int, int>();

	private List<EquipOptionData> _slotOptionDataList = new List<EquipOptionData>();

	public override void Reset()
	{
		base.Reset();
		ResetInternal();
	}

	private void ResetInternal()
	{
		_selectedSlotIndex = 0;
		_selectedSlotCount = 0;
		_slotOptionDic.Clear();
		_slotOptionDataList.Clear();
		FactoryBase.ReleaseAndClearList(_selectAblepOptionComponentList);
		FactoryBase.ReleaseAndClearList(_selectedOptionComponentList);
		FactoryBase.ReleaseComponent(_selectedItemIcon);
		_selectedItemIcon = null;
	}

	public void Open(ItemData itemData, int selectMaxCount, bool isExtraOption, Listener listener)
	{
		base.Open();
		if (itemData == null)
		{
			VespaDebug.LogError("itemData == null");
			return;
		}
		if (itemData.Type != ItemType.Equip)
		{
			VespaDebug.LogError("itemData.Type != NShared.ItemType.Equip");
			return;
		}
		_listener = listener;
		_itemData = itemData;
		_isExtraOption = isExtraOption;
		_equipItemData = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemData.Index);
		if (_equipItemData == null)
		{
			VespaDebug.LogError("_equipItemData == null");
			return;
		}
		_selectMaxCount = selectMaxCount;
		SetItemDescription(_itemData);
		InitSelectAbleOptionList();
	}

	private void SetItemDescription(ItemData itemData)
	{
		if (itemData != null)
		{
			UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(itemData));
			_selectedItemIcon = UIHelper.AddGridItem(Grid_Item, ItemIconComponent.PrefabAuto);
			_selectedItemIcon.Open(itemData);
			ShowOptionSlotList(itemData);
			SetConfirmButton();
		}
	}

	private void ShowOptionSlotList(ItemData itemData)
	{
		for (int i = 1; i <= _selectMaxCount; i++)
		{
			EquipOptionSelectSlotComponent equipOptionSelectSlotComponent = UIHelper.AddGridItem(Grid_OptionSlot, EquipOptionSelectSlotComponent.PrefabAuto);
			equipOptionSelectSlotComponent.Open(i, this);
			_selectedOptionComponentList.Add(equipOptionSelectSlotComponent);
		}
		UIHelper.RepositionGrid(Grid_OptionSlot);
	}

	private void SetConfirmButton()
	{
		UIHelper.EnableButton(Btn_Confirm, IsAbleConfirm());
	}

	private void InitSelectAbleOptionList()
	{
		int[] array = null;
		array = ((!_isExtraOption) ? _equipItemData.SubData.OptionGroupIndex : _equipItemData.SubData.UniqueOptionGroupIndex);
		List<EquipOptionData> equipOptionDataList = GetEquipOptionDataList(array, (EquipOptionData l, EquipOptionData r) => l.ShowOptionIndex.CompareTo(r.ShowOptionIndex));
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(equipOptionDataList); num++)
		{
			_ = equipOptionDataList[num].Index;
			EquipOptionData equipOptionData = equipOptionDataList[num];
			EquipOptionComponent equipOptionComponent = UIHelper.AddGridItem(Grid_SelectAbleOption, EquipOptionComponent.PrefabAuto);
			equipOptionComponent.Open(equipOptionData, this);
			_selectAblepOptionComponentList.Add(equipOptionComponent);
		}
		UIHelper.RepositionGrid(Grid_SelectAbleOption);
	}

	private List<EquipOptionData> GetEquipOptionDataList(int[] optionGroupIndices, Comparison<EquipOptionData> comparison)
	{
		Dictionary<int, EquipOptionData> dictionary = new Dictionary<int, EquipOptionData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(optionGroupIndices); i++)
		{
			int key = optionGroupIndices[i];
			EquipOptionGroupData data = GameRoot.Instance.EquipOptionGroupDataContainer.GetData(key);
			if (data == null)
			{
				continue;
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(data.OptionIndex); j++)
			{
				int key2 = data.OptionIndex[j];
				EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(key2);
				if (data2 != null)
				{
					dictionary[data2.Index] = data2;
				}
			}
		}
		List<EquipOptionData> list = new List<EquipOptionData>(dictionary.Values);
		list.Sort(comparison);
		return list;
	}

	private bool IsAbleConfirm()
	{
		if (__Extension_Method_ICollection__.COUNT(_selectedOptionComponentList) == 0)
		{
			return false;
		}
		bool num = __Extension_Method_ICollection__.COUNT(_selectedOptionComponentList) == __Extension_Method_ICollection__.COUNT(_slotOptionDic);
		bool flag = !_slotOptionDic.ContainsValue(0);
		return num && flag;
	}

	public void OnClickExitButton()
	{
		_slotOptionDic.Clear();
		_slotOptionDataList.Clear();
		if (_listener != null)
		{
			_listener.OnOptionChange(_slotOptionDic, _isExtraOption);
		}
		Close();
	}

	public void OnClickConfirm()
	{
		if (_listener != null)
		{
			_listener.OnOptionChange(_slotOptionDic, _isExtraOption);
		}
		Close();
	}

	public void OnClickOptionReset()
	{
		if (_slotOptionDic.Count >= 1 && _selectedSlotCount >= 1)
		{
			Reset();
			SetItemDescription(_itemData);
			InitSelectAbleOptionList();
		}
	}

	void EquipOptionSelectSlotComponent.Listener.OnSelectSlot(int slotIndex)
	{
	}

	private void SetSelectedOption(EquipOptionData selectedOptionData)
	{
		AddAndSort(selectedOptionData);
		DisableDuplicatedOptionComponent(selectedOptionData);
	}

	private void AddAndSort(EquipOptionData optionData)
	{
		SortSelectedOptionComponent(optionData);
		RefreshSlotOptionPair();
	}

	private void SortSelectedOptionComponent(EquipOptionData optionData)
	{
		_slotOptionDataList.Add(optionData);
		_slotOptionDataList.Sort((EquipOptionData l, EquipOptionData r) => l.ShowOptionIndex.CompareTo(r.ShowOptionIndex));
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_slotOptionDataList); num++)
		{
			int slotIndex = num + 1;
			_selectedOptionComponentList.Find((EquipOptionSelectSlotComponent e) => e.SlotIndex == slotIndex).OnOptionChanged(_slotOptionDataList[num]);
		}
	}

	private void RefreshSlotOptionPair()
	{
		_slotOptionDic.Clear();
		for (int i = 0; i < _slotOptionDataList.Count; i++)
		{
			if (_slotOptionDataList[i] != null)
			{
				int key = i + 1;
				_slotOptionDic.Add(key, _slotOptionDataList[i].Index);
			}
		}
	}

	private void DisableDuplicatedOptionComponent(EquipOptionData optionData)
	{
		if (!_equipItemData.SubData.EnableDuplicationOption && _slotOptionDic.ContainsValue(optionData.Index))
		{
			List<EquipOptionComponent> list = _selectAblepOptionComponentList.FindAll((EquipOptionComponent e) => e.EquipOptionData.Index == optionData.Index);
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
			{
				list[num].SetDisable(disabled: true);
			}
		}
	}

	void EquipOptionComponent.Listener.OnSelectOption(EquipOptionData optionData)
	{
		if (_selectedSlotCount >= _selectMaxCount)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(Inventory.SELECT_OPTION_NOTIFICATION_TEXT));
			return;
		}
		_selectedSlotIndex++;
		_selectedSlotCount++;
		SetSelectedOption(optionData);
		SetConfirmButton();
	}
}
