using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class WorldBossGlobalRanking : LobbyFullWindow, WorldBossGlobalRankerComponent.Listener, RecordsOfHonorShortcut.ServerTypeProvider, LobbyTopmostMenu.ICustomStaminaObserver
{
	[Serializable]
	public class MyRankingInfoUiData
	{
		public UITexture Texture_GuildLogo;

		public UITexture Texture_GuildBack;

		public UITexture Texture_MyHero;

		public UITexture Texture_CountryFlag;

		public UILabel Label_GlobalName;

		public UILabel Label_LocalName;

		public UILabel Label_RankPercent;

		public UILabel Label_Score;

		private Component Root;

		public void Init(Component componentRoot)
		{
			Root = componentRoot;
		}

		public void InitGuildLogo(byte logoIndex)
		{
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(logoIndex);
			UIHelper.ResetPlaneAssetTextureAsync(Root, Texture_GuildLogo, guildLogoIcon);
		}

		public void InitGuildBack(byte bgIndex)
		{
			AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(bgIndex);
			UIHelper.ResetPlaneAssetTextureAsync(Root, Texture_GuildBack, guildBackGround2D);
		}

		public void InitMyInfo(int avatarIndex, string countryCode, string name, string rankPercent, string score, bool isGlobal)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(UIAvatarHelper.GetBattlePortraitPath(avatarIndex));
			UIHelper.ResetPlaneAssetTextureAsync(Root, Texture_MyHero, plainTexture);
			UIHelper.EnableGameObject(Label_RankPercent, !rankPercent.Equals("0"));
			UIHelper.SetLabelText(Label_RankPercent, rankPercent);
			UIHelper.SetLabelText(Label_Score, score);
			UIHelper.EnableGameObject(Label_GlobalName, isGlobal);
			UIHelper.EnableGameObject(Label_LocalName, !isGlobal);
			if (isGlobal)
			{
				UIHelper.SetLabelText(Label_GlobalName, name);
				CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
				if (dataByCode != null)
				{
					AssetTexture plainTexture2 = UIHelper.GetPlainTexture(dataByCode.IconPath);
					UIHelper.ResetPlaneAssetTextureAsync(Root, Texture_CountryFlag, plainTexture2);
				}
			}
			else
			{
				UIHelper.SetLabelText(Label_LocalName, name);
			}
		}
	}

	[Serializable]
	public class WorldBossInfoUiData
	{
		public UITexture Texture_Boss;

		public UILabel Label_Name;

		public UILabel Label_RemainTime;

		public Transform Transform_OpenedRoot;

		public Transform Transform_ClosedRoot;

		private Component Root;

		public void Init(Component componentRoot)
		{
			Root = componentRoot;
		}

		public void InitBossInfo(string bossInfoImage, string name)
		{
			if (!String_Extension_Method.IsNullOrEmpty(bossInfoImage))
			{
				AssetTexture assetTexture = new AssetTexture();
				assetTexture.Open(bossInfoImage);
				UIHelper.ResetPlaneAssetTextureAsync(Root, Texture_Boss, assetTexture);
			}
			else
			{
				VespaDebug.LogError("GuildRaid bossinfoimage is null");
			}
			UIHelper.SetLabelText(Label_Name, name);
		}

		public void InitBossStateLabel(bool isOpened, int remainTime)
		{
			UIHelper.EnableGameObject(Transform_OpenedRoot, isOpened);
			UIHelper.EnableGameObject(Transform_ClosedRoot, !isOpened);
			if (isOpened)
			{
				UIHelper.EnableGameObject(Label_RemainTime, enable: true);
				UILabel label_RemainTime = Label_RemainTime;
				object key = GuildRaid.GUILDRAID_STATE_OPENED;
				object[] array = new object[1];
				array[0] = UIHelper.GetRemainTimeLabelText(remainTime);
				UIHelper.SetLabelText(label_RemainTime, LocalizationManager.GetString((Enum)key, array));
			}
		}
	}

	public enum EToggle
	{
		None,
		World,
		WorldPrevSeason
	}

	public interface Listener
	{
		void OnClickEnter(WorldBossData data);

		void OnClickInfo();
	}

	public UILabel Label_RankingTitle;

	public UILabel Label_RankerPage;

	public UIBaseGrid RankerGrid;

	public Transform EmptyRankerRoot;

	public UIButton Button_Enter;

	public UIToggle Toggle_WorldRanking;

	public UIToggle Toggle_WorldPrevSeasonRanking;

	public GameObject Widget_Ranking;

	public const string PrefabPath = "Resources/Prefab2/Window/WorldBoss/WorldBossGlobalRanking";

	public const int MaxPageCount = 10;

	public MyRankingInfoUiData MyRankingInfoUi;

	public WorldBossInfoUiData BossInfoUi;

	private bool _firstUpdate;

	private int _page;

	private Listener _listener;

	private List<WorldBossGlobalRankerComponent> _rankerComponentList = new List<WorldBossGlobalRankerComponent>();

	private float _lastRefreshInfoTime;

	private EToggle _toggle;

	public WorldBossData WorldBossData { get; private set; }

	public CreatureData BossCreatureData { get; private set; }

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RefreshInfo();
	}

	public override void Reset()
	{
		base.Reset();
		InitToggle(EToggle.None);
		_toggle = EToggle.None;
		_firstUpdate = true;
		ClearRanker();
	}

	public void Open(Listener listener, int chapterIndex, Action onOpen = null)
	{
		_listener = listener;
		WorldBossData = GameRoot.Instance.WorldBossDataContainer.GetData(chapterIndex);
		_lastRefreshInfoTime = Time.time;
		title.Key = "WORLDBOSS_MAIN_TITLE";
		_firstUpdate = true;
		if (WorldBossData == null)
		{
			VespaDebug.LogError("WorldBossChapter not found : chapterIndex=" + chapterIndex);
			return;
		}
		Open(delegate
		{
			Init();
			Vespa_System_Action_Extension_Method.fire(onOpen);
		});
	}

	public void Init()
	{
		MyRankingInfoUi.Init(this);
		BossInfoUi.Init(this);
		SetBossDesc();
		InitGuildLogo();
		InitStateLabel();
		InitButtonState();
		InitToggle(EToggle.World);
		WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
		bool isActive = worldBossInfo != null && worldBossInfo.Status != WorldBossStatus.Ended;
		OpenWorldBossRanking openWorldBossRanking = new OpenWorldBossRanking();
		openWorldBossRanking.IsActive = isActive;
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(openWorldBossRanking);
	}

	private void InitButtonState()
	{
		WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
		bool flag = worldBossInfo != null && worldBossInfo.Status != WorldBossStatus.Ended;
		int remainBattleTime = GetRemainBattleTime();
		UIHelper.EnableButton(Button_Enter, flag && remainBattleTime > 0);
	}

	private void ClearRanker()
	{
		FactoryBase.ReleaseAndClearList(_rankerComponentList);
	}

	private void SetBossDesc()
	{
		int chapterIndex = WorldBossData.ChapterIndex;
		int dungeonIndex = WorldBossData.DungeonIndex;
		BossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy, scenario: false, out var bossWaveData);
		if (BossCreatureData == null)
		{
			VespaDebug.LogError("WorldBoss BossCreatureData not found: chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		string visualInfo = bossWaveData?.BossVisualInfo;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(BossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		BossInfoUi.InitBossInfo(WorldBossData.BossInfoImage, creatureName);
	}

	private void RefreshMyHeroInfo(WorldBossRankerInfo rankInfo)
	{
		if (!SceneAutoBehaviour<WorldBossManagement>.isValidInstance)
		{
			return;
		}
		RefreshTabInfo();
		string empty = string.Empty;
		string score = "0";
		string rankPercent = "0";
		int avatarHeroIndex;
		string countryCode;
		if (rankInfo == null || rankInfo.Nick == null)
		{
			empty = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
			avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex;
			countryCode = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetCountryFlagCode();
		}
		else
		{
			if (rankInfo.Nick != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick)
			{
				rankInfo.Nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
			}
			empty = rankInfo.Nick;
			score = Vespa_Long_Extension_Method.ToCommaString(rankInfo.Score);
			avatarHeroIndex = rankInfo.AvatarHeroIndex;
			countryCode = rankInfo.CountryCode;
			int num = rankInfo.Rank + 1;
			if (num <= 0)
			{
				score = "0";
				rankPercent = "0";
			}
			else
			{
				rankPercent = LocalizationManager.GetString(NShared.NLocalization.WorldBoss.WORLDBOSS_MY_RANKING, num);
			}
		}
		MyRankingInfoUi.InitMyInfo(avatarHeroIndex, countryCode, empty, rankPercent, score, isGlobal: true);
		RefreshTitle();
	}

	private void InitGuildLogo()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null)
		{
			MyRankingInfoUi.InitGuildLogo(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo);
			int num = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
			if (num <= 0 || num > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				num = 1;
			}
			MyRankingInfoUi.InitGuildBack((byte)num);
		}
		else
		{
			MyRankingInfoUi.InitGuildBack(1);
		}
	}

	private void InitStateLabel()
	{
		WorldBossStatus num = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo?.Status ?? WorldBossStatus.Ended;
		int remainBattleTime = GetRemainBattleTime();
		bool isOpened = num != WorldBossStatus.Ended && remainBattleTime > 0;
		BossInfoUi.InitBossStateLabel(isOpened, remainBattleTime);
	}

	private int GetRemainBattleTime()
	{
		WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
		if (worldBossInfo == null)
		{
			return 0;
		}
		int num = WorldBossData.DealBattleDuration + WorldBossData.NormalBattleDuration;
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(worldBossInfo.CreatedTime, out var o_dateTime))
		{
			int num2 = (int)(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - o_dateTime).TotalSeconds;
			return Mathf.Max(num - num2, 0);
		}
		return 0;
	}

	private void RefreshRanker(int page)
	{
		_page = page;
		UIHelper.SetLabelText(Label_RankerPage, page + 1 + "/" + 10);
		if (_listener != null)
		{
			if (_toggle == EToggle.World)
			{
				SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.GetCurrentRankerList(page, RefreshRanker);
			}
			else if (_toggle == EToggle.WorldPrevSeason)
			{
				SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.GetPrevRankerList(page, RefreshRanker);
			}
		}
	}

	private void RefreshRanker(WorldBossRankerInfo[] rankerInfos)
	{
		ClearRanker();
		if (rankerInfos == null)
		{
			UIHelper.EnableGameObject(EmptyRankerRoot, enable: true);
			return;
		}
		string myServerGroup = string.Empty;
		WorldBossRankerInfo rankInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RankInfo;
		if (rankInfo != null)
		{
			myServerGroup = rankInfo.ServerGroup;
		}
		foreach (WorldBossRankerInfo rankerInfo in rankerInfos)
		{
			WorldBossGlobalRankerComponent worldBossGlobalRankerComponent = UIHelper.AddGridItem(RankerGrid, WorldBossGlobalRankerComponent.PrefabAuto);
			worldBossGlobalRankerComponent.SetData(rankerInfo, myServerGroup, this);
			_rankerComponentList.Add(worldBossGlobalRankerComponent);
		}
		UIHelper.RepositionGrid(RankerGrid);
		UIHelper.EnableGameObject(EmptyRankerRoot, _rankerComponentList.Count == 0);
	}

	private void RefreshInfo()
	{
		if (!(Time.time - _lastRefreshInfoTime < 1f))
		{
			_lastRefreshInfoTime = Time.time;
			InitStateLabel();
			InitButtonState();
		}
	}

	private void RefreshTitle()
	{
		string stringDirect = LocalizationManager.GetStringDirect("WORLDBOSS_WORLD_RANK_TITLE");
		UIHelper.SetLabelText(Label_RankingTitle, stringDirect);
	}

	private void RefreshMyInfo(Action<int> onNext)
	{
		if (_listener != null)
		{
			if (_firstUpdate)
			{
				RefreshMyHeroInfo(SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RankInfo);
				Vespa_System_Action_Extension_Method.fire(onNext, 0);
				_firstUpdate = false;
			}
			else if (_toggle == EToggle.World)
			{
				SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestCurrentRankInfo(RefreshMyHeroInfo, onNext);
			}
			else if (_toggle == EToggle.WorldPrevSeason)
			{
				SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestPrevRankInfo(RefreshMyHeroInfo, onNext);
			}
		}
	}

	private void RefreshTabInfo()
	{
		UIHelper.EnableGameObject(Widget_Ranking, enable: true);
	}

	private void InitToggle(EToggle startToggle)
	{
		Toggle_WorldRanking.Set(startToggle == EToggle.World);
		Toggle_WorldPrevSeasonRanking.Set(startToggle == EToggle.WorldPrevSeason);
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (Toggle_WorldRanking.value)
		{
			eToggle = EToggle.World;
		}
		else if (Toggle_WorldPrevSeasonRanking.value)
		{
			eToggle = EToggle.WorldPrevSeason;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			RefreshMyInfo(RefreshRanker);
		}
	}

	void WorldBossGlobalRankerComponent.Listener.OnClickComponent(long accountId)
	{
		NGame2.NGuild.GuildMemberManager memberManager = GlobalAutoBehaviour<GuildManager>.instance.MemberManager;
		if (memberManager != null)
		{
			GuildMemberInfo guildMemberInfo = memberManager.Get(accountId);
			if (guildMemberInfo != null && guildMemberInfo.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
			{
				InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(guildMemberInfo, InspectUserCategory.WorldBoss);
				SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
			}
		}
	}

	public void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(WorldBossData);
		}
	}

	public void OnClickInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickInfo();
		}
	}

	public void OnClickGuide()
	{
		if (SceneAutoBehaviour<WorldBossManagement>.isValidInstance)
		{
			SceneAutoBehaviour<WorldBossManagement>.instance.GetInstance<WorldBossGlobalGuide>().Open();
		}
	}

	public void OnClickRankingReward()
	{
		if (SceneAutoBehaviour<WorldBossManagement>.isValidInstance)
		{
			SceneAutoBehaviour<WorldBossManagement>.instance.GetInstance<WorldBossGlobalRewardInfo>().Open();
		}
	}

	public void OnClickNextPage()
	{
		if (_page < 9)
		{
			int page = _page + 1;
			RefreshRanker(page);
		}
	}

	public void OnClickPrevPage()
	{
		if (_page > 0)
		{
			int page = _page - 1;
			RefreshRanker(page);
		}
	}

	HonoraryChamber.ServerType RecordsOfHonorShortcut.ServerTypeProvider.GetCurrentServerType()
	{
		return HonoraryChamber.ServerType.World;
	}

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType
	{
		get
		{
			return StaminaType.WorldBossGlobalTicket;
		}
	}
}
