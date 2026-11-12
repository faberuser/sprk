using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext.NContext.NCampaignRuntime;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NContents;
using NGame2.NEventTrigger;
using NGame2.NGuild;
using NGame2.NOrdeal;
using NGame2.NPet;
using NGame2.NScene.NPreloader;
using NGame2.NSocket;
using NGame2.NSubStory;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NShared;
using NShared.BeginCampaign;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NSocket;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NBattleContext;

public class CampaignContext : SingletonAuto<CampaignContext>, OfflinePlay.IDataProvider, IDataProvider, OnlinePlay.IDataProvider
{
	public int[] HeroIndices;

	public int[] AiHeroIndices;

	public int[] GroupHeroIndices;

	public int LeaderHeroIndex;

	public long[] AccountIds;

	public EnterDungeonData EnterDungeon = new EnterDungeonData();

	public int WorldMapEventIndex;

	public int WorldMapNodeIndex;

	public long MultiplayMasterId;

	public int TowerIndex;

	public int TowerFloor;

	public int[] DeltaHeroIndices;

	public int[] ZetaHeroIndices;

	public BattleInstanceExtraData ExtraData = new BattleInstanceExtraData();

	public int RaidIndex;

	public int RaidLevel;

	public int WorldBossIndex;

	public long OnetimeBoosterCount;

	public int OnetimeBoosterMaxCount;

	public int OnlineGameSpeedRatio;

	public bool IsSkipRewardPopup;

	private EnterDungeonData _requestEnterDungeon = new EnterDungeonData();

	private OfflinePlay.ResponseData _responseData;

	private OnlineBattleCallback _onlineBattleCallback;

	private ResultCreatureInfo[] _userHeroInfos;

	private ResultCreatureInfo[] _enemyHeroInfos;

	private CachedHeroInfo[] _forcedEnemyHeroInfos;

	private BattleCreatureCreateInfo[] _forcedUserBattleCreatureCreateInfos;

	private ICampaignRuntime _campaignRuntime;

	public int[] AffixIndices => ExtraData.AffixIndices;

	private List<List<BattleCreatureCreateInfo>> BattleCreatureCreateInfoList
	{
		get
		{
			if (CheckEachWaveEachTeam(_requestEnterDungeon) && !__Extension_Method_ICollection__.IsNullOrEmpty(GroupHeroIndices))
			{
				List<int> list = HeroIndices.ToList();
				int i = 0;
				while (i < GroupHeroIndices.Length)
				{
					list.RemoveAll((int index) => index == GroupHeroIndices[i]);
					int num = i + 1;
					i = num;
				}
				List<List<BattleCreatureCreateInfo>> list2 = new List<List<BattleCreatureCreateInfo>>();
				list2.Clear();
				List<BattleCreatureCreateInfo> list3 = new List<BattleCreatureCreateInfo>();
				list3.Clear();
				list3.AddRange(CreateBattleCreatureCreateInfoList(HeroIndices, null));
				list2.Add(list3);
				list3 = new List<BattleCreatureCreateInfo>();
				list3.Clear();
				list3.AddRange(CreateBattleCreatureCreateInfoList(GroupHeroIndices, null));
				list2.Add(list3);
				return list2;
			}
			return null;
		}
	}

	public RewardItemInfo[] GetResoponseDropItems()
	{
		if (_responseData == null)
		{
			return null;
		}
		return _responseData.DropItems;
	}

	public int GetResponseDropGold()
	{
		if (_responseData == null)
		{
			return 0;
		}
		return _responseData.DropGold;
	}

	public ResultCreatureInfo[] GetUserHeroInfos()
	{
		return _userHeroInfos;
	}

	public ResultCreatureInfo[] GetEnemyHeroInfos()
	{
		return _enemyHeroInfos;
	}

	public bool IsGuildRaidCampaign()
	{
		return GetBattleType() == BattleType.GuildRaid;
	}

	public bool IsChallengeRaidSingle()
	{
		return GetBattleType() == BattleType.ChallengeRaidSingle;
	}

	public bool IsChallengeRaidMulti()
	{
		return GetBattleType() == BattleType.ChallengeRaid;
	}

	public bool IsWorldBossCampaign()
	{
		return GetBattleType() == BattleType.WorldBoss;
	}

	public bool IsGuildSuppressCampaign()
	{
		if (GetBattleType() != BattleType.GuildSuppress)
		{
			return false;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(RaidIndex, RaidLevel);
		if (data == null)
		{
			return false;
		}
		if (data.Type != RaidType.GuildSuppress && data.Type != RaidType.GuildSuppressSingle)
		{
			return false;
		}
		return true;
	}

	public bool IsEclipseBattle()
	{
		return GetBattleType() == BattleType.Eclipse;
	}

	public void BeginCampaignRequest(NShared.BeginCampaign.Request request)
	{
		_requestEnterDungeon = new EnterDungeonData(request.ChapterIndex, request.DungeonIndex, request.LinkedDungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty, request.ScenarioDungeon);
		BeginCampaignRequestInternal(request);
	}

	public bool CheckEachWaveEachTeam(EnterDungeonData data)
	{
		if (data == null)
		{
			return false;
		}
		if (data.OriginalDungeonData == null)
		{
			return false;
		}
		return CheckEachWaveEachTeam(data.LinkedDungeonData.BattleType);
	}

	public static bool CheckEachWaveEachTeam(BattleType battleType)
	{
		if (battleType == BattleType.ArcdimRaid || battleType == BattleType.ArcdimHardRaid)
		{
			return true;
		}
		return false;
	}

	public void SetCustomCampaign(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficultType)
	{
		EnterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, 0, difficultType);
	}

