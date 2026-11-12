using System;
using NGame2.NContentsInfo;
using NGame2.NMatch;
using NGame2.NScene;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PvpResult : BaseWindow
{
	public interface Listener
	{
		void OnArena(PvpResult caller);

		void OnStatistics(PvpResult caller);

		void OnRetry(PvpResult caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PvpResult";

	public GameObject WinObject;

	public GameObject LoseObject;

	public GameObject ButtonRoot;

	public float EffectDuration = 3f;

	public PvpResultScoreInfo ScoreInfo;

	public UIButton LobbyButton;

	public UIButton RetryButton;

	public UIButton StatisticsButton;

	public UIWidget BottomRoot;

	public Transform EffectRoot;

	public UIGame3DWidget EffectWin;

	public UIGame3DWidget EffectLose;

	public Transform NormalMatchRoot;

	public Transform BanPickMatchRoot;

	public UILabel BanPickSuccessiveWin;

	public UILabel BanPickRecord;

	public int InputDisabledTimeMs;

	public Transform TierIconRoot;

	public Transform GiveUpRoot;

	private Listener _listener;

	private float _startTime;

	private bool _effectEnded;

	private float _openedTime;

	private BaseRewardPopup _rewardPopup;

	public MatchResultInfo MatchResultInfo { get; private set; }

	public bool Win => MatchResultInfo.Win;

	public void Open_Pvp(MatchResultInfo matchResultInfo, Listener listener)
	{
		if (matchResultInfo != null)
		{
			Open(delegate
			{
				Init_Pvp(matchResultInfo, listener);
			}, null);
		}
	}

	private void Init_Pvp(MatchResultInfo matchResultInfo, Listener listener)
	{
		SprkHonorDuel.PrepareResultEffect(this);
		_listener = listener;
		_startTime = Time.time;
		_effectEnded = false;
		MatchResultInfo = matchResultInfo;
		_openedTime = Time.time;
		bool win = matchResultInfo.Win;
		UIHelper.EnableGameObject(WinObject, win);
		UIHelper.EnableGameObject(LoseObject, !win);
		UIHelper.EnableGameObject(ButtonRoot, enable: false);
		bool enable = win && matchResultInfo.GiveUp;
		UIHelper.EnableGameObject(GiveUpRoot, enable);
		bool enable2 = false;
		if (SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.Normal && SingletonUpdater<MatchManager>.instance.DuelType == MatchDuelType.Arena)
		{
			enable2 = true;
		}
		UIHelper.EnableButton(RetryButton, enable2);
		UIHelper.EnableGameObject(BottomRoot, enable2);
		if (SingletonUpdater<MatchManager>.instance.DuelType == MatchDuelType.Friendly)
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(EffectRoot.transform);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(EffectWin.EntryList))
			{
				EffectWin.EntryList[0].ExtraTransform.Scale = 100f;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(EffectLose.EntryList))
			{
				EffectLose.EntryList[0].ExtraTransform.Scale = 100f;
			}
		}
		(win ? EffectWin : EffectLose).GetComponent<UIGame3DWidget>().PlayNow();
		PlaySound(win);
		UIHelper.EnableGameObject(NormalMatchRoot, enable: true);
		UIHelper.EnableGameObject(BanPickMatchRoot, enable: false);
		ScoreInfo.Open(matchResultInfo, SingletonUpdater<MatchManager>.instance.ArenaType);
		if (SingletonUpdater<MatchManager>.instance.DuelType == MatchDuelType.Arena && matchResultInfo.ArenaType == MatchArenaType.Normal)
		{
			_ = (object)null;
			_ = 28;
			_ = 1;
			_ = 0;
			Array.Empty<object>();
		}
		_rewardPopup = OpenRewardPopup(MatchResultInfo);
		UIHelper.EnableGameObject(_rewardPopup, enable: false);
	}

	public void Open_BanPickOnly(MatchResultInfo matchResultInfo, PlayerBattleBanPickInfo banPickInfo, Listener listener)
	{
		Open(delegate
		{
			Init_BanPickOnly(matchResultInfo, banPickInfo, listener);
		}, null);
	}

	private void Init_BanPickOnly(MatchResultInfo matchResultInfo, PlayerBattleBanPickInfo banPickInfo, Listener listener)
	{
		SprkHonorDuel.PrepareResultEffect(this);
		_listener = listener;
		_startTime = Time.time;
		_effectEnded = false;
		MatchResultInfo = matchResultInfo;
		bool win = matchResultInfo.Win;
		UIHelper.EnableGameObject(WinObject, win);
		UIHelper.EnableGameObject(LoseObject, !win);
		UIHelper.EnableGameObject(ButtonRoot, enable: false);
		UIHelper.EnableButton(RetryButton, enable: true);
		bool enable = win && matchResultInfo.GiveUp;
		UIHelper.EnableGameObject(GiveUpRoot, enable);
		bool enable2 = GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2;
		UIHelper.EnableButton(StatisticsButton, enable2);
		(win ? EffectWin : EffectLose).GetComponent<UIGame3DWidget>().PlayNow();
		PlaySound(win);
		UIHelper.EnableGameObject(NormalMatchRoot, enable: true);
		UIHelper.EnableGameObject(BanPickMatchRoot, enable: false);
		ScoreInfo.Open(matchResultInfo, SingletonUpdater<MatchManager>.instance.ArenaType);
		MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<MatchArena>.instance.GetMatchSeasonInfo(MatchPvpType.BanPick);
		UIHelper.EnableGameObject(TierIconRoot, matchSeasonInfo?.ActiveTier ?? false);
		_rewardPopup = OpenRewardPopup(matchResultInfo);
		UIHelper.EnableGameObject(_rewardPopup, enable: false);
		SprkHonorDuel.FriendlyResult(this);
	}

	private void PlaySound(bool win)
	{
		if (win)
		{
			SoundStock stock = new SoundStock
			{
				Code = "UI.Campaign.Victory",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 60f);
			SingletonAuto<BgmManager2>.instance.Request("UI.Campaign.VictoryBGM", isEvent: true);
		}
		else
		{
			SoundStock stock2 = new SoundStock
			{
				Code = "UI.Campaign.Lose",
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock2, 60f);
			SingletonAuto<BgmManager2>.instance.Request("UI.Campaign.LoseBGM", isEvent: true);
		}
	}

	public override void Reset()
	{
		base.Reset();
		ScoreInfo.Reset();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateEffectEnd();
	}

	private void UpdateEffectEnd()
	{
		if (!_effectEnded && Time.time - _startTime >= EffectDuration)
		{
			_effectEnded = true;
			UIHelper.EnableGameObject(_rewardPopup, enable: true);
			UIHelper.EnableGameObject(ButtonRoot, enable: true);
		}
	}

	private static BaseRewardPopup OpenRewardPopup(MatchResultInfo matchResultInfo)
	{
		if (matchResultInfo == null)
		{
			return null;
		}
		return BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(GuildRaid.GUILDRAID_DEFAULT_REWARD_TITLE)).AddCurrencyResultInfos3(matchResultInfo.CurrencyResults).AddItemResultInfos(matchResultInfo.ItemResults), isGlobal: false);
	}

	public void OnClickArena()
	{
		if (_effectEnded && _listener != null)
		{
			_listener.OnArena(this);
		}
	}

	public void OnClickStatistics()
	{
		if (_effectEnded && _listener != null)
		{
			_listener.OnStatistics(this);
		}
	}

	public void OnClickRetry()
	{
		if (_effectEnded && _listener != null)
		{
			_listener.OnRetry(this);
		}
	}
}
