using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class UseMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/UseMessagePopup";

	public GameObject UseEffect;

	public UIWidget WidgetUnLockItemUse;

	public GameObject RecoverStamina;

	public UILabel UseValueLabel;

	public UISimpleGrid UnlockItemGrid;

	public UIButton Button_OK;

	private Action _onYes;

	private Action _onNo;

	private ItemIconComponent _UnlockItemComponent;

	public static void Open(ItemActionType actionType, int actionValue, Action onYes, Action onNo = null)
	{
		WindowPrefab<UseMessagePopup> windowPrefab = new WindowPrefab<UseMessagePopup>("Resources/Prefab2/Window/MessagePopup/UseMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(actionType, actionValue, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenUseToUnlockSeal(ItemData itemData, int itemCount, bool CanUse, Action onYes, Action onNo = null)
	{
		WindowPrefab<UseMessagePopup> windowPrefab = new WindowPrefab<UseMessagePopup>("Resources/Prefab2/Window/MessagePopup/UseMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(itemData, itemCount, CanUse, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(ItemData itemData, int itemCount, bool CanUse, Action onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		UIHelper.EnableGameObject(UseEffect, enable: false);
		UIHelper.EnableGameObject(WidgetUnLockItemUse, enable: true);
		_UnlockItemComponent = UIHelper.AddGridItem(UnlockItemGrid, ItemIconComponent.PrefabAuto);
		_UnlockItemComponent.Open(itemData);
		ItemIconComponent unlockItemComponent = _UnlockItemComponent;
		Color? color = (CanUse ? Color.white : Color.red);
		unlockItemComponent.SetItemCount(itemCount, color);
		UIHelper.EnableButton(Button_OK, CanUse);
	}

	protected void OpenInternal(ItemActionType actionType, int actionValue, Action onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		UIHelper.EnableGameObject(UseEffect, enable: true);
		UIHelper.EnableGameObject(WidgetUnLockItemUse, enable: false);
		UIHelper.EnableGameObject(RecoverStamina, actionType == ItemActionType.RecoverStamina);
		UIHelper.EnableGameObject(UseValueLabel, actionValue > 0);
		UIHelper.SetLabelText(UseValueLabel, actionValue);
	}

	public override void Reset()
	{
		base.Reset();
		if (_UnlockItemComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _UnlockItemComponent);
			_UnlockItemComponent = null;
		}
		UIHelper.EnableButton(Button_OK, enable: true);
	}

	public void OnClickYes()
	{
		Action onYes = _onYes;
		Close();
		Vespa_System_Action_Extension_Method.fire(onYes);
	}

	public void OnClickNo()
	{
		Action onNo = _onNo;
		Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
	}
}
