using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RuneSelectItemUsePopup : BaseWindow
{
	public interface IListener
	{
		void OnClickConfirm(ItemData usingItemData, int itemCount, Action onComplete);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/RuneSelectItemUsePopup";

	public UILabel LabelTitle;

	public UILabel LabelContentDescription;

	public UISimpleGrid GridUsingItem;

	public UILabel LabelSelectedCount;

	public UIInput InputSelectedCount;

	public UIButton ButtonPlus;

	public UIButton ButtonMinus;

	public UIButton ButtonConfirm;

	private IListener _listener;

	private int _currentCount = 1;

	private int _usingItemMaxCount = 1;

	private ItemData _usingItemData;

	private ItemIconComponent _usingItemIconComponent;

	private List<SelectionPopupItemInfo> _selectAbleItems = new List<SelectionPopupItemInfo>();

	public void Open(ItemData itemData, int itemCount, IListener listener)
	{
		Open(delegate
		{
			Init(itemData, itemCount, listener);
		}, null);
	}

	public override void Reset()
	{
		base.Reset();
		_currentCount = 1;
		_listener = null;
		_usingItemData = null;
		_usingItemMaxCount = 1;
		if (InputSelectedCount != null)
		{
			InputSelectedCount.value = "1";
		}
		FactoryBase.SafeReleaseComponent(ref _usingItemIconComponent);
		UIHelper.EnableButton(ButtonMinus, enable: false);
		UIHelper.EnableButton(ButtonPlus, enable: true);
		UIHelper.EnableButton(ButtonPlus, enable: false);
	}

	private void Init(ItemData usingItemData, int usingItemMaxCount, IListener listener)
	{
		base.Open();
		_listener = listener;
		_usingItemData = usingItemData;
		_usingItemMaxCount = usingItemMaxCount;
		UpdateTitle(usingItemData);
		UpdateUsingItemIcon(usingItemData, usingItemMaxCount);
		UpdateCount();
	}

	private void UpdateTitle(ItemData usingItemData)
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		UIHelper.SetLabelText(LabelTitle, text);
		string text2 = string.Empty;
		if (usingItemData != null)
		{
			text2 = LocalizationManager.GetString(ItemUse.INVEN_USE_BOOSTER, usingItemData.Name);
		}
		UIHelper.SetLabelText(LabelContentDescription, text2);
	}

	private void UpdateUsingItemIcon(ItemData usingItemData, int usingItemMaxCount)
	{
		if (usingItemData != null)
		{
			if (_usingItemIconComponent != null)
			{
				FactoryBase.SafeReleaseComponent(ref _usingItemIconComponent);
			}
			_usingItemIconComponent = UIHelper.AddGridItem(GridUsingItem, ItemIconComponent.PrefabAuto);
			if (!(_usingItemIconComponent == null))
			{
				_usingItemIconComponent.Open(usingItemData);
				_usingItemIconComponent.SetItemCount(usingItemMaxCount);
			}
		}
	}

	private void UpdateCount()
	{
		UIHelper.SetLabelText(LabelSelectedCount, _currentCount);
		UpdatePlusMinusButtonState();
		UpdateConfirmButtonState();
		InputSelectedCount.value = _currentCount.ToString();
	}

	public void OnClickConfirm()
	{
		if (_listener != null)
		{
			_listener.OnClickConfirm(_usingItemData, _currentCount, Close);
		}
	}

	private void UpdatePlusMinusButtonState()
	{
		UIHelper.EnableButton(ButtonMinus, _currentCount > 1);
		UIHelper.EnableButton(ButtonPlus, _currentCount < _usingItemMaxCount);
	}

	private void UpdateConfirmButtonState()
	{
		bool enable = true;
		if (_currentCount < 1 || _currentCount > _usingItemMaxCount)
		{
			enable = false;
		}
		UIHelper.EnableButton(ButtonConfirm, enable);
	}

	public void OnChangeInputSelectCount()
	{
		if (!(InputSelectedCount == null))
		{
			int.TryParse(InputSelectedCount.value, out _currentCount);
			_currentCount = Mathf.Clamp(_currentCount, 0, _usingItemMaxCount);
			UpdateCount();
		}
	}

	public void OnClickPlus()
	{
		if (_currentCount + 1 <= _usingItemMaxCount)
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
		_currentCount = _usingItemMaxCount;
		UpdateCount();
	}
}
