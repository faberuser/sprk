using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectItemMessagePopup : BaseWindow, ItemIconComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SelectItemMessagePopup";

	public const string PrefabPath2 = "Resources/Prefab2/Window/Inventory/SelectItemMessagePopup2";

	public UILabel Label_Title;

	public UILabel Label_Content;

	public UIButton Button_Yes;

	public UIButton Button_IncCount;

	public UIButton Button_DecCount;

	public UIButton Button_Max;

	public UILabel Label_SelectCount;

	public UIInput Input_SelectCount;

	public UIScrollGrid Grid_Item;

	public Transform GemRoot;

	public UILabel Label_GemCount;

	private List<ItemIconComponent> _itemList = new List<ItemIconComponent>();

	private Action<SelectItemMessagePopup, int, int> _onYes;

	private Action _onCancel;

	private int _selectedIdx;

	private long _selectedCount;

	private List<Tuple<ItemData, int>> _itemCountList;

	private bool _closeOnYes;

	private bool _singleSelect;

	private bool _useCount;

	private bool _disableMaxButton;

	private int _gem;

	private int _selectCountMax;

	public static void Open(string title, string content, List<Tuple<ItemData, int>> itemList, Action<SelectItemMessagePopup, int, int> onYes, Action onCancel = null, bool closeOnYes = true)
	{
		WindowPrefab<SelectItemMessagePopup> windowPrefab = new WindowPrefab<SelectItemMessagePopup>("Resources/Prefab2/Window/MessagePopup/SelectItemMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, content, itemList, onYes, onCancel, closeOnYes, 0);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenWithDisableMaxButton(string title, string content, List<Tuple<ItemData, int>> itemList, Action<SelectItemMessagePopup, int, int> onYes, int selectCountMax)
	{
		WindowPrefab<SelectItemMessagePopup> windowPrefab = new WindowPrefab<SelectItemMessagePopup>("Resources/Prefab2/Window/MessagePopup/SelectItemMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, content, itemList, onYes, null, closeOnYes: true, 0, useCount: true, disableMaxButton: true, selectCountMax);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenWithGemPrice(string title, string content, List<Tuple<ItemData, int>> itemList, int gem, Action<SelectItemMessagePopup, int, int> onYes, Action onCancel = null, bool closeOnYes = true)
	{
		WindowPrefab<SelectItemMessagePopup> windowPrefab = new WindowPrefab<SelectItemMessagePopup>("Resources/Prefab2/Window/MessagePopup/SelectItemMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, content, itemList, onYes, onCancel, closeOnYes, gem);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open2(string title, string content, List<Tuple<ItemData, int>> itemList, Action<SelectItemMessagePopup, int, int> onYes, Action onCancel = null, bool closeOnYes = true)
	{
		WindowPrefab<SelectItemMessagePopup> windowPrefab = new WindowPrefab<SelectItemMessagePopup>("Resources/Prefab2/Window/Inventory/SelectItemMessagePopup2", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, content, itemList, onYes, onCancel, closeOnYes, 0, useCount: false);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string title, string content, List<Tuple<ItemData, int>> itemCountList, Action<SelectItemMessagePopup, int, int> onYes, Action onCancel, bool closeOnYes, int gem, bool useCount = true, bool disableMaxButton = false, int itemSelectCountMax = 0)
	{
		base.Open();
		_onYes = onYes;
		_onCancel = onCancel;
		_selectedIdx = 0;
		_useCount = useCount;
		_gem = gem;
		_disableMaxButton = disableMaxButton;
		_selectCountMax = itemSelectCountMax;
		if (_useCount)
		{
			int num = 0;
			_selectedCount = num;
		}
		_closeOnYes = closeOnYes;
		_singleSelect = false;
		UIHelper.SetLabelText(Label_Title, title);
		UIHelper.SetLabelText(Label_Content, content);
		UIHelper.EnableButton(Button_Yes, enable: false);
		UIHelper.EnableGameObject(Button_Max, !_disableMaxButton);
		FactoryBase.ReleaseAndClearList(_itemList);
		Tuple<ItemData, int>[] array = itemCountList.ToArray();
		Array.Sort(array, delegate(Tuple<ItemData, int> lhs, Tuple<ItemData, int> rhs)
		{
			ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(lhs.Item1.Index);
			ItemData<PotionItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(rhs.Item1.Index);
			if (data != null || data2 != null)
			{
				if (data.SubData.ActionValue < data2.SubData.ActionValue)
				{
					return -1;
				}
				if (data.SubData.ActionValue > data2.SubData.ActionValue)
				{
					return 1;
				}
				return 0;
			}
			if (lhs.Item1.Index < rhs.Item1.Index)
			{
				return -1;
			}
			return (lhs.Item1.Index > rhs.Item1.Index) ? 1 : 0;
		});
		Tuple<ItemData, int>[] array2 = array;
		foreach (Tuple<ItemData, int> tuple in array2)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_Item, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(tuple.Item1, this, isShowInvenItemCount: true);
			int item = tuple.Item2;
			Color? color = null;
			int num3 = item;
			itemIconComponent.SetItemCount(num3, color);
			itemIconComponent.gameObject.SetActive(value: true);
			_itemList.Add(itemIconComponent);
			if (!_useCount)
			{
				int item2 = tuple.Item2;
				_selectedCount = item2;
			}
		}
		_itemCountList = itemCountList;
		if (__Extension_Method_ICollection__.COUNT(_itemList) == 1)
		{
			_singleSelect = true;
			if (_useCount)
			{
				_selectedIdx = _itemList[0].ItemIndex;
				int num4 = 1;
				_selectedCount = num4;
			}
			else
			{
				_selectedIdx = _itemList[0].ItemIndex;
				_selectedCount = _itemList[0].ItemCount;
			}
			UIHelper.EnableButton(Button_Yes, enable: true);
		}
		UpdateCountButtonState(updateInput: true);
		if ((bool)Input_SelectCount)
		{
			Input_SelectCount.selectAllTextOnFocus = true;
		}
		if (Grid_Item != null && (bool)Grid_Item.Grid)
		{
			if (__Extension_Method_ICollection__.COUNT(array) > Grid_Item.Grid.maxPerLine)
			{
				UIHelper.RepositionGrid(Grid_Item);
			}
			else
			{
				Grid_Item.Grid.Reposition();
			}
		}
		UIHelper.EnableGameObject(Button_Yes, gem <= 0);
		UIHelper.EnableGameObject(GemRoot, gem > 0);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_itemList);
		_itemCountList = null;
	}

	public void OnClickYes()
	{
		long selectedCount = _selectedCount;
		int num = 1000;
		_selectedCount = Math.Min(selectedCount, num);
		Action<SelectItemMessagePopup, int, int> onYes = _onYes;
		if (_closeOnYes)
		{
			Close();
		}
		int selectedIdx = _selectedIdx;
		long selectedCount2 = _selectedCount;
		Vespa_System_Action_Extension_Method.fire(onYes, this, selectedIdx, checked((int)selectedCount2));
	}

	public void OnClickCancel()
	{
		Action onCancel = _onCancel;
		Close();
		Vespa_System_Action_Extension_Method.fire(onCancel);
	}

	private int GetItemSelectableCount(int itemIndex)
	{
		int num = 0;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_itemCountList); i++)
		{
			Tuple<ItemData, int> tuple = _itemCountList[i];
			if (tuple.Item1 != null && tuple.Item1.Index == itemIndex)
			{
				num = tuple.Item2;
				break;
			}
		}
		if (_selectCountMax == 0)
		{
			return Mathf.Min(num, 1000);
		}
		return Mathf.Min(Math.Min(num, _selectCountMax), 1000);
	}

	private void UpdateCountButtonState(bool updateInput)
	{
		if (_selectedIdx == 0)
		{
			UIHelper.EnableButton(Button_IncCount, enable: false);
			UIHelper.EnableButton(Button_DecCount, enable: false);
			if (updateInput && Input_SelectCount != null)
			{
				Input_SelectCount.value = "0";
			}
			return;
		}
		int itemSelectableCount = GetItemSelectableCount(_selectedIdx);
		long selectedCount = _selectedCount;
		int num = itemSelectableCount;
		bool enable = selectedCount < num;
		long selectedCount2 = _selectedCount;
		int num2 = 1;
		bool enable2 = selectedCount2 > num2;
		UIHelper.EnableButton(Button_IncCount, enable);
		UIHelper.EnableButton(Button_DecCount, enable2);
		if (updateInput && Input_SelectCount != null)
		{
			Input_SelectCount.value = _selectedCount.ToString();
		}
		UILabel label_GemCount = Label_GemCount;
		long selectedCount3 = _selectedCount;
		int gem = _gem;
		UIHelper.SetLabelText(label_GemCount, checked(selectedCount3 * gem));
	}

	public void OnClickIncCount()
	{
		int itemSelectableCount = GetItemSelectableCount(_selectedIdx);
		long selectedCount = _selectedCount;
		int num = 1;
		checked
		{
			long num2 = selectedCount + num;
			int max = itemSelectableCount;
			int min = 1;
			long num3 = num2;
			int num4 = Mathf.Clamp((int)num3, min, max);
			_selectedCount = num4;
			UpdateCountButtonState(updateInput: true);
		}
	}

	public void OnClickDecCount()
	{
		int itemSelectableCount = GetItemSelectableCount(_selectedIdx);
		long selectedCount = _selectedCount;
		int num = 1;
		checked
		{
			long num2 = selectedCount - num;
			int max = itemSelectableCount;
			int min = 1;
			long num3 = num2;
			int num4 = Mathf.Clamp((int)num3, min, max);
			_selectedCount = num4;
			UpdateCountButtonState(updateInput: true);
		}
	}

	public void OnClickMaxCount()
	{
		int itemSelectableCount = GetItemSelectableCount(_selectedIdx);
		int num = itemSelectableCount;
		_selectedCount = num;
		UpdateCountButtonState(updateInput: true);
	}

	public void OnChangeInput_SelectCount()
	{
		if (!(Input_SelectCount == null))
		{
			int itemSelectableCount = GetItemSelectableCount(_selectedIdx);
			int num = Mathf.Clamp(Vespa_String_Extension_Method.TOINT(Input_SelectCount.value, 0), 1, itemSelectableCount);
			_selectedCount = num;
			UpdateCountButtonState(updateInput: true);
		}
	}

	public void OnSubmitInput_SelectCount()
	{
		if (!(Input_SelectCount == null))
		{
			int itemSelectableCount = GetItemSelectableCount(_selectedIdx);
			int num = Mathf.Clamp(Vespa_String_Extension_Method.TOINT(Input_SelectCount.value, 0), 1, itemSelectableCount);
			_selectedCount = num;
			UpdateCountButtonState(updateInput: true);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_singleSelect)
		{
			return;
		}
		foreach (ItemIconComponent item in _itemList)
		{
			if (item.ItemIndex == itemIconComponent.ItemIndex)
			{
				item.SetSelected(selected: true);
				_selectedIdx = item.ItemIndex;
				UIHelper.EnableButton(Button_Yes, enable: true);
				if (_useCount)
				{
					int num = 1;
					_selectedCount = num;
				}
				else
				{
					_selectedCount = item.ItemCount;
				}
				UpdateCountButtonState(updateInput: true);
			}
			else
			{
				item.SetSelected(selected: false);
			}
		}
	}
}
