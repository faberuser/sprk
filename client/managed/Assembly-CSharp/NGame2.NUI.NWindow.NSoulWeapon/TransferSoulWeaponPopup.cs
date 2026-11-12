using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class TransferSoulWeaponPopup : CommonSoulWeaponWindow, SoulWeaponList.IListener
{
	public interface IListener
	{
		void TransferSoulWeapon(int soulWeaponSlotIndex, int specialWeaponSlotIndex, RequirePrice requirePrice);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/TransferSoulWeaponPopup";

	public RequirePrice RequirePrice;

	public UIButton TransferButton;

	public Transform EmptyLabelTransform;

	private IListener _iListener;

	public void Open(IListener iListener, EquipItemInfo equipItemInfo)
	{
		Open(equipItemInfo);
		Init(iListener);
	}

	public override void Close()
	{
		base.Close();
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		SetSelectSoulWeapon(null);
		InitSoulWeaponList(this, base.EquipItemInfo, SoulWeaponList.ListType.EqualSpecialWeapon);
		SetTransferButtonState();
		SetTransferReqPriceLabel();
		EnableEmptyLabelState();
	}

	private void SetTransferButtonState()
	{
		UIHelper.EnableButton(TransferButton, enable: false);
		if (SoulWeaponList != null)
		{
			UIHelper.EnableButton(TransferButton, SoulWeaponList.SelectedSoulWeaponItemIconComponent != null);
		}
	}

	private void SetTransferReqPriceLabel()
	{
		RequirePrice.Open(-1L, 0L);
		if (base.SoulWeaponInfo != null)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SoulWeaponTransition");
			RequirePrice.Open(valueInt, 0L);
		}
	}

	private void EnableEmptyLabelState()
	{
		UIHelper.EnableGameObject(EmptyLabelTransform, !base.ExistList);
	}

	public void OnClickTransfer()
	{
		if (_iListener != null && base.EquipItemInfo != null && SoulWeaponList != null && !(SoulWeaponList.SelectedSoulWeaponItemIconComponent == null) && SoulWeaponList.SelectedSoulWeaponItemIconComponent.EquipItemInfo != null)
		{
			_iListener.TransferSoulWeapon(base.EquipItemInfo.SlotIndex, SoulWeaponList.SelectedSoulWeaponItemIconComponent.EquipItemInfo.SlotIndex, RequirePrice);
		}
	}

	void SoulWeaponList.IListener.SelectSoulWeapon(EquipItemInfo equipItemInfo, SoulWeaponList.ListType listType)
	{
		if (equipItemInfo != null)
		{
			SetTransferButtonState();
		}
	}
}
