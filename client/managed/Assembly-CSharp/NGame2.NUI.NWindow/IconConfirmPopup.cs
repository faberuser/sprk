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

public class IconConfirmPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/IconConfirmPopup";

	public const string NewPrefabPath = "Resources/Prefab2/Window/PayShop/PayShopIconConfirmPopup";

	public const string GodKingPrefabPath = "Resources/Prefab2/Window/MessagePopup/GodKingIconConfirmPopup";

	public UIScrollGrid IconGrid;

	public UILabel MessageLabel;

	public UILabel ConfirmButtonLabel;

	public UILabel CancelButtonLabel;

	public float ItemIconScale = 1f;

	public float HeroIconScale = 1f;

	private List<Component> _iconComponentList = new List<Component>();

	private Action _onConfirm;

	private Action _onCancel;

	private bool _manualClose;

	public static IconConfirmPopup Open(string message, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel = null)
	{
		WindowPrefab<IconConfirmPopup> windowPrefab = new WindowPrefab<IconConfirmPopup>("Resources/Prefab2/Window/MessagePopup/IconConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(message, confirmLabel, cancelLabel, onConfirm, onCancel);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	public static IconConfirmPopup NewOpen(string message, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel = null)
	{
		WindowPrefab<IconConfirmPopup> windowPrefab = new WindowPrefab<IconConfirmPopup>("Resources/Prefab2/Window/PayShop/PayShopIconConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(message, confirmLabel, cancelLabel, onConfirm, onCancel);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	public static IconConfirmPopup GodKingStyleOpen(string message, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel = null)
	{
		WindowPrefab<IconConfirmPopup> windowPrefab = new WindowPrefab<IconConfirmPopup>("Resources/Prefab2/Window/MessagePopup/GodKingIconConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(message, confirmLabel, cancelLabel, onConfirm, onCancel);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(string message, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel)
	{
		base.Open();
		_onConfirm = onConfirm;
		_onCancel = onCancel;
		UIHelper.SetLabelText(ConfirmButtonLabel, confirmLabel);
		UIHelper.SetLabelText(CancelButtonLabel, cancelLabel);
		UIHelper.SetLabelText(MessageLabel, message);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_iconComponentList);
		_onConfirm = null;
		_onCancel = null;
		_manualClose = false;
	}

	public void AddItem(int itemIndex, int count)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.transform.localScale = Vector3.one * ItemIconScale;
				itemIconComponent.Open(data);
				itemIconComponent.SetItemCount(count);
				_iconComponentList.Add(itemIconComponent);
				UIHelper.RepositionGrid(IconGrid);
			}
		}
	}

	public void SetConfirmCallback(Action onConfirm)
	{
		_onConfirm = onConfirm;
	}

	public void SetCancelCallback(Action onCancel)
	{
		_onCancel = onCancel;
	}

	public void AddEquipItem(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.transform.localScale = Vector3.one * ItemIconScale;
				itemIconComponent.Open(equipItemInfo, null);
				_iconComponentList.Add(itemIconComponent);
				UIHelper.RepositionGrid(IconGrid);
			}
		}
	}

	public void AddHero(int heroIndex, int star = 0, int transcend = 0)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(IconGrid, HeroPortraitComponent.SmallPrefabAuto);
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.transform.localScale = Vector3.one * HeroIconScale;
				heroPortraitComponent.Open(data, null, HeroPortraitComponent.EPortraitMode.Icon, star, transcend);
				heroPortraitComponent.ShowCreatureName();
				_iconComponentList.Add(heroPortraitComponent);
				UIHelper.RepositionGrid(IconGrid);
			}
		}
	}

	public void SetManualClose(bool manualClose)
	{
		_manualClose = manualClose;
	}

	public void OnClickConfirm()
	{
		Action onConfirm = _onConfirm;
		_onConfirm = null;
		if (!_manualClose)
		{
			Close();
		}
		Vespa_System_Action_Extension_Method.fire(onConfirm);
	}

	public void OnClickCancel()
	{
		Action onCancel = _onCancel;
		_onCancel = null;
		Close();
		Vespa_System_Action_Extension_Method.fire(onCancel);
	}
}
