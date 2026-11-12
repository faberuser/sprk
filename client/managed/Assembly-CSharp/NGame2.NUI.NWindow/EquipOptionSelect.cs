using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipOptionSelect : WeaponUniqueSelect
{
	public new interface Listener
	{
		void OnUseOptionSelectItem(int useItemIndex, int equipItemIndex, int[] optionIndices, int[] extraOptionIndices);

		void OnClickSelectOption(ItemData optionItem, int selectMaxCount, bool isExtraOption);
	}

	[Serializable]
	public class ItemInfoUI
	{
		public GameObject RootObject;

		public UILabel Label_Description;

		public UIWidget StatRootWidget;

		public UILabel Label_SelectItem;

		public UIButton Btn_OptionSelect;

		public UIButton Btn_ExtraOptionSelect;
	}

	public new const string PrefabPath = "Prefab2/Window/Inventory/EquipOptionSelect";

	private Listener _listener;

	private int _MaxOptionCount;

	private int _maxExtraOptionCount = 1;

	private Dictionary<int, int> _slotOptionDics;

	private int[] _selectExtraOptionIndex;

	public ItemInfoUI EquipInfoUI;

	public ItemInfoUI TechnoMagicEquipInfoUI;

	public UIButton Btn_OptionSelect;

	public UIButton Btn_ExtraOptionSelect;

	public void Open(ItemData itemData, Listener listener)
	{
		Open();
		_listener = listener;
		_selectExtraOptionIndex = new int[_maxExtraOptionCount];
		InitInfoUI(itemData);
		Init(itemData);
		SetBuyButton();
		SetOptionSelectButton();
		if (SelectFilter != null)
		{
			SelectType = SelectFilter.SelectType.SELECT_EQUIP_OPTION;
			SelectFilter.InitFilter(SelectType, base.AddEquipItemByFilter);
			SetSubComboxPartType(itemData);
		}
		else
		{
			UIHelper.EnableGameObject(SelectFilter, enable: false);
		}
	}

	private void InitInfoUI(ItemData itemData)
	{
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(itemData);
		if (selectItemData != null)
		{
			if (selectItemData.SelectUniqueOption)
			{
				UIHelper.EnableGameObject(TechnoMagicEquipInfoUI.RootObject, enable: true);
				UIHelper.EnableGameObject(EquipInfoUI.RootObject, enable: false);
				ItemDesc.Label_Description = TechnoMagicEquipInfoUI.Label_Description;
				ItemDesc.StatRootWidget = TechnoMagicEquipInfoUI.StatRootWidget;
				ItemDesc.Label_SelectItem = TechnoMagicEquipInfoUI.Label_SelectItem;
				Btn_OptionSelect = TechnoMagicEquipInfoUI.Btn_OptionSelect;
				Btn_ExtraOptionSelect = TechnoMagicEquipInfoUI.Btn_ExtraOptionSelect;
			}
			else
			{
				UIHelper.EnableGameObject(TechnoMagicEquipInfoUI.RootObject, enable: false);
				UIHelper.EnableGameObject(EquipInfoUI.RootObject, enable: true);
				ItemDesc.Label_Description = EquipInfoUI.Label_Description;
				ItemDesc.StatRootWidget = EquipInfoUI.StatRootWidget;
				ItemDesc.Label_SelectItem = EquipInfoUI.Label_SelectItem;
				Btn_OptionSelect = EquipInfoUI.Btn_OptionSelect;
				Btn_ExtraOptionSelect = EquipInfoUI.Btn_ExtraOptionSelect;
			}
		}
	}

	private void SetSubComboxPartType(ItemData itemData)
	{
		if (itemData == null)
		{
			return;
		}
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(itemData);
		if (selectItemData == null || __Extension_Method_ICollection__.IsNullOrEmpty(selectItemData.ItemIndices))
		{
			return;
		}
		ItemData itemData2 = null;
		EquipItemData equipItemData = null;
		List<EquipPartType> list = new List<EquipPartType>();
		list.Clear();
		IEnumerator enumerator = selectItemData.ItemIndices.GetEnumerator();
		while (enumerator.MoveNext())
		{
			itemData2 = GameRoot.Instance.ItemDataContainer.GetData((int)enumerator.Current);
			if (itemData2 != null)
			{
				equipItemData = GameRoot.Instance.EquipItemDataContainer.GetData(itemData2.Code);
				if (list != null && !list.Contains(equipItemData.PartType))
				{
					list.Add(equipItemData.PartType);
				}
			}
		}
		if (SelectFilter != null)
		{
			SelectFilter.SetSpecificEquipPartType(list);
		}
	}

	protected override WeaponUniqueSelectItemData GetSelectItemData(ItemData itemData)
	{
		return GameRoot.Instance.WeaponUniqueSelectItemDataContainer.GetData(itemData.Code);
	}

	protected override void SetTitle(WeaponUniqueSelectItemData selectItemData)
	{
	}

	private bool IsAbleBuy()
	{
		if (_buyItemData == null)
		{
			return false;
		}
		if (GetSelectItemData(_useItemData).SelectUniqueOption)
		{
			for (int i = 0; i < _selectExtraOptionIndex.Length; i++)
			{
				if (GameRoot.Instance.EquipOptionDataContainer.GetData(_selectExtraOptionIndex[i]) == null)
				{
					return false;
				}
			}
			if (_selectExtraOptionIndex.Length == 0)
			{
				return false;
			}
		}
		if (_MaxOptionCount == 0)
		{
			return true;
		}
		if (_slotOptionDics == null)
		{
			return false;
		}
		if (_MaxOptionCount == __Extension_Method_ICollection__.COUNT(_slotOptionDics) && !_slotOptionDics.ContainsValue(0))
		{
			return true;
		}
		return false;
	}

	private void SetOptionSelectButton()
	{
		UIHelper.EnableButton(Btn_OptionSelect, IsAbleOptionSelect());
		UIHelper.EnableButton(Btn_ExtraOptionSelect, IsAbleExtraOptionSelect());
	}

	private bool IsAbleOptionSelect()
	{
		if (_MaxOptionCount == 0)
		{
			return false;
		}
		if (_buyItemData != null)
		{
			return true;
		}
		return false;
	}

	private bool IsAbleExtraOptionSelect()
	{
		if (_buyItemData == null)
		{
			return false;
		}
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(_useItemData);
		if (selectItemData == null)
		{
			return false;
		}
		if (!selectItemData.SelectUniqueOption)
		{
			return false;
		}
		return true;
	}

	private void SetBuyButton()
	{
		UIHelper.EnableButton(Button_Buy, IsAbleBuy());
	}

	public override void OnSelect(ItemData itemData)
	{
		if (_buyItemData == itemData)
		{
			return;
		}
		_buyItemData = itemData;
		_slotOptionDics = null;
		_selectExtraOptionIndex = new int[_maxExtraOptionCount];
		SetItemDescription(itemData);
		SetOptionSelectButton();
		SetBuyButton();
		foreach (WeaponUniqueItemComponent item in _shopItemComponent)
		{
			item.SetSelected(itemData.Index);
		}
	}

	public void SetOptionDictionary(Dictionary<int, int> slotOptionDics)
	{
		_slotOptionDics = slotOptionDics;
		SetOptions();
	}

	public void SetExtraOptionArray(int[] extraOptions)
	{
		if (extraOptions.Length == 0)
		{
			_selectExtraOptionIndex = new int[_maxExtraOptionCount];
		}
		else
		{
			_selectExtraOptionIndex = extraOptions;
		}
		SetOptions();
	}

	private void SetOptions()
	{
		SetItemDescription(_buyItemData);
		SetBuyButton();
	}

	protected override void SetupEquipItem(ItemData itemData)
	{
		UIHelper.EnableGameObject(ItemDesc.WidgetEquipSubInfo, enable: true);
		UIHelper.EnableGameObject(ItemDesc.Label_ItemType, enable: false);
		UIHelper.EnableGameObject(ItemDesc.StatRootWidget, enable: true);
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemData.Code);
		if (data == null)
		{
			return;
		}
		WeaponUniqueSelectItemData selectItemData = GetSelectItemData(_useItemData);
		if (selectItemData == null)
		{
			return;
		}
		SetupUniqueAndClass(data.SubData);
		UIHelper.SetLabelText(ItemDesc.Label_EquipLevel, (data.SubData.ReqLevel <= 0) ? 1 : data.SubData.ReqLevel);
		StatData statData = new StatData();
		statData.AddEquipItemStat(data.SubData, selectItemData.Star, data.SubData.DetailIndex, selectItemData.Level);
		_statListComponent = StatListComponent.Create(ItemDesc.StatRootWidget, 5);
		_statListComponent.AddStat(statData);
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		int maxSelectCount = GetMaxSelectCount();
		int num = data2?.OptionCount ?? 0;
		if (maxSelectCount > num)
		{
			VespaDebug.LogError("MaxSelectCount > MaxOptionCount");
		}
		_MaxOptionCount = ((maxSelectCount > num) ? num : maxSelectCount);
		_statListComponent.AddOptionSubject(LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION));
		if (_MaxOptionCount != 0)
		{
			for (int i = 1; i <= num; i++)
			{
				string desc = string.Empty;
				if (!TryGetOptionDesc(i, out desc))
				{
					desc = LocalizationManager.GetString(Equip.EQUIP_OPTION_STAT_TEXT);
				}
				_statListComponent.AddCustomStat(desc, null, _statListComponent.OptionClosedStatValueColor, null, isResizable: false, SingleStatInfo.StatIconType.Normal, shirinkContents: true);
			}
		}
		else
		{
			string text = "";
			text = ((data.SubData.EquipType != EquipItemEquipType.TechnoMagic) ? LocalizationManager.GetStringDirect("SELECT_UNIQUE_ITEM_RANDOM_4_OPTION_TEXT") : LocalizationManager.GetStringDirect("SELECT_UNIQUE_ITEM_RANDOM_2_OPTION_TEXT"));
			_statListComponent.AddCustomStat(text, null, null, null, isResizable: false, SingleStatInfo.StatIconType.Normal, shirinkContents: true);
			for (int j = 2; j <= num; j++)
			{
				_statListComponent.AddCustomStat(string.Empty, null, null, null, isResizable: false, SingleStatInfo.StatIconType.Normal, shirinkContents: true, enableIcon: false);
			}
		}
		if (data.SubData.EquipType == EquipItemEquipType.TechnoMagic)
		{
			_statListComponent.AddOptionSubject(LocalizationManager.GetStringDirect("EQUIP_OPTION_ADDITIONALSKILL"));
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_selectExtraOptionIndex); k++)
			{
				string text2 = "";
				EquipOptionData data3 = GameRoot.Instance.EquipOptionDataContainer.GetData(_selectExtraOptionIndex[k]);
				if (data3 != null)
				{
					OperationType optionType = data3.OptionType;
					int optionValue = data3.GetOptionValue(selectItemData.Star, 0);
					text2 = StatData.GetOptionDisplay(optionType, optionValue);
				}
				else
				{
					text2 = LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION);
				}
				_statListComponent.AddCustomStat(text2, null, _statListComponent.OptionClosedStatValueColor, null, isResizable: false, SingleStatInfo.StatIconType.Normal, shirinkContents: true);
			}
			if (__Extension_Method_ICollection__.COUNT(_selectExtraOptionIndex) == 0)
			{
				_statListComponent.AddCustomStat(LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION), null, _statListComponent.OptionClosedStatValueColor, null, isResizable: false, SingleStatInfo.StatIconType.Normal, shirinkContents: true);
			}
		}
		StatListComponent.AddOptionStat(_statListComponent, data.GetSubData<EquipItemData>(), null);
		StatListComponent.AddExtraOptionStat(_statListComponent, null);
		StatListComponent.AddSetOptionStat(_statListComponent, null, data.GetSubData<EquipItemData>(), null);
	}

	private bool TryGetOptionDesc(int slotIndex, out string desc)
	{
		desc = string.Empty;
		int value = 0;
		if (_slotOptionDics != null && _slotOptionDics.TryGetValue(slotIndex, out value) && value != 0)
		{
			int equipItemOptionLevel = GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionLevel(slotIndex);
			LocalizationManager.GetStringDirect("EQUIP_GRADE_STAT_MASSAGE", equipItemOptionLevel);
			desc = GetSelectedOptionString(value);
			return true;
		}
		return false;
	}

	private string GetSelectedOptionString(int optionIndex)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			return string.Empty;
		}
		int optionValueMin = data.GetOptionValueMin(0);
		int optionValueMax = data.GetOptionValueMax(0);
		return StatData.GetOptionDisplay(data.OptionType, optionValueMin, optionValueMax, "~");
	}

	private int GetMaxSelectCount()
	{
		return GameRoot.Instance.WeaponUniqueSelectItemDataContainer.GetData(_useItemData.Code)?.SelectOptionCount ?? 0;
	}

	private int[] GetOptionAryFromDics(Dictionary<int, int> slotOptionDics)
	{
		if (slotOptionDics == null)
		{
			return null;
		}
		SortedDictionary<int, int> sortedDictionary = new SortedDictionary<int, int>(slotOptionDics);
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, int> item in sortedDictionary)
		{
			list.Add(item.Value);
		}
		return list.ToArray();
	}

	public new void OnClickBuy()
	{
		if (_listener != null)
		{
			int[] array = GetOptionAryFromDics(_slotOptionDics);
			if (array == null && _MaxOptionCount == 0)
			{
				array = new int[0];
			}
			_listener.OnUseOptionSelectItem(_useItemData.Index, _buyItemData.Index, array, _selectExtraOptionIndex);
		}
	}

	public void OnClickSelectOption()
	{
		if (_listener != null)
		{
			_listener.OnClickSelectOption(_buyItemData, _MaxOptionCount, isExtraOption: false);
		}
	}

	public void OnClickSelectExtraOption()
	{
		if (_listener != null)
		{
			_listener.OnClickSelectOption(_buyItemData, _maxExtraOptionCount, isExtraOption: true);
		}
	}
}
