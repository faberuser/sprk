using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAccount.NNotice;
using NGame2.NEventTrigger;
using NGame2.NUI.NWindow.NSoulWeapon;
using NGameAsset;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SoulWeaponContentsWindow : FrameControlWindow, IUserObserver<SoulWeaponNoticeInfo>, IUserObserver
{
	public interface IListener
	{
		void OnclickSoulStoneRestoreButton();

		void OnClickSoulWeaponUpgradeButton();

		void OnClickSoulWeaponManageButton();

		void OnclickSoulWeaponReinforceButton();

		void OnClickSoulWeaponLiberationButton();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponContentsWindow";

	public UIButton ButtonLiberation;

	public UIButton ButtonManage;

	public UIButton ButtonReinforce;

	public UIButton ButtonUpgrade;

	public GameObject GoLiberationNotice;

	public GameObject GoUpgradeNotice;

	public GameObject GoRestoreNotice;

	public GameObject GoManageNotice;

	public List<PlayAnimSoulWeaponContents> PlayAnimList = new List<PlayAnimSoulWeaponContents>();

	private IListener _listener;

	private DateTime _lastButtonClickTime;

	public void Open(IListener listener)
	{
		OpenSanctuaryOfGodKing triggerEvent = new OpenSanctuaryOfGodKing();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		_listener = listener;
		Open(delegate
		{
			Setup();
		}, null);
		_lastButtonClickTime = DateTime.Now;
	}

	public override void Close()
	{
		base.Close();
		_listener = null;
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		}
	}

	public void Setup()
	{
		SetupButtonState();
		SetPlayAnimState();
		SetupLiberationNotice();
		SetupRestoreNotice();
		SetupUpgradeNotice();
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SetupButtonState();
		SetPlayAnimState();
	}

	public override void Reset()
	{
		base.Reset();
		SetPlayAnimState();
	}

	private void SetPlayAnimState()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(PlayAnimList))
		{
			return;
		}
		PlayAnimSoulWeaponContents playAnimSoulWeaponContents = null;
		for (int i = 0; i < PlayAnimList.Count; i++)
		{
			playAnimSoulWeaponContents = PlayAnimList[i];
			if (!(playAnimSoulWeaponContents == null))
			{
				playAnimSoulWeaponContents.Open();
			}
		}
	}

	public void SetupButtonState()
	{
		bool enable = HaveSoulWeapon();
		UIHelper.EnableButton(ButtonLiberation, UIHelper.IsOpenSoulWeaponLiberationButton());
		UIHelper.EnableButton(ButtonManage, enable, grayscaleIfDisable: true);
		UIHelper.EnableButton(ButtonReinforce, enable, grayscaleIfDisable: true);
		UIHelper.EnableButton(ButtonUpgrade, enable, grayscaleIfDisable: true);
	}

	public void SetupLiberationNotice()
	{
		bool enable = false;
		if (UIHelper.IsOpenSoulWeaponLiberationButton() && AccountManager.IsLogined())
		{
			enable = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.GetLiberationNoticeState();
		}
		UIHelper.EnableGameObject(GoLiberationNotice, enable);
	}

	public void SetupUpgradeNotice()
	{
		bool enable = false;
		if (HaveSoulWeapon() && AccountManager.IsLogined())
		{
			enable = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.GetUpgradeNoticeState();
		}
		UIHelper.EnableGameObject(GoUpgradeNotice, enable);
		UIHelper.EnableGameObject(GoManageNotice, enable);
	}

	public void SetupRestoreNotice()
	{
		bool enable = false;
		if (AccountManager.IsLogined())
		{
			enable = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.GetRetoreNoticeState();
		}
		UIHelper.EnableGameObject(GoRestoreNotice, enable);
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
	}

	private bool HaveSoulWeapon()
	{
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.HaveSoulWeapon();
	}

	private bool CanClick()
	{
		if ((DateTime.Now - _lastButtonClickTime).TotalSeconds < 0.30000001192092896)
		{
			return false;
		}
		_lastButtonClickTime = DateTime.Now;
		return true;
	}

	public void OnclickSoulStoneRestoreButton()
	{
		PlaySound("UI.Common.Button04");
		if (CanClick() && _listener != null)
		{
			_listener.OnclickSoulStoneRestoreButton();
		}
	}

	public void OnClickSoulWeaponUpgradeButton()
	{
		PlaySound("UI.Common.Button05");
		if (CanClick() && _listener != null)
		{
			_listener.OnClickSoulWeaponUpgradeButton();
		}
	}

	public void OnClickSoulWeaponManageButton()
	{
		PlaySound("UI.Common.Button06");
		if (CanClick() && _listener != null)
		{
			_listener.OnClickSoulWeaponManageButton();
		}
	}

	public void OnclickSoulWeaponReinforceButton()
	{
		PlaySound("UI.Common.Button07");
		if (CanClick() && _listener != null)
		{
			_listener.OnclickSoulWeaponReinforceButton();
		}
	}

	public void OnClickSoulWeaponLiberationButton()
	{
		PlaySound("UI.Common.Button08");
		if (CanClick() && _listener != null)
		{
			_listener.OnClickSoulWeaponLiberationButton();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<SoulWeaponNoticeInfo>.OnChanged(SoulWeaponNoticeInfo changedInfo)
	{
		SetupUpgradeNotice();
		SetupLiberationNotice();
		SetupRestoreNotice();
	}
}
