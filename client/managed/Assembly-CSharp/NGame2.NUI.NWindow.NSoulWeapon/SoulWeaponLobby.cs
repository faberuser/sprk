using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAccount.NNotice;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulWeaponLobby : LobbyFullWindow, IUserObserver<SoulWeaponNoticeInfo>, IUserObserver
{
	[Serializable]
	public class GodKingTrial
	{
		public UIWidget KeyIconProxyRootWidget;

		public CurrencyType KeyCurrencyType;

		public StaminaType StaminaType;

		public UILabel KeyCountLabel;
	}

	public interface IListener
	{
		void MoveGodkingTemple();

		void EnterSoulWeaponContents();

		void EnterManageSoulWeapon();

		void EnterDungeon();

		void EnterGodkingRewardPopup();

		void EnterEclipse();

		void OpenEclipseDungeonRewardPopup();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponLobby";

	public GameObject EclipseKeyGaugeRoot;

	private EclipseKeyGaugeComponent _eclipseKeyGauge;

	public GameObject EclipseRedMoonRoot;

	public GameObject EclipseBlueMoonRoot;

	public UIButton SoulWeaponManageButton;

	public GameObject GoManageButtonNotice;

	public GameObject GoMoveButtonNotice;

	public UIPanel GrayPanel;

	public int CheckChapterIndex;

	public int CheckDungeonIndex;

	public List<Transform> BackgroundEffectTransformList = new List<Transform>();

	public GodKingTrial GodKingTrialInfo;

	private IListener _iListener;

	private IconProxy _godKingTrialKeyIconProxy;

	private string _beforeBgmCode;

	public void Open(IListener iListener)
	{
		Open();
		Init(iListener);
	}

	private void Init(IListener iListener)
	{
		SetGodKingTrialKeyInfo();
		SetGray(isGray: false);
		EableBackgroundEffect(enable: true);
		if (!SoulWeaponManagement.IsOpenGodKingTemple())
		{
			MoveGodkingTemple(iListener);
			return;
		}
		_iListener = iListener;
		Clear();
		InitEclipseKeyGauge();
		SetSoulWeaponButtonState();
		PlayBGM();
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}
	}

	private void MoveGodkingTemple(IListener iListener)
	{
		SetGray(isGray: true);
		EableBackgroundEffect(enable: false);
		iListener?.MoveGodkingTemple();
	}

	private void SetGray(bool isGray)
	{
		WindowRoot.GetComponent<UIWidget>();
		if (GrayPanel != null)
		{
			GrayPanel.isGrayscale = isGray;
		}
	}

	private void EableBackgroundEffect(bool enable)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(BackgroundEffectTransformList))
		{
			return;
		}
		Transform transform = null;
		for (int i = 0; i < BackgroundEffectTransformList.Count; i++)
		{
			transform = BackgroundEffectTransformList[i];
			if (transform != null)
			{
				UIHelper.EnableGameObject(transform, enable);
			}
		}
	}

	public override void Close()
	{
		base.Close();
		if (AccountManager.IsLogined())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		}
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
		FactoryBase.SafeReleaseComponent(ref _godKingTrialKeyIconProxy);
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SetGodKingTrialKeyInfo();
	}

	private void Clear()
	{
		FactoryBase.SafeReleaseComponent(ref _eclipseKeyGauge);
	}

	public void InitEclipseKeyGauge()
	{
		if (_eclipseKeyGauge == null)
		{
			_eclipseKeyGauge = EclipseKeyGaugeComponent.PrefabAuto2.Factory.Create(EclipseKeyGaugeRoot.transform);
		}
		long num = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.EclipsePoint);
		CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3();
		currencyResultInfo.CurrencyType = CurrencyType.EclipsePoint;
		currencyResultInfo.AddValue = 0L;
		currencyResultInfo.NewValue = userStat;
		_eclipseKeyGauge.Init(num, currencyResultInfo, playFillAnimation: false);
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo != null && eclipseInfo.IsPlayEclipse)
		{
			int num2 = 1;
			num = num2;
		}
		GameObject eclipseBlueMoonRoot = EclipseBlueMoonRoot;
		long num3 = num;
		int num4 = 0;
		UIHelper.EnableGameObject(eclipseBlueMoonRoot, num3 <= num4);
		GameObject eclipseRedMoonRoot = EclipseRedMoonRoot;
		long num5 = num;
		int num6 = 0;
		UIHelper.EnableGameObject(eclipseRedMoonRoot, num5 > num6);
	}

	public void SetSoulWeaponButtonState()
	{
		UIHelper.EnableButton(SoulWeaponManageButton, enable: false);
		if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager != null)
		{
			bool enable = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.HaveSoulWeapon();
			UIHelper.EnableButton(SoulWeaponManageButton, enable);
			SetSoulWeaponNotice();
		}
	}

	private void SetSoulWeaponNotice()
	{
		bool enable = false;
		bool enable2 = false;
		if (AccountManager.IsLogined())
		{
			enable = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.GetUpgradeNoticeState();
			enable2 = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponNoticeManager.AnyNoticeStateOn();
		}
		UIHelper.EnableGameObject(GoManageButtonNotice, enable);
		UIHelper.EnableGameObject(GoMoveButtonNotice, enable2);
	}

	private void PlayBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_beforeBgmCode = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		if (_beforeBgmCode != "EntryMenu.KainAscensionBGM")
		{
			SingletonAuto<BgmManager2>.instance.Request("EntryMenu.KainAscensionBGM", isEvent: true);
		}
	}

	private void PlayPrevBgm()
	{
		SingletonAuto<BgmManager2>.instance.StopEvent("EntryMenu.KainAscensionBGM");
		if (_beforeBgmCode != "EntryMenu.KainAscensionBGM")
		{
			SingletonAuto<BgmManager2>.instance.Request(_beforeBgmCode, isEvent: true);
		}
	}

	private void SetGodKingTrialKeyInfo()
	{
		if (_godKingTrialKeyIconProxy == null)
		{
			_godKingTrialKeyIconProxy = IconProxy.Create(GodKingTrialInfo.KeyIconProxyRootWidget);
		}
		if (_godKingTrialKeyIconProxy != null)
		{
			_godKingTrialKeyIconProxy.SetupCurrencyNoBg(GodKingTrialInfo.KeyCurrencyType);
		}
		UIHelper.SetLabelText(GodKingTrialInfo.KeyCountLabel, "-");
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(GodKingTrialInfo.StaminaType);
		if (data != null)
		{
			long godkingTrialKey = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.GodkingTrialKey;
			UIHelper.SetLabelText(GodKingTrialInfo.KeyCountLabel, godkingTrialKey + "/" + data.MaxCountValue);
		}
	}

	public override void OnClosing()
	{
		PlayPrevBgm();
		base.OnClosing();
	}

	public void OnClickMoveSanctum()
	{
		if (_iListener != null)
		{
			_iListener.EnterSoulWeaponContents();
		}
	}

	public void OnClickManageSoulWeapon()
	{
		if (_iListener != null)
		{
			_iListener.EnterManageSoulWeapon();
		}
	}

	public void OnClickGodkingRewardPopup()
	{
		if (_iListener != null)
		{
			_iListener.EnterGodkingRewardPopup();
		}
	}

	public void OnClickEnterDungeon()
	{
		if (_iListener != null)
		{
			_iListener.EnterDungeon();
		}
	}

	public void OnClickEnterEclipse()
	{
		if (_iListener != null)
		{
			_iListener.EnterEclipse();
		}
	}

	public void OnClickEclipseDungeonInfo()
	{
		if (_iListener != null)
		{
			_iListener.OpenEclipseDungeonRewardPopup();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<SoulWeaponNoticeInfo>.OnChanged(SoulWeaponNoticeInfo changedInfo)
	{
		SetSoulWeaponNotice();
	}
}
