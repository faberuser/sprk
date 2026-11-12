using System;
using NGame2.NBattleContext;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.BeginCampaign;
using NShared.NLocalization;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.NLobby;

public class WorldBossManagement : BaseManagement<WorldBossManagement>, WorldBossGlobalRanking.Listener, BattlePartySetting.Listener, WorldBossKillRewardPopup.Listener
{
	private float _battleRequestedTime;

	private float _lastRequesetWorldBossTime;

	public static bool WorldBossKilledByMe;

	public static PolledRewardInfo WorldBossKilledRewardInfo;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<WorldBossGlobalRanking>("Resources/Prefab2/Window/WorldBoss/WorldBossGlobalRanking");
		AddInstance<WorldBossGlobalRewardInfo>("Resources/Prefab2/Window/WorldBoss/WorldBossGlobalRewardInfo");
		AddInstance<WorldBossGlobalGuide>("Resources/Prefab2/Window/WorldBoss/WorldBossGlobalGuide");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<WorldBossProfile>("Resources/Prefab2/Window/WorldBoss/WorldBossProfile");
		AddInstance<WorldBossKillRewardPopup>("Resources/Prefab2/Window/WorldBoss/WorldBossKillRewardPopup");
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			WorldBossKilledByMe = false;
			WorldBossKilledRewardInfo = null;
			base.OnDestroy();
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateShowLobbyMenu();
		RefreshWorldBossInfo();
	}

	private void UpdateShowLobbyMenu()
	{
	}

	private bool WorldBossRankingWindowIsOpened()
	{
		if (SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo == null)
		{
			return false;
		}
		WorldBossGlobalRanking worldBossGlobalRanking = GetInstance<WorldBossGlobalRanking>(openedOnly: true);
		if (worldBossGlobalRanking == null || !worldBossGlobalRanking.IsOpen())
		{
			return false;
		}
		return true;
	}

	private void RefreshWorldBossInfo()
	{
		if (!(Time.time - _lastRequesetWorldBossTime < 10f))
		{
			_lastRequesetWorldBossTime = Time.time;
			if (WorldBossRankingWindowIsOpened())
			{
				SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestWorldBossInfo(showLoadingGauge: false, null);
			}
		}
	}

	private string GetWorldBossName()
	{
		WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
		if (worldBossInfo == null)
		{
			return null;
		}
		int index = worldBossInfo.Index;
		WorldBossData data = GameRoot.Instance.WorldBossDataContainer.GetData(index);
		int chapterIndex = data.ChapterIndex;
		int dungeonIndex = data.DungeonIndex;
		CampaignWaveData bossWaveData;
		CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
		if (bossCreatureData == null)
		{
			VespaDebug.LogError("WorldBoss BossCreatureData not found: chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return null;
		}
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		string visualInfo = bossWaveData?.BossVisualInfo;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		return creatureName;
	}

	public void OpenWorldBossKillRewardPopup()
	{
		string worldBossName = GetWorldBossName();
		GetInstance<WorldBossKillRewardPopup>().Open(this, WorldBossKilledRewardInfo, worldBossName);
	}

	public void OpenWorldBossRanking(Action onOpen)
	{
		SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestWorldBossInfo(showLoadingGauge: true, delegate(WorldBossInfo worldBossInfo)
		{
			if (worldBossInfo == null)
			{
				VespaDebug.LogError("WorldBossInfo is null");
				Vespa_System_Action_Extension_Method.fire(onOpen);
			}
			else
			{
				SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestCurrentRankInfo(delegate(WorldBossRankerInfo rankInfo)
				{
					if (rankInfo != null)
					{
						GetInstance<WorldBossGlobalRanking>().Open(this, worldBossInfo.Index, onOpen);
						SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.OpenContentsResultPopup(ContentsNoticeType.WorldBoss);
					}
					else
					{
						VespaDebug.LogError("Get world boss ranker failed.");
						Vespa_System_Action_Extension_Method.fire(onOpen);
					}
				});
			}
		});
	}

	private void EnterBattle(WorldBossData data)
	{
		WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
		if (worldBossInfo == null)
		{
			VespaDebug.LogError("WorldBossInfo is null");
			return;
		}
		int chapterIndex = data.ChapterIndex;
		int dungeonIndex = data.DungeonIndex;
		int num = 0;
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, (NShared.DungeonDifficultyType)num);
		enterDungeonData.InitialHeroInfos = worldBossInfo.CreateResultCreatureInfos();
		CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(staminaType: data2.ReqStaminaType, staminaCount: data2.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), (NShared.DungeonDifficultyType)num), storageKey: data2.LocalStorageKey, startButtonTextKey: Battle.START_BATTLE, enterDungeon: enterDungeonData);
		GetInstance<BattlePartySetting>().Open(this, dataProvider);
	}

	void WorldBossGlobalRanking.Listener.OnClickEnter(WorldBossData data)
	{
		CurrencyMaxCap.WarningOrExecute(CurrencyType.WorldBossPoint, delegate
		{
			EnterBattle(data);
		});
	}

	void WorldBossGlobalRanking.Listener.OnClickInfo()
	{
		WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
		if (worldBossInfo == null)
		{
			return;
		}
		int index = worldBossInfo.Index;
		WorldBossData data = GameRoot.Instance.WorldBossDataContainer.GetData(index);
		if (data != null)
		{
			int chapterIndex = data.ChapterIndex;
			int dungeonIndex = data.DungeonIndex;
			CampaignWaveData bossWaveData;
			CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy, scenario: false, out bossWaveData);
			if (bossCreatureData == null)
			{
				VespaDebug.LogError("WorldBoss BossCreatureData not found: chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			}
			else
			{
				GetInstance<WorldBossProfile>().Open(data, bossCreatureData);
			}
		}
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (!(Time.time - _battleRequestedTime < 10f))
		{
			_battleRequestedTime = Time.time;
			RequestBegin(enterDungeon, battlePartyInfo, extraOption);
		}
	}

	private void RequestBegin(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		WorldBossInfo worldBossInfo = SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.WorldBossInfo;
		if (worldBossInfo == null)
		{
			_battleRequestedTime = 0f;
			return;
		}
		int index = worldBossInfo.Index;
		NShared.BeginCampaign.Request obj = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			DungeonDifficulty = (int)enterDungeon.Difficulty,
			ScenarioDungeon = enterDungeon.IsScenario,
			HeroIndices = battlePartyInfo.heroIndices,
			AiHeroIndices = battlePartyInfo.subHeroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			WorldBossIndex = index,
			Repeat = false
		};
		int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
		obj.ItemBoosterCount = onetimeBoosterCount;
		obj.AffixIndices = battlePartyInfo.AffixIndices;
		NShared.BeginCampaign.Request request = obj;
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			LobbyScene2.ListReturnState.Add(new WorldBossReturnState(enterDungeon));
		}, delegate
		{
			_battleRequestedTime = 0f;
		});
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	void WorldBossKillRewardPopup.Listener.OnClickViewList(WorldBossKillRewardPopup caller)
	{
		caller.Close();
	}
}
