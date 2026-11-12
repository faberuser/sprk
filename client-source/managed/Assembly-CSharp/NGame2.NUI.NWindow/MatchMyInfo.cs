using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchMyInfo : LobbyFullWindow, LobbyTopmostMenu.ICustomStaminaObserver, LobbyTopmostMenu.IUserStatObserver
{
	public interface Listener
	{
		void OnRewardInfo();

		void OnShop();

		void OnPartySelect();

		void OnHeroView();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchMyInfo";

	public UILabel MatchScoreLabel;

	public UILabel MatchScorePercentageLabel;

	public UILabel TierRankLabel;

	public UILabel WinLabel;

	public UILabel LoseLabel;

	public UILabel WinRatioLabel;

	public UILabel SuccessiveWinLabel;

	public Transform IconParent;

	public UISprite SeasonProgressSprite;

	public UILabel SeasonRemainLabel;

	public Transform Info;

	public Transform InfoNotJoined;

	public UILabel Label_NotJoined;

	public Transform MatchScoreObject;

	public Transform MatchRecordObject;

	public Transform TierIconRoot;

	private Listener _listener;

	private MyMatchTierIconComponent _tierIconComponent;

	public UIButton ButtonPartySelect;

	public UIButton ButtonBanPickPartySelect;

	public UIButton ButtonBanPickHeroView;

	public UILabel LabelTitle;

	private List<HeroPortraitTextureName> _HeroPortraitComponentList = new List<HeroPortraitTextureName>();

	public MatchPvpType PvpType { get; private set; }

	public StaminaType StaminaType
	{
		get
		{
			NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(PvpType);
			if (matchSeasonInfo == null)
			{
				return StaminaType.Empty;
			}
			if (matchSeasonInfo.ActiveTier)
			{
				if (PvpType == MatchPvpType.Normal)
				{
					return StaminaType.Sword;
				}
				return StaminaType.Sword2;
			}
			if (PvpType != MatchPvpType.Normal)
			{
				return StaminaType.Empty;
			}
			return StaminaType.Sword;
		}
	}

	public UserManager.UserStatType StatType
	{
		get
		{
			if (PvpType != MatchPvpType.Normal)
			{
				return UserManager.UserStatType.GloryPoint;
			}
			return UserManager.UserStatType.PvpCoin;
		}
	}

	public void InitMyInfo(Listener listener, MatchPvpType pvpType)
	{
		_listener = listener;
		PvpType = pvpType;
		UpdateBattleInfo();
		UIHelper.EnableGameObject(TierRankLabel, enable: false);
		UIHelper.EnableGameObject(SeasonRemainLabel, enable: false);
		UIHelper.EnableGameObject(SeasonProgressSprite, enable: false);
		UIHelper.EnableGameObject(ButtonPartySelect, PvpType != MatchPvpType.BanPick);
		UIHelper.EnableGameObject(ButtonBanPickHeroView, PvpType == MatchPvpType.BanPick);
		UIHelper.EnableGameObject(ButtonBanPickPartySelect, PvpType == MatchPvpType.BanPick);
		if (PvpType == MatchPvpType.BanPick)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(LocalizationManager.GetStringDirect("PVP2_TITLE_NAME"));
			stringBuilder.Append("(");
			NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(PvpType);
			if (matchSeasonInfo != null && matchSeasonInfo.MatchSeasonType == MatchSeasonType.Free)
			{
				stringBuilder.Append(LocalizationManager.GetStringDirect("MATCH_PICKBAN_FREE_SEASON"));
			}
			else
			{
				stringBuilder.Append(LocalizationManager.GetStringDirect("MATCH_PICKBAN_REAL_SEASON"));
			}
			stringBuilder.Append(")");
			UIHelper.SetLabelText(LabelTitle, stringBuilder);
		}
		else
		{
			UIHelper.SetLabelText(LabelTitle, LocalizationManager.GetStringDirect("PVP_MYINFO"));
		}
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ClearTierIcon();
		ClearRecommendPick();
	}

	public void UpdateBattleInfo()
	{
		ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(PvpType);
		if (arenaBattleInfo != null)
		{
			RefreshInfoObject(arenaBattleInfo);
			UILabel matchScoreLabel = MatchScoreLabel;
			int matchScore = arenaBattleInfo.MatchScore;
			UIHelper.SetLabelText(matchScoreLabel, Vespa_Int_Extension_Method.ToCommaString(matchScore));
			UIHelper.SetLabelText(WinLabel, arenaBattleInfo.SeasonWin);
			UIHelper.SetLabelText(LoseLabel, arenaBattleInfo.SeasonLose);
			UIHelper.SetLabelText(SuccessiveWinLabel, arenaBattleInfo.SuccessiveWin);
			int num = arenaBattleInfo.SeasonWin + arenaBattleInfo.SeasonLose;
			int num2 = ((num > 0) ? (arenaBattleInfo.SeasonWin * 100 / num) : 0);
			UIHelper.SetLabelText(WinRatioLabel, num2 + "%");
		}
	}

	private void UpdateTierRank(RankResultInfo rankResult, out VisualTierRank visualTierRank)
	{
		ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(PvpType);
		visualTierRank = VisualTierRank.CreateFromServer(rankResult.TotalRank);
		int tierIndex = NGame2.NContentsDefine.MatchArena.GetTierIndex(PvpType.ToMatchArenaType(), arenaBattleInfo);
		MatchTierData tierData = NGame2.NContentsDefine.MatchArena.GetTierData(PvpType.ToMatchArenaType(), tierIndex);
		if (tierData != null)
		{
			SetTierRank(visualTierRank, rankResult.TotalRankerCount, VisualTierRank.CreateFromTableMinRank(tierData, PvpType.ToMatchArenaType()), VisualTierRank.CreateFromTableMaxRank(tierData, PvpType.ToMatchArenaType()), tierData.MaxPercent);
		}
		InitTierIcon(visualTierRank);
	}

	private void SetTierRank(VisualTierRank visualTotalRank, int totalRankerCount, VisualTierRank visualMinRank, VisualTierRank visualMaxRank, float maxPercent)
	{
		if (visualMinRank.Value > 0 && visualMaxRank.Value > 0)
		{
			UIHelper.EnableGameObject(MatchScorePercentageLabel, enable: false);
		}
		else if (maxPercent > 0f)
		{
			if (totalRankerCount > 0)
			{
				int num = Mathf.CeilToInt((float)visualTotalRank.Value / (float)totalRankerCount * 100f);
				string text = LocalizationManager.GetString(Match.MATCH_TOP_PERCENTAGE, num);
				UIHelper.EnableGameObject(MatchScorePercentageLabel, enable: true);
				UIHelper.SetLabelText(MatchScorePercentageLabel, "(" + text + ")");
			}
			else
			{
				UIHelper.EnableGameObject(MatchScorePercentageLabel, enable: false);
			}
		}
		else
		{
			UIHelper.EnableGameObject(MatchScorePercentageLabel, enable: true);
			string text2 = LocalizationManager.GetString(Match.MATCH_OUT_OF_RANK);
			UIHelper.SetLabelText(MatchScorePercentageLabel, text2);
		}
	}

	private void UpdateNormalRank(RankResultInfo rankResult, NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo, out VisualTierRank visualTierRank)
	{
		visualTierRank = VisualTierRank.CreateFromServer(rankResult.TotalRank);
		if (rankResult.TotalRankerCount > 0)
		{
			int num = Mathf.CeilToInt((float)visualTierRank.Value / (float)rankResult.TotalRankerCount * 100f);
			string text = LocalizationManager.GetString(Match.MATCH_TOP_PERCENTAGE, num);
			UIHelper.EnableGameObject(MatchScorePercentageLabel, enable: true);
			UIHelper.SetLabelText(MatchScorePercentageLabel, "(" + text + ")");
		}
		else
		{
			UIHelper.EnableGameObject(MatchScorePercentageLabel, enable: false);
		}
		InitSeasonIcon(seasonInfo.SeasonIndex, seasonInfo.MatchSeasonType);
	}

	public void UpdateRank()
	{
		RankResultInfo rankResultInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetRankResultInfo(PvpType);
		if (rankResultInfo == null)
		{
			return;
		}
		VisualTierRank visualTierRank = VisualTierRank.CreateFromServer(rankResultInfo.TotalRank);
		if (PvpType == MatchPvpType.Normal)
		{
			UpdateTierRank(rankResultInfo, out visualTierRank);
		}
		else
		{
			NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(PvpType);
			if (matchSeasonInfo.ActiveTier)
			{
				UpdateTierRank(rankResultInfo, out visualTierRank);
			}
			else
			{
				UpdateNormalRank(rankResultInfo, matchSeasonInfo, out visualTierRank);
			}
		}
		object obj;
		if (visualTierRank.Value != 0)
		{
			int value = visualTierRank.Value;
			obj = Vespa_Int_Extension_Method.ToCommaString(value);
		}
		else
		{
			obj = "-";
		}
		string text = (string)obj;
		UIHelper.EnableGameObject(TierRankLabel, enable: true);
		UIHelper.SetLabelText(TierRankLabel, text);
	}

	public void UpdateSeasonInfo()
	{
		NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(PvpType);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		float fillAmount = 1f;
		if (matchSeasonInfo != null)
		{
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			double totalSeconds = (matchSeasonInfo.EndUtc - matchSeasonInfo.BeginUtc).TotalSeconds;
			double totalSeconds2 = (currentServerUTCTime - matchSeasonInfo.BeginUtc).TotalSeconds;
			fillAmount = Mathf.Clamp01((totalSeconds > 0.0) ? ((float)(totalSeconds2 / totalSeconds)) : 0f);
			TimeSpan timeSpan = matchSeasonInfo.EndUtc - currentServerUTCTime;
			num = timeSpan.Days;
			num2 = timeSpan.Hours;
			num3 = timeSpan.Minutes;
			num4 = timeSpan.Seconds;
		}
		string text = LocalizationManager.GetString((num == 0 && num2 == 0) ? Match.MATCH_SEASON_REMAIN_LABEL_MINUTE : Match.MATCH_SEASON_REMAIN_LABEL, num, num2, num3, num4);
		UIHelper.EnableGameObject(SeasonRemainLabel, enable: true);
		UIHelper.SetLabelText(SeasonRemainLabel, text);
		UIHelper.EnableGameObject(SeasonProgressSprite, enable: true);
		UIHelper.SetSpriteFillAmount(SeasonProgressSprite, fillAmount);
	}

	private void RefreshInfoObject(ArenaBattleInfo battleInfo)
	{
		UIHelper.EnableGameObject(MatchScoreObject, enable: true);
		UIHelper.EnableGameObject(TierIconRoot, enable: true);
		UIHelper.EnableGameObject(MatchRecordObject, enable: true);
		UIHelper.EnableGameObject(MatchScorePercentageLabel, enable: true);
		bool flag = false;
		if (battleInfo != null)
		{
			flag = battleInfo.SeasonWin + battleInfo.SeasonLose > 0;
		}
		UIHelper.EnableGameObject(Info, flag);
		UIHelper.EnableGameObject(InfoNotJoined, !flag);
		if (!flag)
		{
			string key = "MATCH_SEASON_NOT_JOINED2";
			UIHelper.SetLabelText(Label_NotJoined, LocalizationManager.GetStringDirect(key));
		}
	}

	private void InitTierIcon(VisualTierRank visualTierRank)
	{
		ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(PvpType);
		if (arenaBattleInfo == null)
		{
			return;
		}
		int tierIndex = NGame2.NContentsDefine.MatchArena.GetTierIndex(PvpType.ToMatchArenaType(), arenaBattleInfo);
		MatchTierData tierData = NGame2.NContentsDefine.MatchArena.GetTierData(PvpType.ToMatchArenaType(), tierIndex);
		if (tierData == null)
		{
			VespaDebug.LogError("Failed to get tier data of index : " + tierIndex);
			return;
		}
		if (_tierIconComponent == null)
		{
			_tierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(IconParent);
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_tierIconComponent.transform);
		_tierIconComponent.Open(tierData.Tier, tierData.Level, visualTierRank, PvpType.ToMatchArenaType());
	}

	private void InitSeasonIcon(int seasonIndex, MatchSeasonType matchSeasonType = MatchSeasonType.Regular)
	{
		if (_tierIconComponent == null)
		{
			_tierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(IconParent);
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_tierIconComponent.transform);
		_tierIconComponent.Open(matchSeasonType, seasonIndex);
	}

	private void ClearTierIcon()
	{
		FactoryBase.SafeReleaseComponent(ref _tierIconComponent);
	}

	private void ClearRecommendPick()
	{
		FactoryBase.ReleaseAndClearList(_HeroPortraitComponentList);
	}

	public void OnClickRewardInfo()
	{
		if (_listener != null)
		{
			_listener.OnRewardInfo();
		}
	}

	public void OnClickShop()
	{
		if (_listener != null)
		{
			_listener.OnShop();
		}
	}

	public void OnClickPartySelect()
	{
		if (_listener != null)
		{
			_listener.OnPartySelect();
		}
	}

	public void OnClickHeroView()
	{
		if (_listener != null)
		{
			_listener.OnHeroView();
		}
	}
}