	public void BeginCampaignResponse(NShared.BeginCampaign.Response response)
	{
		_responseData = new OfflinePlay.ResponseData();
		_responseData.DropGold = response.DropGold;
		_responseData.DropItems = response.DropItems;
		_responseData.CheckIntegrity = response.CheckIntegrity;
	}

	public void SetRewardItems(RewardItemInfo[] rewardItems)
	{
		if (_responseData == null)
		{
			_responseData = new OfflinePlay.ResponseData();
		}
		_responseData.DropItems = rewardItems;
	}

	public void SetInitialUserHeroInfo(ResultCreatureInfo[] heroInfos)
	{
		_userHeroInfos = heroInfos;
	}

	public void SetInitialEnemyHeroInfo(ResultCreatureInfo[] heroInfos)
	{
		_enemyHeroInfos = heroInfos;
	}

	public void SetInitialHeroInfo(ResultCreatureInfo[] heroInfos)
	{
		if (__Extension_Method_ICollection__.COUNT(heroInfos) != 0)
		{
			_userHeroInfos = heroInfos.Where((ResultCreatureInfo e) => e.TeamId == 0).ToArray();
			_enemyHeroInfos = heroInfos.Where((ResultCreatureInfo e) => e.TeamId == 1).ToArray();
		}
	}

	public void SetForcedEnemyHeroInfo(CachedHeroInfo[] heroInfos)
	{
		_forcedEnemyHeroInfos = heroInfos;
	}

	public void SetForcedUserBattleCreatureCreateInfos(BattleCreatureCreateInfo[] forcedUserBattleCreatureCreateInfos)
	{
		_forcedUserBattleCreatureCreateInfos = forcedUserBattleCreatureCreateInfos;
	}

	public void SetOnlineBattleCallbackIsBattleEnd(bool IsBattleEnd)
	{
		_onlineBattleCallback.SetBattleEnd(IsBattleEnd);
	}

