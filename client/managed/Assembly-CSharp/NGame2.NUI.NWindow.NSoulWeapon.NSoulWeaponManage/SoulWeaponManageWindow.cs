using NGame2.NAccount;
using NGame2.NAccount.NNotice;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon.NSoulWeaponManage;

public class SoulWeaponManageWindow : CommonSoulWeaponWindow, SoulWeaponList.IListener, SelectSoulWeaponInfo.IListener, IUserObserver<SoulWeaponNoticeInfo>, IUserObserver
{
	public interface IListener
	{
		void OpenSoulWeaponReinforceWindow(EquipItemInfo selectedSoulWeaponEquipItemInfo);

		void OpenSoulWeaponUpgradeWindow(EquipItemInfo selectedSoulWeaponEquipItemInfo);

		void OpenTransferSoulWeaponPopup(EquipItemInfo selectedEquipItemInfo);

		void OpenRenewSoulWeaponStatPopup(EquipItemInfo selectedEquipItemInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponManageWindow";

	public SelectSoulWeaponInfo SelectSoulWeaponInfo;

	public GameObject GoUpgradeNotice;

	private IListener _iListener;

	public void Open(IListener iListener, EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		Open(equipItemInfo, delegate
		{
			SelectSoulWeaponInfo.SetSelectSoulWeaponInfo(base.EquipItemInfo);
		});
		Init(iListener, isSelectFirst);
	}

	private void Init(IListener iListener, bool isSelectFirst)
	{
		_iListener = iListener;
		InitSoulWeaponList(this, base.EquipItemInfo, SoulWeaponList.ListType.SoulWeapon, isSelectFirst);
		InitSelectSoulWeaponInfo();
		if (base.EquipItemInfo != null)
		{
			SetUpgradeNotice(base.EquipItemInfo.SlotIndex);
		}
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}
	}

	private void InitSelectSoulWeaponInfo()
	{
		SelectSoulWeaponInfo.Open(this);
	}

	private void SetUpgradeNotice(int slotIndex)
	{
		bool enable = false;
		if (AccountManager.IsLogined())
		{
			enable = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.GetSoulWeaponUpgradeNoticeState(slotIndex);
		}
		UIHelper.EnableGameObject(GoUpgradeNotice, enable);
	}

	public override void Close()
	{
		base.Close();
		_iListener = null;
		ReleaseSelectSoulWeaponIcon();
		SelectSoulWeaponInfo.Close();
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		}
	}

	public void Refresh(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			base.EquipItemInfo = equipItemInfo;
			RefreshList(equipItemInfo);
			SelectSoulWeaponInfo.SetSelectSoulWeaponInfo(equipItemInfo);
			if (base.EquipItemInfo != null)
			{
				SetUpgradeNotice(base.EquipItemInfo.SlotIndex);
			}
		}
	}

	private void OpenSoulWeaponReinforceWindow()
	{
		if (_iListener != null)
		{
			_iListener.OpenSoulWeaponReinforceWindow(base.EquipItemInfo);
		}
	}

	private void OpenSoulWeaponUpgradeWindow()
	{
		if (_iListener != null)
		{
			_iListener.OpenSoulWeaponUpgradeWindow(base.EquipItemInfo);
		}
	}

	private void OpenTransferSoulWeaponPopup()
	{
		if (base.EquipItemInfo != null && _iListener != null)
		{
			_iListener.OpenTransferSoulWeaponPopup(base.EquipItemInfo);
		}
	}

	private void OpenRenewSoulWeaponStatPopup()
	{
		if (base.EquipItemInfo != null && _iListener != null)
		{
			_iListener.OpenRenewSoulWeaponStatPopup(base.EquipItemInfo);
		}
	}

	void SoulWeaponList.IListener.SelectSoulWeapon(EquipItemInfo equipItemInfo, SoulWeaponList.ListType listType)
	{
		if (equipItemInfo != null)
		{
			base.EquipItemInfo = equipItemInfo;
			SelectSoulWeaponInfo.SetSelectSoulWeaponInfo(equipItemInfo);
			if (base.EquipItemInfo != null)
			{
				SetUpgradeNotice(base.EquipItemInfo.SlotIndex);
			}
			if (listType == AllSoulWeaponList.AllListType)
			{
				SoulWeaponList.SelectSoulWeaponItemIconComponent(equipItemInfo);
				EnableAllSoulWeaponList(enable: false);
			}
		}
	}

	void SelectSoulWeaponInfo.IListener.ChangeSoulWeaponInfo(SelectSoulWeaponInfo.SoulWeaponInfoType infoType)
	{
		if (infoType != SelectSoulWeaponInfo.SoulWeaponInfoType.None)
		{
			base.EquipItemInfo = ((SoulWeaponList.SelectedSoulWeaponItemIconComponent != null) ? SoulWeaponList.SelectedSoulWeaponItemIconComponent.EquipItemInfo : null);
			switch (infoType)
			{
			case SelectSoulWeaponInfo.SoulWeaponInfoType.Reinforce:
				OpenSoulWeaponReinforceWindow();
				break;
			case SelectSoulWeaponInfo.SoulWeaponInfoType.Upgrade:
				OpenSoulWeaponUpgradeWindow();
				break;
			case SelectSoulWeaponInfo.SoulWeaponInfoType.Transfer:
				OpenTransferSoulWeaponPopup();
				break;
			case SelectSoulWeaponInfo.SoulWeaponInfoType.Redistribution:
				OpenRenewSoulWeaponStatPopup();
				break;
			}
			if (base.EquipItemInfo != null)
			{
				SetUpgradeNotice(base.EquipItemInfo.SlotIndex);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<SoulWeaponNoticeInfo>.OnChanged(SoulWeaponNoticeInfo changedInfo)
	{
		if (changedInfo.NoticeType == ESoulWeaponNoticeType.Upgrade && base.EquipItemInfo != null && base.EquipItemInfo.SlotIndex == changedInfo.SlotIndex)
		{
			SetUpgradeNotice(changedInfo.SlotIndex);
		}
	}
}
