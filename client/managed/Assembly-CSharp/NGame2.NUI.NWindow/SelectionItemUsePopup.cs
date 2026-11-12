using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectionItemUsePopup : BaseWindow, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OnClickOk(SelectionItemUsePopup popup, int usingItemIndex, int selecteditemIndex, int itemCount);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/SelectionItemUsePopup";

	public UILabel LabelTitle;

	public UILabel LabelContentDescription;

	public UISimpleGrid GridUsingItem;

	public UIScrollGrid GridSelectableItem;

	public UILabel LabelSelectedCount;

	public UIInput InputSelectedCount;

	public UIButton ButtonPlus;

	public UIButton ButtonMinus;

	public UIButton ButtonOk;

	private int _currentCount = 1;

	private int _selectedItemIndex;

	private SelectionPopupItemInfo _usingItemData;

	private ItemIconComponent _usingItem;

	private List<ItemIconComponent> _selectableItems = new List<ItemIconComponent>();

	private IListener _listener;

	public static void Open(SelectionPopupItemInfo usingItem, List<SelectionPopupItemInfo> selectableItems, IListener listener = null)
	{
		WindowPrefab<SelectionItemUsePopup> windowPrefab = new WindowPrefab<SelectionItemUsePopup>("Resources/Prefab2/Window/MessagePopup/SelectionItemUsePopup", GameUIRoot.ELayerType.UI);
		if (!(windowPrefab.instance == null))
		{
			windowPrefab.instance.OpenInternal(usingItem, selectableItems, listener);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	protected override void OnDestroy()
	{
		_listener = null;
		base.OnDestroy();
	}

	private void OpenInternal(SelectionPopupItemInfo usingItemData, List<SelectionPopupItemInfo> selectableItems, IListener listener = null)
	{
		base.Open();
		_listener = listener;
		_usingItemData = usingItemData;
		UpdateTitle();
		UpdateUsingItemIcon(usingItemData);
		UpdateCount();
		SortSelectableItems(ref selectableItems);
		UpdateSelectableItemGrid(selectableItems);
		UpdateBtnOkState();
		SprkHeroMaterialTickets.Prefill(this, listener, _selectableItems);
	}

	public override void Reset()
	{
		base.Reset();
		_currentCount = 1;
		_selectedItemIndex = 0;
		_usingItemData = null;
		if (InputSelectedCount != null)
		{
			InputSelectedCount.value = "1";
		}
		FactoryBase.SafeReleaseComponent(ref _usingItem);
		FactoryBase.ReleaseAndClearList(_selectableItems);
		UIHelper.EnableButton(ButtonMinus, enable: false);
		UIHelper.EnableButton(ButtonPlus, enable: true);
		UIHelper.EnableButton(ButtonPlus, enable: false);
	}

	private void UpdateTitle()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		UIHelper.SetLabelText(LabelTitle, text);
		string text2 = string.Empty;
		if (_usingItemData != null && _usingItemData.Data != null)
		{
			text2 = LocalizationManager.GetString(ItemUse.INVEN_USE_BOOSTER, _usingItemData.Data.Name);
		}
		UIHelper.SetLabelText(LabelContentDescription, text2);
	}

	private void UpdateUsingItemIcon(SelectionPopupItemInfo itemData)
	{
		if (itemData != null)
		{
			if (_usingItem != null)
			{
				FactoryBase.SafeReleaseComponent(ref _usingItem);
			}
			_usingItem = AddItem(itemData.Data, itemData.Count, GridUsingItem, this);
		}
	}

	private void UpdateCount()
	{
		UIHelper.SetLabelText(LabelSelectedCount, _currentCount);
		UpdatePlusMinusButtonState();
		UpdateBtnOkState();
		InputSelectedCount.value = _currentCount.ToString();
	}

	private void SortSelectableItems(ref List<SelectionPopupItemInfo> selectableItemDataList)
	{
		if (selectableItemDataList == null)
		{
			return;
		}
		selectableItemDataList.Sort(delegate(SelectionPopupItemInfo itemL, SelectionPopupItemInfo itemR)
		{
			ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(itemL.Data.Index);
			ItemData<PotionItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(itemR.Data.Index);
			if (data == null && data2 == null)
			{
				if (itemL.Data.Index < itemR.Data.Index)
				{
					return -1;
				}
				if (itemL.Data.Index > itemR.Data.Index)
				{
					return 1;
				}
				return 0;
			}
			if (data == null)
			{
				return 1;
			}
			if (data2 == null)
			{
				return -1;
			}
			if (data.SubData.ActionValue < data2.SubData.ActionValue)
			{
				return -1;
			}
			return (data.SubData.ActionValue > data2.SubData.ActionValue) ? 1 : 0;
		});
	}

	private void UpdateSelectableItemGrid(List<SelectionPopupItemInfo> selectableItemDataList)
	{
		if (selectableItemDataList == null || GridSelectableItem == null)
		{
			return;
		}
		int i = 0;
		for (int count = selectableItemDataList.Count; i < count; i++)
		{
			SelectionPopupItemInfo selectionPopupItemInfo = selectableItemDataList[i];
			if (selectionPopupItemInfo != null)
			{
				ItemIconComponent itemIconComponent = AddItem(selectionPopupItemInfo.Data, selectionPopupItemInfo.Count, GridSelectableItem, this, showInvenCount: true);
				if (!(itemIconComponent == null))
				{
					_selectableItems.Add(itemIconComponent);
				}
			}
		}
		if (GridSelectableItem.ScrollView != null)
		{
			if (IsOverFlowClippingRegion())
			{
				GridSelectableItem.ScrollView.contentPivot = UIWidget.Pivot.Left;
			}
			else
			{
				GridSelectableItem.ScrollView.contentPivot = UIWidget.Pivot.Center;
			}
		}
		GridSelectableItem.RepositionGrid();
	}

	private bool IsOverFlowClippingRegion()
	{
		if (__Extension_Method_ICollection__.COUNT(_selectableItems) > 7)
		{
			return true;
		}
		return false;
	}

	private void UpdatePlusMinusButtonState()
	{
		UIHelper.EnableButton(ButtonMinus, _currentCount > 1);
		if (_usingItemData != null)
		{
			UIHelper.EnableButton(ButtonPlus, _currentCount < Mathf.Min(_usingItemData.Count, 1000));
		}
	}

	private void UpdateBtnOkState()
	{
		bool enable = true;
		if (ButtonOk == null)
		{
			enable = false;
		}
		if (_selectedItemIndex == 0)
		{
			enable = false;
		}
		if (_currentCount < 1 || _currentCount > Mathf.Min(_usingItemData.Count, 1000))
		{
			enable = false;
		}
		UIHelper.EnableButton(ButtonOk, enable);
	}

	private ItemIconComponent AddItem(ItemData iData, int count, IGrid gridUI, ItemIconComponent.Listener onClickListener = null, bool showInvenCount = false)
	{
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(gridUI, ItemIconComponent.PrefabAuto);
		if (itemIconComponent == null)
		{
			return null;
		}
		itemIconComponent.Open(iData, onClickListener, showInvenCount);
		itemIconComponent.gameObject.SetActive(value: true);
		if (!showInvenCount)
		{
			itemIconComponent.SetItemCount(count);
		}
		return itemIconComponent;
	}

	public void OnClickPlus()
	{
		if (_usingItemData != null && _currentCount + 1 <= Mathf.Min(_usingItemData.Count, 1000))
		{
			_currentCount++;
			UpdateCount();
		}
	}

	public void OnClickMinus()
	{
		if (_currentCount - 1 >= 1)
		{
			_currentCount--;
			UpdateCount();
		}
	}

	public void OnClickMax()
	{
		if (_usingItemData != null)
		{
			_currentCount = Mathf.Min(_usingItemData.Count, 1000);
			UpdateCount();
		}
	}

	public void OnClickOk()
	{
		if (_selectedItemIndex != 0 && _usingItemData != null && _usingItemData.Data != null && _listener != null)
		{
			_listener.OnClickOk(this, _usingItemData.Data.Index, _selectedItemIndex, _currentCount);
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnChangeInputSelectCount()
	{
		if (!(InputSelectedCount == null) && _usingItemData != null)
		{
			int.TryParse(InputSelectedCount.value, out _currentCount);
			_currentCount = Mathf.Clamp(_currentCount, 0, Mathf.Min(_usingItemData.Count, 1000));
			UpdateCount();
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent selectedItemComp)
	{
		if (selectedItemComp.ItemData == _usingItemData.Data)
		{
			return;
		}
		if (selectedItemComp != null)
		{
			_selectedItemIndex = selectedItemComp.ItemIndex;
			selectedItemComp.SetSelected(selected: true);
		}
		int i = 0;
		for (int count = _selectableItems.Count; i < count; i++)
		{
			ItemIconComponent itemIconComponent = _selectableItems[i];
			if (!(itemIconComponent == null) && selectedItemComp != itemIconComponent)
			{
				itemIconComponent.SetSelected(selected: false);
			}
		}
		UpdateBtnOkState();
	}
}
