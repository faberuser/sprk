using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipConfirmPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/EquipConfirmPopup";

	public UILabel MessageLabel;

	public Transform IconRoot;

	public UILabel NameLabel;

	public UILabel ConfirmButtonLabel;

	public UILabel CancelButtonLabel;

	protected ItemIconComponent _itemIconComponent;

	private Action _onConfirm;

	public EquipItemInfo EquipItemInfo { get; private set; }

	public static EquipConfirmPopup Open(string message, string confirmLabel, string cancelLabel, EquipItemInfo equipItemInfo, Action onConfirm)
	{
		WindowPrefab<EquipConfirmPopup> windowPrefab = new WindowPrefab<EquipConfirmPopup>("Resources/Prefab2/Window/MessagePopup/EquipConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(message, confirmLabel, cancelLabel, equipItemInfo, onConfirm);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(string message, string confirmLabel, string cancelLabel, EquipItemInfo equipItemInfo, Action onConfirm)
	{
		base.Open();
		EquipItemInfo = equipItemInfo.Clone();
		EquipItemInfo.SoulWeaponInfo = equipItemInfo.SoulWeaponInfo;
		_onConfirm = onConfirm;
		UIHelper.SetLabelText(ConfirmButtonLabel, confirmLabel);
		UIHelper.SetLabelText(CancelButtonLabel, cancelLabel);
		UIHelper.SetLabelText(MessageLabel, message);
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot);
		_itemIconComponent.ResetPosition();
		if (EquipItemInfo != null)
		{
			_itemIconComponent.Open(EquipItemInfo, null);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(EquipItemInfo.ItemIndex);
			UIHelper.SetLabelText(NameLabel, ItemNameBuilder.GetItemName(data));
		}
		else
		{
			_itemIconComponent.OpenEmpty(null);
			UIHelper.SetLabelText(NameLabel, "");
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	public void OnClickConfirm()
	{
		Close();
		Vespa_System_Action_Extension_Method.fire(_onConfirm);
	}
}