	private void EndCampaign(bool win, int star, Dictionary<int, int> killMonsterCount, IList<ResultCreatureInfo> creatureInfos, int pureBattleTime, long elapsedBattleTime, string battleLogString = null)
	{
		if (win)
		{
			EnterDungeon = new EnterDungeonData(EnterDungeon.Chapter, EnterDungeon.OriginalDungeon, EnterDungeon.OriginalDungeon, EnterDungeon.Difficulty, EnterDungeon.IsScenario);
		}
		List<int> list = new List<int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureInfos); i++)
		{
			ResultCreatureInfo resultCreatureInfo = creatureInfos[i];
			if (resultCreatureInfo != null && resultCreatureInfo.Hp > 0)
			{
				int index = resultCreatureInfo.Index;
				SprkAddPartySurvivor(list, index, HeroIndices, GroupHeroIndices);
			}
		}
		SceneAutoBehaviour<NGame2.NUI.NManager.NBattle.EndCampaign>.instance.Open(HeroIndices, AiHeroIndices, GroupHeroIndices, AccountIds, win, star, killMonsterCount, creatureInfos, list, pureBattleTime, elapsedBattleTime, battleLogString);
	}

	private void BeginCampaignRequestInternal(NShared.BeginCampaign.Request request)
	{
		HeroIndices = request.HeroIndices;
		AiHeroIndices = request.AiHeroIndices;
		GroupHeroIndices = request.GroupHeroIndices;
		LeaderHeroIndex = request.LeaderHeroIndex;
		AccountIds = request.AccountIds;
		EnterDungeon = new EnterDungeonData(request.ChapterIndex, request.DungeonIndex, request.LinkedDungeonIndex, (NShared.DungeonDifficultyType)request.DungeonDifficulty, request.ScenarioDungeon);
		WorldMapEventIndex = request.WorldMapEventIndex;
		WorldMapNodeIndex = request.WorldMapNodeIndex;
		MultiplayMasterId = request.MultiplayMasterId;
		TowerIndex = request.TowerIndex;
		TowerFloor = request.TowerFloor;
		RaidIndex = request.RaidIndex;
		RaidLevel = request.RaidLevel;
		WorldBossIndex = request.WorldBossIndex;
		OnetimeBoosterCount = request.ItemBoosterCount;
		OnlineGameSpeedRatio = request.OnlineGameSpeedRatio;
		DeltaHeroIndices = request.DeltaHeroIndices;
		ZetaHeroIndices = request.ZetaHeroIndices;
		ExtraData.AffixIndices = request.AffixIndices;
		if (_onlineBattleCallback != null)
		{
			_onlineBattleCallback.Close();
			_onlineBattleCallback = null;
		}
		if (IsMultiplay())
		{
			_onlineBattleCallback = new OnlineBattleCallback(GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector);
			_onlineBattleCallback.OnCreatureDestroyedCallback = OnCreatureDestroyed;
			_onlineBattleCallback.OnBattleResultCallback = OnBattleResult;
			_onlineBattleCallback.OnBattleEndCallback = OnBattleEnd;
			_onlineBattleCallback.OnSetNextDungeonNotCallback = OnSetNextDungeon;
			_onlineBattleCallback.OnSetNextTeamNotCallback = OnSetNextTeam;
			_onlineBattleCallback.OnBattleCancelNotCallback = OnBattleCancel;
			_onlineBattleCallback.Init();
		}
		_userHeroInfos = null;
		_enemyHeroInfos = null;
		_forcedEnemyHeroInfos = null;
		_forcedUserBattleCreatureCreateInfos = null;
		InitCampaignRuntime();
	}

	private void InitCampaignRuntime()
	{
		_campaignRuntime = null;
		if (IsGuildRaidCampaign())
		{
			_campaignRuntime = new NGame2.NBattleContext.NContext.NCampaignRuntime.GuildRaid();
		}
		else if (IsWorldBossCampaign())
		{
			_campaignRuntime = new NGame2.NBattleContext.NContext.NCampaignRuntime.WorldBoss(WorldBossIndex);
		}
		else if (IsGuildSuppressCampaign())
		{
			_campaignRuntime = new NGame2.NBattleContext.NContext.NCampaignRuntime.GuildSuppress();
		}
	}

	public ICampaignRuntime GetCampaignRuntime()
	{
		return _campaignRuntime;
	}

	private BattleType GetBattleType()
	{
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			VespaDebug.LogError("DungeonData not found : chapterIndex=" + EnterDungeon.Chapter + " linkeddungeonIndex=" + EnterDungeon.LinkedDungeon);
			return BattleType.None;
		}
		return linkedDungeonData.BattleType;
	}

	public bool IsMultiplay()
	{
		return MultiplayMasterId > 0;
	}

	private bool IsRepeatable()
	{
		if (IsMultiplay())
		{
			return false;
		}
		if (EnterDungeon.OriginalDungeonData == null)
		{
			return false;
		}
		if (EnterDungeon.OriginalDungeon != EnterDungeon.LinkedDungeon)
		{
			return false;
		}
		if (SingletonAuto<ContentsOpenChecker>.instance.IsContentsOpenNode(EnterDungeon.Chapter, EnterDungeon.OriginalDungeon, EnterDungeon.Difficulty))
		{
			return false;
		}
		return true;
	}

	private bool IsPartyDungeon(CampaignDungeonData data)
	{
		if (data == null)
		{
			return false;
		}
		return GameRoot.Instance.PartyDungeonDataContainer.GetData(data.ChapterIndex, data.DungeonIndex) != null;
	}

	private EBattleMenu GetBattleMenuType()
	{
		EBattleMenu eBattleMenu = EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto;
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData != null)
		{
			BattleDefineData battleDefine = linkedDungeonData.GetBattleDefine(EnterDungeon.IsScenario);
			if (battleDefine != null)
			{
				eBattleMenu = battleDefine.BattleMenu;
			}
		}
		if (!IsRepeatable())
		{
			eBattleMenu = (EBattleMenu)((ulong)eBattleMenu & 0xFFFFFFFFFFFFFFEFuL);
		}
		if (IsMultiplay())
		{
			eBattleMenu = (EBattleMenu)((ulong)eBattleMenu & 0xFFFFFFFFFFFFFFFDuL);
			eBattleMenu = (EBattleMenu)((ulong)eBattleMenu & 0xFFFFFFFFFFFF7FFFuL);
			if (IsPartyDungeon(linkedDungeonData))
			{
				eBattleMenu |= EBattleMenu.SystemButtonAutoSimple;
				eBattleMenu |= EBattleMenu.PartyExitButton;
			}
		}
		return SprkPartyRetry.Menu(eBattleMenu, this);
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		preloader.EnterDungeon = EnterDungeon;
		preloader.PreloadDungeonMonster = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetPrealodCreateMonster();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]);
			preloader.AddCreature(heroInfo, null, loadCreatureImage: true);
		}
	}

	bool IDataProvider.CanNextDungeon()
	{
		int nextDungeonIndex = EnterDungeon.LinkedDungeonData.NextDungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(EnterDungeon.Chapter, nextDungeonIndex);
		if (data == null)
		{
			return false;
		}
		bool num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(EnterDungeon.Chapter, nextDungeonIndex);
		PlayerVisitNode playerVisitNode = new PlayerVisitNode
		{
			ChapterIndex = EnterDungeon.Chapter,
			DungeonIndex = nextDungeonIndex
		};
		if (!num && data.FirstDungeonIndex != 0)
		{
			playerVisitNode.DungeonIndex = data.FirstDungeonIndex;
		}
		if (SingletonAuto<EventTriggerManager>.instance.CanRaiseEvent(playerVisitNode))
		{
			return false;
		}
		if (TowerIndex != 0)
		{
			TowerFloorData data2 = GameRoot.Instance.TowerFloorDataContainer.GetData(TowerIndex, TowerFloor + 1);
			if (data2 == null || !TowerManagement.IsOpenedFloor(data2))
			{
				return false;
			}
		}
		return true;
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return EnterDungeon;
	}

	void IDataProvider.OnEnterBattle()
	{
	}

	void IDataProvider.OnWaveMoveEnd()
	{
	}

	bool IDataProvider.CanStartBattle()
	{
		return true;
	}

	BattleStartEffectData IDataProvider.GetBattleStartEffectData()
	{
		if (IsEclipseBattle())
		{
			return new BattleStartEffectData
			{
				PrefabWindow = "Resources/Prefab2/Window/SoulWeapon/EclipseBattleStart",
				ScreenEffectCode = "UI.Arena.Count",
				SoundEffectCode = "UI.Eclipse.BattleStart"
			};
		}
		return null;
	}

	IPlay IDataProvider.GetBattlePlay()
	{
		if (IsMultiplay())
		{
			return SceneAutoBehaviour<OnlinePlay>.instance;
		}
		return SceneAutoBehaviour<OfflinePlay>.instance;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return GetBattleType();
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return EnterDungeon.LinkedDungeonData?.GetBattleDefine(EnterDungeon.IsScenario);
	}

	BattleFacade.BattleConditionBase IDataProvider.CreateBattleCondition()
	{
		return new CampaignCondition();
	}

	bool IDataProvider.IsOnline()
	{
		return IsMultiplay();
	}

	void IDataProvider.OnUpdate()
	{
		if (_campaignRuntime != null)
		{
			_campaignRuntime.Update();
		}
	}

	bool IDataProvider.IsTowerBattle()
	{
		return TowerIndex >= 1;
	}

	int IDataProvider.GetTowerFloor()
	{
		return TowerFloor;
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return null;
		}
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
		{
			return null;
		}
		MyGuildInfo info = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info;
		return new GuildFlagData
		{
			EGuildFlagDisplayType = linkedDungeonData.EGuildFlagDisplayType,
			LeftGuildFlag = (info?.Flag ?? 1),
			RightGuildFlag = 1,
			LeftGuildLogo = (info?.Logo ?? 1),
			RightGuildLogo = 1,
			LeftGuildFlagBG = (info?.Back ?? 1),
			RightGuildFlagBG = 1,
			ExistLeftGuild = (info != null),
			ExistRightGuild = false
		};
	}

	List<IBattleLogRuntime> OfflinePlay.IDataProvider.CreateInitialBattleLog()
	{
		return new List<IBattleLogRuntime>
		{
			CreateEnterPlayerBattleLog_Me(),
			CreateEnterPlayerBattleLog_Enemy()
		};
	}

	int OfflinePlay.IDataProvider.GetChapterIndex()
	{
		return EnterDungeon.Chapter;
	}

	int OfflinePlay.IDataProvider.GetDungeonIndex()
	{
		return EnterDungeon.LinkedDungeon;
	}

	NShared.DungeonDifficultyType OfflinePlay.IDataProvider.GetDifficulty()
	{
		return EnterDungeon.Difficulty;
	}

	bool OfflinePlay.IDataProvider.IsScenarioDungeon()
	{
		return EnterDungeon.IsScenario;
	}

	int OfflinePlay.IDataProvider.GetRequestDungeonIndex()
	{
		return EnterDungeon.OriginalDungeon;
	}

	OfflinePlay.ResponseData OfflinePlay.IDataProvider.GetResponseData()
	{
		return _responseData;
	}

	int OfflinePlay.IDataProvider.GetWorldMapEventIndex()
	{
		return WorldMapEventIndex;
	}

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		return GetBattleMenuType();
	}

	void OfflinePlay.IDataProvider.HandleBattleBegin()
	{
	}

	List<List<BattleCreatureCreateInfo>> OfflinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return BattleCreatureCreateInfoList;
	}

	List<CachedHeroInfo> IDataProvider.GetPreloadBattleLogs()
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			list.Add(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]));
		}
		return list;
	}

	void OfflinePlay.IDataProvider.HandleBattleEnd(TeamType winTeamType, BattleInstance battleInstance)
	{
		Dictionary<int, int> killCount = battleInstance.GetKillCount(TeamType.Left);
		bool flag = winTeamType == TeamType.Left;
		int campaignResultStar = battleInstance.GetCampaignResultStar(flag, EnterDungeon.Difficulty);
		List<ResultCreatureInfo> creatureInfoList = BattleInstanceIntegrityChecker.GetCreatureInfoList(battleInstance);
		if (flag)
		{
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle();
			SceneAutoBehaviour<WavePositionCollector>.instance.CreateLootingOnly();
		}
		string battleLogString = null;
		int pureBattleTime = (int)battleInstance.GlobalTimer.GetCurrentTimeMs() / 1000;
		long elapsedPureBattleTimeMs = battleInstance.GetElapsedPureBattleTimeMs();
		if (_responseData != null && _responseData.CheckIntegrity)
		{
			battleLogString = ReplayUtil.CreateBattleLogString(battleInstance, JsonMarshaler.EncodeJsonString, compress: true);
		}
		EndCampaign(flag, campaignResultStar, killCount, creatureInfoList, pureBattleTime, elapsedPureBattleTimeMs, battleLogString);
	}

	private void CheckBattleIntegrity(BattleInstance battleInstance, IList<ResultCreatureInfo> creatureInfos)
	{
		if (EnterDungeon.IsScenario)
		{
			return;
		}
		BattleInstanceExtraData extraData = battleInstance.ExtraData;
		BattleType battleType = GetBattleType();
		BattleInstanceBuilder battleInstanceBuilder = BattleInstanceIntegrityChecker.CreateBattleInstanceBuilderForCheck(EnterDungeon.Chapter, EnterDungeon.LinkedDungeon, (int)EnterDungeon.Difficulty, extraData);
		if (battleInstanceBuilder == null)
		{
			VespaDebug.LogError("Failed to create battleinstance builder. chapter=" + EnterDungeon.Chapter + " dungeon=" + EnterDungeon.LinkedDungeon + " type=" + battleType);
			return;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			int key = HeroIndices[i];
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(key);
			if (cachedHeroInfo == null)
			{
				VespaDebug.LogError("Hero info not found. heroIndex=" + key);
				return;
			}
			CachedHeroInfo cachedHeroInfo2 = JsonMarshaler.Clone(cachedHeroInfo);
			cachedHeroInfo2.Validate();
			list.Add(cachedHeroInfo2);
		}
		List<BaseBattleLog> allBattleLogList = battleInstance.BattleLogContainer.GetAllBattleLogList();
		List<ResultCreatureInfo> verifiedCreatureInfoList = null;
		BattleInstanceIntegrityChecker.CheckResult checkResult = BattleInstanceIntegrityChecker.Run(battleInstanceBuilder, list, _forcedEnemyHeroInfos, _userHeroInfos, _enemyHeroInfos, allBattleLogList, creatureInfos, out verifiedCreatureInfoList, JsonMarshaler.Parse<PlayerEnterBattleLog>, JsonMarshaler.Parse<WaveReadyBattleLog>, -1);
		if (checkResult != null)
		{
			VespaDebug.LogError("Battle integrity check failed : " + checkResult.Result);
		}
	}

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return true;
	}

	BattleInstanceExtraData OfflinePlay.IDataProvider.GetExtraData()
	{
		return ExtraData;
	}

	int OnlinePlay.IDataProvider.GetChapterIndex()
	{
		return EnterDungeon.Chapter;
	}

	int OnlinePlay.IDataProvider.GetDungeonIndex()
	{
		return EnterDungeon.LinkedDungeon;
	}

	NShared.DungeonDifficultyType OnlinePlay.IDataProvider.GetDifficulty()
	{
		return EnterDungeon.Difficulty;
	}

	void OnlinePlay.IDataProvider.OnReturnToLobby()
	{
	}

	OnlineBattleCallback OnlinePlay.IDataProvider.GetBattleCallback()
	{
		return _onlineBattleCallback;
	}

	int OnlinePlay.IDataProvider.GetWorldMapEventIndex()
	{
		return WorldMapEventIndex;
	}

	IRequester OnlinePlay.IDataProvider.GetSocketConnector()
	{
		return GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector;
	}

	string OnlinePlay.IDataProvider.GetError()
	{
		return null;
	}

	int OnlinePlay.IDataProvider.OnlineGameSpeedRatio()
	{
		return System.Math.Max(OnlineGameSpeedRatio, 1);
	}

	List<List<BattleCreatureCreateInfo>> OnlinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return BattleCreatureCreateInfoList;
	}

	BattleInstanceExtraData OnlinePlay.IDataProvider.GetExtraData()
	{
		return ExtraData;
	}

	private static BattleCreatureCreateInfo.BuffSkillInfo GetMyBuffSkillInfo()
	{
		GuildSkillLevelData[] array = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance.GetGuildSkillDataList() : null);
		List<AccountBuffDataBase> activeAccountBuffDataBaseList = GlobalAutoBehaviour<AccountManager>.instance.GetActiveAccountBuffDataBaseList();
		ClassBuffDataBase[] classBuffDataBasess = GetClassBuffDataBasess();
		OrdealArenaSkillDataBase[] ordealArenaSkillDataBases = (GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance ? GlobalAutoBehaviour<OrdealArenaManager>.instance.GetOrdealArenaSkillDataBases() : null);
		BattleCreatureCreateInfo.BuffSkillInfo buffSkillInfo = new BattleCreatureCreateInfo.BuffSkillInfo();
		GuildSkillDataBase[] guildSkillDataBases = array;
		buffSkillInfo.GuildSkillDataBases = guildSkillDataBases;
		buffSkillInfo.AccountBuffDataBases = activeAccountBuffDataBaseList?.ToArray();
		buffSkillInfo.ClassBuffDataBasess = classBuffDataBasess;
		buffSkillInfo.OrdealArenaSkillDataBases = ordealArenaSkillDataBases;
		return buffSkillInfo;
	}

	private static ClassBuffDataBase[] GetClassBuffDataBasess()
	{
		return SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper?.GetClassBuffDataListByClassBuffInfo().ToArray();
	}

	private static BattleCreatureCreateInfo.BuffStatInfo GetMyBuffStatInfo()
	{
		List<DefaultExtraStatData> list = new List<DefaultExtraStatData>();
		List<DefaultExtraStatData> petStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetStatDataBaseList();
		SumExtraStatData(list, petStatDataBaseList);
		List<PetStatDataBase> petContentsStatDataBaseList = GlobalAutoBehaviour<PetManager>.instance.GetPetContentsStatDataBaseList();
		OrdealArenaStatDataBase[] ordealArenaStatDataBases = (GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance ? GlobalAutoBehaviour<OrdealArenaManager>.instance.GetOrdealArenaStatDataBases() : null);
		return new BattleCreatureCreateInfo.BuffStatInfo
		{
			ExtraStatDatas = ((__Extension_Method_ICollection__.COUNT(list) > 0) ? list.ToArray() : null),
			OrdealArenaStatDataBases = ordealArenaStatDataBases,
			PetStatDataBases = ((__Extension_Method_ICollection__.COUNT(petContentsStatDataBaseList) > 0) ? petContentsStatDataBaseList.ToArray() : null)
		};
	}

	private static void SumExtraStatData(List<DefaultExtraStatData> extraStatDataList, List<DefaultExtraStatData> targetStatDataList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(targetStatDataList); i++)
		{
			DefaultExtraStatData statDataBase = targetStatDataList[i];
			if (statDataBase != null)
			{
				int num = extraStatDataList.FindIndex((DefaultExtraStatData statData) => statData.OptionType == statDataBase.OptionType);
				if (num == -1)
				{
					DefaultExtraStatData item = new DefaultExtraStatData
					{
						OptionType = statDataBase.OptionType,
						OptionValue = statDataBase.OptionValue
					};
					extraStatDataList.Add(item);
				}
				else
				{
					extraStatDataList[num].OptionValue += statDataBase.OptionValue;
				}
			}
		}
	}

	private void OnCreatureDestroyed(DestroyCreatureNot data)
	{
	}

	private void OnBattleResult(BattleResultNot data)
	{
		if (EnterDungeon == null)
		{
			VespaDebug.LogError("OnBattleResult - EnterDungeon is null.");
			return;
		}
		if (IsEclipseBattle())
		{
			((OnlinePlay.IDataProvider)this)?.GetSocketConnector().Close();
			WaitBattleEnd(data, GameRoot.Instance.ConstantDataContainer.GetValueInt("EclipseWaitBattleEndExpireSecond", 3), EndEclipseTeam);
			return;
		}
		BattleInstance stoppedBattleInstance = SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance();
		if (stoppedBattleInstance == null)
		{
			VespaDebug.LogError("OnBattleResult - battleInstance is null.");
			return;
		}
		int campaignResultStar = stoppedBattleInstance.GetCampaignResultStar(data.Win, EnterDungeon.Difficulty);
		SceneAutoBehaviour<NGame2.NUI.NManager.NBattle.EndCampaign>.instance.Open(HeroIndices, AiHeroIndices, GroupHeroIndices, AccountIds, data.Win, campaignResultStar, data.KillCount, null, data.AliveHeroIndices, 0, 0L);
	}

	private void WaitBattleEnd(BattleResultNot data, int waitSeconds, Action<BattleResultNot, BattleInstance> endBattleAction)
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (waitSeconds <= 0 || (SingletonAuto<BattleFacade>.instance.IsValidBattle() && battleCache.IsBattleEnded))
		{
			if (SingletonAuto<BattleFacade>.instance.IsValidBattle() && !battleCache.IsBattleEnded)
			{
				SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			}
			Vespa_System_Action_Extension_Method.fire(endBattleAction, data, SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance());
		}
		else
		{
			SingletonUpdater<Executer>.instance.DelayRun(1f, this, delegate
			{
				WaitBattleEnd(data, waitSeconds - 1, endBattleAction);
			});
		}
	}

	private void EndEclipseTeam(BattleResultNot data, BattleInstance battleInstance)
	{
		if (battleInstance == null)
		{
			VespaDebug.LogError("OnBattleResult - battleInstance is null.");
		}
		else if (data != null)
		{
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.IsLastDeck())
			{
				int campaignResultStar = battleInstance.GetCampaignResultStar(data.Win, EnterDungeon.Difficulty);
				SceneAutoBehaviour<NGame2.NUI.NManager.NBattle.EndCampaign>.instance.Open(HeroIndices, AiHeroIndices, GroupHeroIndices, AccountIds, data.Win, campaignResultStar, data.KillCount, null, data.AliveHeroIndices, 0, 0L);
			}
			else
			{
				SceneAutoBehaviour<NGame2.NUI.NManager.NBattle.EndCampaign>.instance.OpenEclipseRewardPopup(data.MaxWaveIndex);
			}
		}
	}

	private void OnBattleEnd(BattleEndNot data)
	{
		if (data.Reason == BattleEndNot.ReasonType.MasterLogOff)
		{
			SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetString(Party.PARTY_MASTER_LEFT), delegate
			{
				SingletonAuto<BattleContext>.instance.ReturnToLobby();
			});
		}
		else if (data.Reason == BattleEndNot.ReasonType.AllLogOff)
		{
			SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetString(PartyBattle.PARTY_BATTLE_ALL_LOGOFF), delegate
			{
				SingletonAuto<BattleContext>.instance.ReturnToLobby();
			});
		}
	}

	private void OnBattleCancel(BattleCancelNot data)
	{
		SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
		SceneAutoBehaviour<BattleManagement>.instance.CloseBossStatus();
		SceneAutoBehaviour<BattleManagement>.instance.ClosePatternAlarm();
		string stringDirect = LocalizationManager.GetStringDirect("NETWORK_POPUP_TITLE_COMMON");
		string empty = string.Empty;
		MessagePopup.Open(stringDirect, data.Reason switch
		{
			BattleCancelNot.ReasonType.WaveNotStart => LocalizationManager.GetStringDirect("NETWORK_POPUP_DESC_MEMBER_BATTLE_ENTER_FAIL"), 
			BattleCancelNot.ReasonType.UserLogOff => LocalizationManager.GetStringDirect("NETWORK_POPUP_DESC_MEMBER_DISCONNECT"), 
			_ => "Need LocalData", 
		}, delegate
		{
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
		});
		SceneAutoBehaviour<OnlinePlay>.instance.StopBattle();
	}

	private void OnSetNextDungeon(SetNextDungeonNot data)
	{
		EnterDungeon = new EnterDungeonData(data.ChatperIndex, data.DungeonIndex, data.DungeonIndex, EnterDungeon.Difficulty, EnterDungeon.IsScenario);
	}

	private void OnSetNextTeam(SetNextTeamNot data)
	{
		if (CheckEachWaveEachTeam(EnterDungeon))
		{
			Debug.LogError("==================OnSetNextTeam : " + data.DeckIndex);
		}
		else if (AccountManager.IsLogined())
		{
			EnterDungeon = new EnterDungeonData(data.ChatperIndex, data.DungeonIndex, data.DungeonIndex, EnterDungeon.Difficulty, EnterDungeon.IsScenario);
			GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.SetDeckIndex(data.DeckIndex);
		}
	}

	private void OnWaveEndNot(WaveEndNot data)
	{
	}

	public static List<BattleCreatureCreateInfo> CreateBattleCreatureCreateInfoList(IList<int> heroIndices, IList<long> accountIds)
	{
		return GetBattleCreatureCreateInfoList(heroIndices, accountIds, -1, null, null);
	}

	public static PlayerEnterBattleLog CreateMyEnterPlayerBattleLog(IList<int> heroIndices, IList<long> accountIds, int leaderHeroIndex, IList<int> aiHeroIndices, IList<int> groupHeroIndices, ResultCreatureInfo[] userHeroInfos, BattleType battleType = BattleType.None)
	{
		TeamType teamType = TeamType.Left;
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog
		{
			PlayerCreateInfo = new BattlePlayerCreateInfo
			{
				Id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
				TeamType = teamType
			}
		};
		List<BattleCreatureCreateInfo> battleCreatureCreateInfoList = GetBattleCreatureCreateInfoList(heroIndices, accountIds, leaderHeroIndex, aiHeroIndices, groupHeroIndices);
		if (battleCreatureCreateInfoList != null)
		{
			playerEnterBattleLog.CreatureCreateInfos = battleCreatureCreateInfoList.ToArray();
			playerEnterBattleLog.InitialCreatureInfos = userHeroInfos;
		}
		if (CheckEachWaveEachTeam(battleType))
		{
			List<BattleCreatureCreateInfo> battleCreatureCreateInfoList2 = GetBattleCreatureCreateInfoList(groupHeroIndices, accountIds, leaderHeroIndex, null, null);
			if (battleCreatureCreateInfoList2 != null)
			{
				playerEnterBattleLog.BattleCheckCreatureCreateInfos = battleCreatureCreateInfoList2.ToArray();
			}
		}
		return playerEnterBattleLog;
	}

	private static List<BattleCreatureCreateInfo> GetBattleCreatureCreateInfoList(IList<int> heroIndices, IList<long> accountIds, int leaderHeroIndex, IList<int> aiHeroIndices, IList<int> groupHeroIndices)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndices))
		{
			return null;
		}
		BattleCreatureCreateInfo.BuffSkillInfo myBuffSkillInfo = GetMyBuffSkillInfo();
		BattleCreatureCreateInfo.BuffStatInfo myBuffStatInfo = GetMyBuffStatInfo();
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		list.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndices); i++)
		{
			int num = heroIndices[i];
			if (accountIds == null || i >= accountIds.Count || accountIds[i] == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				CachedHeroInfo cachedHeroInfo = JsonMarshaler.Clone(GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(num));
				cachedHeroInfo.Validate();
				bool isAi = aiHeroIndices?.Contains(num) ?? false;
				int groupIndex = ((groupHeroIndices != null && groupHeroIndices.Contains(num)) ? 1 : 0);
				BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(cachedHeroInfo, num == leaderHeroIndex, isAi, myBuffSkillInfo, myBuffStatInfo, groupIndex);
				if (battleCreatureCreateInfo != null)
				{
					list.Add(battleCreatureCreateInfo);
				}
			}
		}
		return list;
	}

	private static BattleCreatureCreateInfo[] GetForcedUserBattleCreatureCreateInfos()
	{
		EnterDungeonData enterDungeon = SingletonAuto<CampaignContext>.instance.EnterDungeon;
		if (SubStoryUtillity.IsSubStoryBattleType(enterDungeon))
		{
			List<BattleCreatureCreateInfo> battleCreatureInfos = SubStoryUtillity.GetBattleCreatureInfos(enterDungeon);
			if (__Extension_Method_ICollection__.COUNT(battleCreatureInfos) > 0)
			{
				return battleCreatureInfos.ToArray();
			}
		}
		return null;
	}

	private PlayerEnterBattleLog CreateEnterPlayerBattleLog_Me()
	{
		PlayerEnterBattleLog playerEnterBattleLog = CreateMyEnterPlayerBattleLog(HeroIndices, AccountIds, LeaderHeroIndex, AiHeroIndices, GroupHeroIndices, _userHeroInfos, GetBattleType());
		SetForcedUserBattleCreatureCreate(playerEnterBattleLog);
		return playerEnterBattleLog;
	}

	private void SetForcedUserBattleCreatureCreate(PlayerEnterBattleLog log)
	{
		if (log != null && __Extension_Method_ICollection__.COUNT(_forcedUserBattleCreatureCreateInfos) > 0)
		{
			log.CreatureCreateInfos = _forcedUserBattleCreatureCreateInfos;
		}
	}

	private PlayerEnterBattleLog CreateEnterPlayerBattleLog_Enemy()
	{
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
		playerEnterBattleLog.PlayerCreateInfo = new BattlePlayerCreateInfo();
		playerEnterBattleLog.PlayerCreateInfo.Id = 0L;
		playerEnterBattleLog.PlayerCreateInfo.TeamType = TeamType.Right;
		playerEnterBattleLog.InitialCreatureInfos = _enemyHeroInfos;
		if (_forcedEnemyHeroInfos != null && _forcedEnemyHeroInfos.Length != 0)
		{
			List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
			System.Random random = new System.Random(DateTime.UtcNow.Millisecond);
			for (int i = 0; i < _forcedEnemyHeroInfos.Length; i++)
			{
				CachedHeroInfo cachedHeroInfo = _forcedEnemyHeroInfos[i];
				BattleCreatureCreateInfo battleCreatureCreateInfo = new BattleCreatureCreateInfo();
				battleCreatureCreateInfo.IsAi = true;
				battleCreatureCreateInfo.HeroInfo = cachedHeroInfo;
				battleCreatureCreateInfo.StatData = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(cachedHeroInfo);
				battleCreatureCreateInfo.RandomSeed = random.Next();
				list.Add(battleCreatureCreateInfo);
			}
			playerEnterBattleLog.CreatureCreateInfos = list.ToArray();
		}
		return playerEnterBattleLog;
	}

	public void ForceBattleCancel()
	{
		BattleCancelNot battleCancelNot = new BattleCancelNot();
		battleCancelNot.Reason = BattleCancelNot.ReasonType.WaveNotStart;
		OnBattleCancel(battleCancelNot);
	}

	public void ForceBattleCancel(BattleCancelNot packet)
	{
		OnBattleCancel(packet);
	}

	public static void SprkAddPartySurvivor(List<int> P_0, int P_1, int[] P_2, int[] P_3)
	{
		if (((P_2 != null && Array.IndexOf(P_2, P_1) >= 0) || (P_3 != null && Array.IndexOf(P_3, P_1) >= 0)) && !P_0.Contains(P_1))
		{
			P_0.Add(P_1);
		}
	}
}
