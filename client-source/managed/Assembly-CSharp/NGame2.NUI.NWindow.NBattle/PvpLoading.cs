using System;
using System.Collections.Generic;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NMatch;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NLoading;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class PvpLoading : MonoBehaviour, IGlobalWindow
{
	[Serializable]
	public class PlayerInfo
	{
		public GameObject Root;

		public GuildArenaInfo GuildArenaInfo;

		public UITexture AvatarTexture;

		public UILabel WinPercentageLabel;

		public UILabel RankLabel;

		public UILabel RecordLabel;

		public UILabel SuccessiveWinLabel;

		public UILabel NameLabel;

		public GameObject OnlineObject;

		public GameObject OfflineObject;

		public Transform TierIconRoot;

		public UISimpleGrid PortraitGrid;

		public WindowAnimation PortraitAnimation;

		public UILabel DebugLabel;

		public UIWidget RankingRoot;

		public UIWidget RecordRoot;

		public UIWidget SuccessiveWinRoot;

		public UIWidget WinPercentageRoot;
	}

	[Serializable]
	public class OrdealArenaPlayerInfo : PlayerInfo
	{
	}

	[Serializable]
	public class GuildArenaInfo
	{
		public GameObject GuildArenaInfoRoot;

		public UILabel Label_ContributionPoint;

		public UILabel Label_BattleRecord;

		public UILabel Label_GuildName;

		public UILabel Label_Nick;

		public UITexture Texture_GuildLogo;

		public UITexture Texture_GuildBack;

		public UITexture Texture_CountryFlag;
	}

	public class InstanceInfo
	{
		public List<HeroPortraitComponent> PortraitComponentList = new List<HeroPortraitComponent>();

		public MyMatchTierIconComponent TierIconComponent;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PvpLoading";

	private static GlobalWindowPrefab<PvpLoading> _prefabInstance = new GlobalWindowPrefab<PvpLoading>("Resources/Prefab2/Window/Battle/PvpLoading", GlobalUIRoot.ELayerType.GlobalUI);

	public PlayerInfo LeftPlayer;

	public PlayerInfo RightPlayer;

	public GameObject WaitingObject;

	public UIWidget MatchedWidget;

	public float MatchedWidgetDelay;

	public UIGame3DWidget VsWidget;

	public UIGame3DWidget VsBanPickWidget;

	public float VsWidgetDuration;

	public GameObject LoadingRoot;

	public UISprite LoadingGaugeSprite;

	public UILabel LoadingGaugeLabel;

	public UIWidget NormalBg;

	public UIWidget BanPickBg;

	public UIWidget GuildArenaBg;

	public UIWidget OrdealArenaBg;

	public OrdealArenaPlayerInfo OrdealArenaLeft;

	public OrdealArenaPlayerInfo OrdealArenaRight;

	private InstanceInfo _leftInfo = new InstanceInfo();

	private InstanceInfo _rightInfo = new InstanceInfo();

	private float _matchedWidgetWaitedTime;

	private bool _sceneLoadingStarted;

	private bool _vsWidgetPlayed;

	private PlayerInfo _left;

	private PlayerInfo _right;

	public static PvpLoading instance => _prefabInstance.instance;

	public static bool isValidInstance => _prefabInstance.IsValidInstance;

	public bool IsMatched { get; private set; }

	public static void CreateInstance()
	{
		_prefabInstance.CreateInstance();
	}

	public static void ReleaseInstance()
	{
		_prefabInstance.ReleaseWindow();
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
		UIHelper.EnableGameObject(LeftPlayer.Root, enable: false);
		UIHelper.EnableGameObject(RightPlayer.Root, enable: false);
		UIHelper.EnableGameObject(LeftPlayer.GuildArenaInfo.GuildArenaInfoRoot, enable: false);
		UIHelper.EnableGameObject(RightPlayer.GuildArenaInfo.GuildArenaInfoRoot, enable: false);
		UIHelper.EnableGameObject(LoadingRoot, enable: false);
		UIHelper.EnableGameObject(OrdealArenaLeft.Root, enable: false);
		UIHelper.EnableGameObject(OrdealArenaRight.Root, enable: false);
		ShowWaiting(show: false);
	}

	public void Close()
	{
		Reset();
		base.gameObject.SetActive(value: false);
	}

	public void Reset()
	{
		FactoryBase.ReleaseAndClearList(_leftInfo.PortraitComponentList);
		FactoryBase.ReleaseAndClearList(_rightInfo.PortraitComponentList);
		FactoryBase.SafeReleaseComponent(ref _leftInfo.TierIconComponent);
		FactoryBase.SafeReleaseComponent(ref _rightInfo.TierIconComponent);
		_matchedWidgetWaitedTime = 0f;
		IsMatched = false;
		_sceneLoadingStarted = false;
		_vsWidgetPlayed = false;
		_left = null;
		_right = null;
	}

	void IGlobalWindow.ReserveCache(UiPreloader preloader)
	{
	}

	private void ShowWaiting(bool show)
	{
		UIHelper.EnableGameObject(WaitingObject, show);
	}

	public bool IsOpen()
	{
		return true;
	}

	public void InitLeftPlayer(MatchDuelType duelType, int winPercentage, int seasonWin, int seasonLose, int successiveWin, int tierIndex, VisualTierRank visualTierRank, string name, bool isOnline, int avatarHeroIndex, IEnumerable<CachedHeroInfo> heroInfos, GuildArenaRankerInfo guildInfo, GuildArenaMemberScoreInfo guidArenaScoreInfo)
	{
		_left = LeftPlayer;
		Init(duelType, TeamType.Left, LeftPlayer, winPercentage, seasonWin, seasonLose, successiveWin, tierIndex, visualTierRank, name, isOnline, avatarHeroIndex, heroInfos, _leftInfo, guildInfo, guidArenaScoreInfo);
		ShowWaiting(show: true);
	}

	public void InitRightPlayer(MatchDuelType duelType, int winPercentage, int seasonWin, int seasonLose, int successiveWin, int tierIndex, VisualTierRank visualTierRank, string name, bool isOnline, int avatarHeroIndex, IEnumerable<CachedHeroInfo> heroInfos, GuildArenaRankerInfo guildInfo, GuildArenaMemberScoreInfo guidArenaScoreInfo)
	{
		_right = RightPlayer;
		Init(duelType, TeamType.Right, RightPlayer, winPercentage, seasonWin, seasonLose, successiveWin, tierIndex, visualTierRank, name, isOnline, avatarHeroIndex, heroInfos, _rightInfo, guildInfo, guidArenaScoreInfo);
		ShowWaiting(show: false);
		IsMatched = true;
	}

	public void SetBG()
	{
		MatchArenaType arenaType = SingletonUpdater<MatchManager>.instance.ArenaType;
		UIHelper.EnableGameObject(NormalBg, arenaType == MatchArenaType.Normal);
		UIHelper.EnableGameObject(BanPickBg, arenaType == MatchArenaType.BanPick);
		UIHelper.EnableGameObject(GuildArenaBg, arenaType == MatchArenaType.GuildArena);
		UIHelper.EnableGameObject(OrdealArenaBg, arenaType == MatchArenaType.Ordeal);
	}

	public void SetOrdealArenaBg()
	{
		UIHelper.EnableGameObject(NormalBg, enable: false);
		UIHelper.EnableGameObject(BanPickBg, enable: false);
		UIHelper.EnableGameObject(GuildArenaBg, enable: false);
		UIHelper.EnableGameObject(OrdealArenaBg, enable: true);
	}

	public void InitOrdealLeft(MatchDuelType duelType, int winPercentage, int seasonWin, int seasonLose, int successiveWin, int tierIndex, VisualTierRank visualTierRank, string name, int avatarHeroIndex, IEnumerable<CachedHeroInfo> heroInfos)
	{
		_left = OrdealArenaLeft;
		Init(duelType, TeamType.Left, OrdealArenaLeft, winPercentage, seasonWin, seasonLose, successiveWin, tierIndex, visualTierRank, name, isOnline: false, avatarHeroIndex, heroInfos, _leftInfo, null, null);
	}

	public void InitOrdealRight(MatchDuelType duelType, int winPercentage, int seasonWin, int seasonLose, int successiveWin, int tierIndex, VisualTierRank visualTierRank, string name, int avatarHeroIndex, IEnumerable<CachedHeroInfo> heroInfos)
	{
		_right = OrdealArenaRight;
		Init(duelType, TeamType.Right, OrdealArenaRight, winPercentage, seasonWin, seasonLose, successiveWin, tierIndex, visualTierRank, name, isOnline: false, avatarHeroIndex, heroInfos, _rightInfo, null, null);
		IsMatched = true;
	}

	public void ClearRightPlayer()
	{
		if (_right == null)
		{
			_right = RightPlayer;
		}
		UIHelper.EnableGameObject(_right.Root, enable: false);
	}

	private void SetupTierComponent(PlayerInfo playerInfo, InstanceInfo instanceInfo, VisualTierRank visualTierRank, MatchTierData tierData, MatchArenaType arenaType, bool hideTierLevel = false)
	{
		if (tierData != null)
		{
			FactoryBase.SafeReleaseComponent(ref instanceInfo.TierIconComponent);
			instanceInfo.TierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(playerInfo.TierIconRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(instanceInfo.TierIconComponent.transform);
			instanceInfo.TierIconComponent.Open(tierData.Tier, tierData.Level, visualTierRank, arenaType);
			instanceInfo.TierIconComponent.HideName();
			if (hideTierLevel)
			{
				instanceInfo.TierIconComponent.DisableLevel();
			}
		}
	}

	private void Init(MatchDuelType duelType, TeamType teamType, PlayerInfo playerInfo, int winPercentage, int seasonWin, int seasonLose, int successiveWin, int tierIndex, VisualTierRank visualTierRank, string name, bool isOnline, int avatarHeroIndex, IEnumerable<CachedHeroInfo> heroInfos, InstanceInfo instanceInfo, GuildArenaRankerInfo guildInfo, GuildArenaMemberScoreInfo guildArenaScoreInfo)
	{
		if (playerInfo == null)
		{
			return;
		}
		if (duelType == MatchDuelType.Arena && SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.GuildArena)
		{
			InitPortraits(playerInfo, heroInfos, instanceInfo);
			InitGuildArenaLayout(playerInfo, name, teamType, guildInfo, guildArenaScoreInfo);
			return;
		}
		UIHelper.EnableGameObject(playerInfo.RankingRoot, duelType == MatchDuelType.Arena && SingletonUpdater<MatchManager>.instance.ArenaType != MatchArenaType.BanPick);
		UIHelper.EnableGameObject(playerInfo.RecordRoot, duelType == MatchDuelType.Friendly || SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.BanPick);
		UIHelper.EnableGameObject(playerInfo.SuccessiveWinRoot, duelType == MatchDuelType.Arena && SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.BanPick);
		UIHelper.EnableGameObject(playerInfo.WinPercentageRoot, duelType == MatchDuelType.Arena && SingletonUpdater<MatchManager>.instance.ArenaType != MatchArenaType.BanPick);
		if (duelType == MatchDuelType.Friendly || SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.BanPick)
		{
			string stringDirect = LocalizationManager.GetStringDirect("DUEL_BATTLE_WIN_LOSE", seasonWin, seasonLose);
			UIHelper.SetLabelText(playerInfo.RecordLabel, stringDirect);
		}
		else
		{
			UIHelper.SetLabelText(playerInfo.RankLabel, (visualTierRank.Value >= 0) ? visualTierRank.Value.ToString() : "-");
		}
		UIHelper.SetLabelText(playerInfo.SuccessiveWinLabel, successiveWin);
		UIHelper.EnableGameObject(playerInfo.Root, enable: true);
		UIHelper.SetLabelText(playerInfo.WinPercentageLabel, (winPercentage >= 0) ? winPercentage.ToString() : "-");
		UIHelper.SetLabelText(playerInfo.NameLabel, name);
		UIHelper.EnableGameObject(playerInfo.OnlineObject, isOnline);
		UIHelper.EnableGameObject(playerInfo.OfflineObject, !isOnline);
		MatchArenaType arenaType = SingletonUpdater<MatchManager>.instance.ArenaType;
		if (arenaType == MatchArenaType.BanPick)
		{
			NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(arenaType);
			if (matchSeasonInfo != null && matchSeasonInfo.ActiveTier)
			{
				MatchBanPickTierData data = GameRoot.Instance.GlobalBanPickTierDataContainer.GetData(tierIndex);
				SetupTierComponent(playerInfo, instanceInfo, visualTierRank, data, arenaType);
			}
			else
			{
				FactoryBase.SafeReleaseComponent(ref instanceInfo.TierIconComponent);
				instanceInfo.TierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(playerInfo.TierIconRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(instanceInfo.TierIconComponent.transform);
				int seasonIndex = 0;
				MatchSeasonType seasonType = MatchSeasonType.Free;
				if (matchSeasonInfo != null)
				{
					seasonType = matchSeasonInfo.MatchSeasonType;
					seasonIndex = matchSeasonInfo.SeasonIndex;
				}
				instanceInfo.TierIconComponent.Open(seasonType, seasonIndex);
			}
		}
		else
		{
			MatchTierData data2 = GameRoot.Instance.MatchTierDataContainer.GetData(tierIndex);
			SetupTierComponent(playerInfo, instanceInfo, visualTierRank, data2, arenaType, duelType == MatchDuelType.OrdealArena);
		}
		InitPortraits(playerInfo, heroInfos, instanceInfo);
	}

	private void InitPortraits(PlayerInfo playerInfo, IEnumerable<CachedHeroInfo> heroInfos, InstanceInfo instanceInfo)
	{
		FactoryBase.ReleaseAndClearList(instanceInfo.PortraitComponentList);
		foreach (CachedHeroInfo heroInfo in heroInfos)
		{
			if (heroInfo != null)
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(playerInfo.PortraitGrid, HeroPortraitComponent.ShortPrefabAuto);
				if (!(heroPortraitComponent == null))
				{
					heroPortraitComponent.Open(heroInfo, null);
					instanceInfo.PortraitComponentList.Add(heroPortraitComponent);
				}
			}
		}
		UIHelper.EnableGameObject(playerInfo.PortraitGrid, enable: false);
	}

	private void InitGuildArenaLayout(PlayerInfo playerInfo, string name, TeamType teamType, GuildArenaRankerInfo guildInfo, GuildArenaMemberScoreInfo guildArenaScoreInfo)
	{
		if (guildInfo == null)
		{
			return;
		}
		UIHelper.SetLabelText(playerInfo.GuildArenaInfo.Label_GuildName, guildInfo.Name);
		if (guildArenaScoreInfo != null)
		{
			UIHelper.EnableGameObject(playerInfo.Root, enable: false);
			UIHelper.EnableGameObject(playerInfo.GuildArenaInfo.GuildArenaInfoRoot, enable: true);
			string text = ((guildArenaScoreInfo.TotalBattleCount == 0 && guildArenaScoreInfo.DefenceWinCount == 0) ? "-" : LocalizationManager.GetStringDirect("GUILD_RANKCOUNT_TOTAL", guildArenaScoreInfo.TotalBattleCount, guildArenaScoreInfo.DefenceWinCount));
			UIHelper.SetLabelText(playerInfo.GuildArenaInfo.Label_BattleRecord, text);
			if (teamType == TeamType.Left)
			{
				text = ((guildArenaScoreInfo.TotalBattleCount == 0 && guildArenaScoreInfo.MatchWinCount == 0) ? "-" : LocalizationManager.GetStringDirect("GUILD_RANKCOUNT_TOTAL", guildArenaScoreInfo.TotalBattleCount, guildArenaScoreInfo.MatchWinCount));
				UIHelper.SetLabelText(playerInfo.GuildArenaInfo.Label_BattleRecord, text);
			}
			UIHelper.SetLabelText(playerInfo.GuildArenaInfo.Label_ContributionPoint, (guildArenaScoreInfo.SessionScore == 0) ? "-" : guildArenaScoreInfo.SessionScore.ToString());
			UIHelper.SetLabelText(playerInfo.GuildArenaInfo.Label_Nick, name);
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(guildInfo.Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, playerInfo.GuildArenaInfo.Texture_GuildLogo, guildLogoIcon);
			byte b = guildInfo.Back;
			if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b = 1;
			}
			string guildBackGround2DLongPath = UIHelper.GetGuildBackGround2DLongPath(b);
			UIHelper.ResetPlaneAssetTextureAsync(this, playerInfo.GuildArenaInfo.Texture_GuildBack, guildBackGround2DLongPath);
			string countryCode = guildInfo.CountryCode;
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, playerInfo.GuildArenaInfo.Texture_CountryFlag, plainTexture);
			}
		}
	}

	public void InitLeftDebugLabel(string text)
	{
		UIHelper.EnableGameObject(_left.DebugLabel, enable: true);
		UIHelper.SetLabelText(_left.DebugLabel, text);
	}

	public void InitRightDebugLabel(string text)
	{
		UIHelper.EnableGameObject(_right.DebugLabel, enable: true);
		UIHelper.SetLabelText(_right.DebugLabel, text);
	}

	public void PlayVsWidgetAndClose()
	{
		_vsWidgetPlayed = true;
		UIGame3DWidget vsWidget = ((SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.BanPick) ? VsBanPickWidget : VsWidget);
		if (vsWidget != null)
		{
			UIHelper.EnableGameObject(vsWidget, enable: true);
			vsWidget.Play(null);
			SingletonUpdater<Executer>.instance.DelayRun(VsWidgetDuration, this, delegate
			{
				vsWidget.Stop(immediatelyClear: true);
				ReleaseInstance();
			});
		}
		else
		{
			ReleaseInstance();
		}
	}

	private void Update()
	{
		UpdatePortaitVisiblity();
		UpdateMatchedWidget();
		UpdateLoadingGauge();
	}

	private void UpdatePortaitVisiblity()
	{
		if (_left != null)
		{
			UpdatePortaitVisiblity(_left, _leftInfo.PortraitComponentList);
		}
		if (_right != null)
		{
			UpdatePortaitVisiblity(_right, _rightInfo.PortraitComponentList);
		}
	}

	private static void UpdatePortaitVisiblity(PlayerInfo playerInfo, List<HeroPortraitComponent> componentList)
	{
		if (!UIHelper.IsEnabledGameObjet(playerInfo.PortraitGrid) && componentList.TrueForAll((HeroPortraitComponent e) => e.IsHeroImageLoaded()))
		{
			UIHelper.EnableGameObject(playerInfo.PortraitGrid, enable: true);
			UIHelper.RepositionGrid(playerInfo.PortraitGrid);
			if ((bool)playerInfo.PortraitAnimation)
			{
				playerInfo.PortraitAnimation.PlayAnimation(reverse: false);
			}
		}
	}

	private void UpdateMatchedWidget()
	{
		if (!UIHelper.IsEnabledGameObjet((SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.BanPick) ? VsBanPickWidget : VsWidget) && UIHelper.IsEnabledGameObjet(_left.PortraitGrid) && UIHelper.IsEnabledGameObjet(_right.PortraitGrid))
		{
			_matchedWidgetWaitedTime += Time.deltaTime;
			if (_matchedWidgetWaitedTime >= MatchedWidgetDelay)
			{
				UIHelper.EnableGameObject(MatchedWidget, enable: true);
			}
		}
	}

	private void UpdateLoadingGauge()
	{
		LoadingManager loadingManager = GlobalAutoBehaviour<LoadingManager>.instance;
		if (!IsMatched || _vsWidgetPlayed || (!_sceneLoadingStarted && !loadingManager.IsSceneLoading))
		{
			UIHelper.EnableGameObject(LoadingRoot, enable: false);
			return;
		}
		_sceneLoadingStarted = true;
		UIHelper.EnableGameObject(LoadingRoot, enable: true);
		float fillAmount = ((LoadingDefault.ILoadingStatus)loadingManager).FillAmount;
		UIHelper.SetSpriteFillAmount(LoadingGaugeSprite, fillAmount);
		UIHelper.SetLabelText(LoadingGaugeLabel, Mathf.FloorToInt(fillAmount * 100f) + "%");
	}
}
