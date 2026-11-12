using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NSoulWeapon;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EclipseMain : FrameControlWindow, IUserObserver<UserManager.UserStatInfo>, IUserObserver, ISweepDungeonWindow
{
	public interface Listener
	{
		void OnClickRewardInfo();

		void OnClickBattleReady();

		void OnClickDeckSetting();
	}

	public interface ReturnStateListener
	{
		void SetHideRoot(bool enable);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/EclipseMain";

	public static string EffectCode_RedMoon = "UI.EclipsDungeon.BG.Red";

	public UILabel Label_Start;

	public UILabel Label_Reward;

	public UILabel Label_BestWave;

	public UILabel Label_StartWave;

	public UILabel Label_RemainTime;

	public GameObject WaveInfoRoot;

	public UIButton Button_Sweep;

	public UIButton Button_Ready;

	public UIButton Button_DeckSetting;

	public UIButton Button_Finish;

	public GameObject HideRoot;

	public UIScrollGrid Grid_Reward;

	public UIGame3DWidget BGEffect;

	private string EffectCode_BlueMoon = "UI.EclipsDungeon.BG.Blue";

	private string EffectCode_RedStart = "UI.EclipsDungeon.BG.RedStart";

	public const string UI_Eclipse_BGM = "EntryMenu.EclipseEntryMenuBGM";

	public static bool IsOpenByEntryMenu = false;

	private string _beforeBGM = string.Empty;

	private List<UIScrollItemBehaviour> _itemIconComponentList = new List<UIScrollItemBehaviour>();

	private bool _playEffectSound;

	public Transform EclipseKeyGaugeRoot;

	private EclipseKeyGaugeComponent _eclipseKeyGauge;

	public EclipseInfo _eclipseInfo;

	private Listener _listener;

	private bool _isExpire;

	private float _lastRefreshInfoTime;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.Stamina && changedInfo.StaminaType == StaminaType.EclipseKey)
		{
			SetSweepButton();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (RefreshTime())
		{
			UpdateRemainTime();
		}
	}

	private bool RefreshTime()
	{
		if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return false;
		}
		if (_eclipseInfo == null)
		{
			return false;
		}
		if (Time.time - _lastRefreshInfoTime < 1f)
		{
			return false;
		}
		_lastRefreshInfoTime = Time.time;
		return true;
	}

	private void UpdateRemainTime()
	{
		int remainTime = GetRemainTime();
		if (_eclipseInfo != null && _eclipseInfo.IsPlayEclipse && remainTime == 0 && !_isExpire)
		{
			_isExpire = true;
			InitButtons();
		}
		InitRemainTimeLabel(remainTime);
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		FactoryBase.SafeReleaseComponent(ref _eclipseKeyGauge);
		_isExpire = false;
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		SingletonAuto<BgmManager2>.instance.StopEvent("EntryMenu.EclipseEntryMenuBGM");
		if (_beforeBGM != "EntryMenu.EclipseEntryMenuBGM")
		{
			SingletonAuto<BgmManager2>.instance.Request(_beforeBGM, isEvent: true);
		}
	}

	public void Open(Listener listener)
	{
		_playEffectSound = false;
		ResetInfo();
		if (_eclipseInfo != null)
		{
			IsOpenByEntryMenu = !SceneAutoBehaviour<SoulWeaponManagement>.instance.IsOpened<SoulWeaponLobby>();
			_listener = listener;
			WaitingPopup.Open(this);
			UIHelper.EnableGameObject(BGEffect.gameObject, enable: false);
			BGEffect.Stop(immediatelyClear: true);
			PlayBGEffect();
			VespaCoroutine.Start(coOpen());
		}
	}

	public void ResetInfo()
	{
		_eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
	}

	private IEnumerator coOpen()
	{
		while (!BGEffect.ISDONE())
		{
			yield return null;
		}
		WaitingPopup.Close(this);
		Open();
		Init();
	}

	public void Init()
	{
		OpenEclipseDungeonUI triggerEvent = new OpenEclipseDungeonUI();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		UIHelper.EnableGameObject(BGEffect.gameObject, enable: true);
		SetHideRoot(enable: true);
		UpdateRemainTime();
		InitEclipseKeyGauge();
		InitLabels();
		InitButtons();
		InitReward();
		PlayBGM();
		PlayBGEffectSound();
	}

	private void PlayBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_beforeBGM = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		if (_beforeBGM != "EntryMenu.EclipseEntryMenuBGM")
		{
			SingletonAuto<BgmManager2>.instance.Request("EntryMenu.EclipseEntryMenuBGM", isEvent: true);
		}
	}

	private void PlayBGEffectSound()
	{
		if (_playEffectSound)
		{
			SoundStock stock = new SoundStock
			{
				Code = "UI.Eclipse.Start"
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
		}
	}

	public void PlayBGEffect()
	{
		TransformData transformData = new TransformData();
		transformData.Position = new Vector3(0f, 0f, 0f);
		transformData.RotationEuler = new Vector3(0f, 0f, 0f);
		transformData.Scale = 112f;
		string effectCode = EffectCode_BlueMoon;
		long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
		int num = 0;
		if (stamina > num)
		{
			LocalAccountBool localAccountBool = new LocalAccountBool(i_default: false, $"Eclipse_MatchIndex_{_eclipseInfo.MatchIndex})");
			if (localAccountBool.Value)
			{
				effectCode = EffectCode_RedMoon;
			}
			else
			{
				effectCode = EffectCode_RedStart;
				localAccountBool.Value = true;
				_playEffectSound = true;
			}
		}
		if (_eclipseInfo.IsPlayEclipse)
		{
			effectCode = EffectCode_RedMoon;
		}
		BGEffect.PlayInstantEffect(effectCode, transformData, null, playSync: true);
	}

	public void InitEclipseKeyGauge()
	{
		if (_eclipseKeyGauge == null)
		{
			_eclipseKeyGauge = EclipseKeyGaugeComponent.PrefabAuto.Factory.Create(EclipseKeyGaugeRoot);
		}
		long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
		long newValue = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(UserManager.UserStatType.EclipsePoint);
		CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3();
		currencyResultInfo.CurrencyType = CurrencyType.EclipsePoint;
		currencyResultInfo.AddValue = 0L;
		currencyResultInfo.NewValue = newValue;
		_eclipseKeyGauge.Init(stamina, currencyResultInfo, playFillAnimation: false);
	}

	public void InitLabels()
	{
		if (_eclipseInfo != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("CONTENTS_ECLIPSE_WAVE_INFO_START_WAVE");
			string stringDirect2 = LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_ENABLE_GET_REWARD_BASE");
			string text = LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_WAVE_TEXT", _eclipseInfo.MaxWaveIndex);
			if (_eclipseInfo.IsPlayEclipse)
			{
				stringDirect2 = LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_ENABLE_GET_REWARD_CONTINUE");
			}
			if (_eclipseInfo.MaxWaveIndex <= 0)
			{
				text = "-";
			}
			UIHelper.SetLabelText(Label_Start, stringDirect);
			UIHelper.SetLabelText(Label_Reward, stringDirect2);
			UIHelper.SetLabelText(Label_BestWave, text);
			int num = 1;
			EclipseStartData data = GameRoot.Instance.EclipseStartDataContainer.GetData(_eclipseInfo.MaxWaveIndex);
			if (data != null)
			{
				num = data.StartWave;
			}
			if (num <= 0)
			{
				num = 1;
			}
			UIHelper.SetLabelText(Label_StartWave, LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_WAVE_TEXT", num));
		}
	}

	private int GetRemainTime()
	{
		return GetRemainTime(_eclipseInfo);
	}

	public static int GetRemainTime(EclipseInfo eclipseInfo)
	{
		if (eclipseInfo == null)
		{
			return 0;
		}
		if (!eclipseInfo.IsPlayEclipse)
		{
			return 0;
		}
		return GetRemainTime(eclipseInfo.ExpireTime);
	}

	public static int GetRemainTime(string expireTime)
	{
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(expireTime, out var o_dateTime))
		{
			return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
		}
		return 0;
	}

	private void InitRemainTimeLabel(int remainTime)
	{
		string text = string.Empty;
		if (_eclipseInfo == null)
		{
			return;
		}
		if (_eclipseInfo.IsPlayEclipse)
		{
			if (_isExpire)
			{
				text = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_TIME_OVER_TIME_MSG");
			}
			else
			{
				string stringDirect = LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_BATTLE_LIMIT_TIME_INFO");
				text = $"{stringDirect}\n{UIHelper.GetRemainTimeLabelText(remainTime)}";
			}
		}
		UIHelper.SetLabelText(Label_RemainTime, text);
	}

	public void InitButtons()
	{
		if (_eclipseInfo != null && _eclipseInfo.IsPlayEclipse)
		{
			UIHelper.EnableGameObject(Button_DeckSetting, enable: false);
			UIHelper.EnableGameObject(Button_Ready, enable: true);
			UIHelper.EnableGameObject(Button_Finish, enable: false);
			UIHelper.EnableGameObject(Button_Sweep, enable: false);
			if (_eclipseInfo.DeckIndex == _eclipseInfo.LastDeckIndex)
			{
				_listener.OnClickBattleReady();
			}
			else if (_isExpire)
			{
				UIHelper.EnableGameObject(Button_Ready, enable: false);
				UIHelper.EnableGameObject(Button_Finish, enable: true);
			}
		}
		else
		{
			UIHelper.EnableGameObject(Button_DeckSetting, enable: true);
			UIHelper.EnableGameObject(Button_Ready, enable: false);
			UIHelper.EnableGameObject(Button_Finish, enable: false);
			UIHelper.EnableGameObject(Button_Sweep, enable: true);
			SetSweepButton();
		}
	}

	private void SetSweepButton()
	{
		int chapterIndex = _eclipseInfo.ChapterIndex;
		int dungeonIndex = _eclipseInfo.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		bool flag = SweepHelper.IsPossibleSweepStep(data);
		if (!flag)
		{
			UIHelper.SetWidgetGrayScale(Button_Sweep, !flag);
			return;
		}
		bool enable = SweepHelper.IsRemainStamina(data);
		UIHelper.EnableButton(Button_Sweep, enable, grayscaleIfDisable: true);
	}

	private void InitReward()
	{
	}

	public void RefreshEclipseMain()
	{
		BGEffect.Stop(immediatelyClear: true);
		PlayBGEffect();
		ResetInfo();
		SetHideRoot(enable: true);
		UpdateRemainTime();
		InitEclipseKeyGauge();
		InitLabels();
		InitButtons();
		InitReward();
	}

	public void RefreshBySweep()
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.RefreshEclipseMain();
	}

	public void OnCloseAboveWindow()
	{
		SetHideRoot(enable: true);
	}

	public void SetHideRoot(bool enable)
	{
		UIHelper.EnableGameObject(HideRoot, enable);
	}

	public void OnClickSweep()
	{
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo == null)
		{
			return;
		}
		int chapterIndex = eclipseInfo.ChapterIndex;
		int dungeonIndex = eclipseInfo.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			if (!SweepHelper.IsPossibleSweepStep(data))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("SWEEP_NOTICE_ACTIVATION_ECLIPSE_CLEAR"));
			}
			else
			{
				SweepHelper.OpenSweepPopup(data, this);
			}
		}
	}

	public void OnClickRewardInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickRewardInfo();
		}
	}

	public void OnClickBattleReady()
	{
		if (_listener != null)
		{
			_listener.OnClickBattleReady();
		}
	}

	public void OnClickDeckSetting()
	{
		if (_listener != null)
		{
			_listener.OnClickDeckSetting();
		}
	}
}
