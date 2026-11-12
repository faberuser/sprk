using System.Collections.Generic;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NGuild;
using NGame2.NOrdeal;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NCampaign;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleContext;

public class OrdealArenaContext : SingletonAuto<OrdealArenaContext>, OfflinePlay.IDataProvider, IDataProvider
{
	public bool OfflineArenaResultRequested;

	private PvpStatus.MatchUserInfo LeftTeamInfo;

	public MatchedAccountInfo MyMatchInfo;

	public int ChapterIndex;

	public int DungeonIndex;

	private PvpContextImpl _impl;

	public int[] HeroIndices { get; private set; }

	public int LeaderHeroIndex { get; private set; }

	public MatchedAccountInfo MatchedNpcInfo { get; private set; }

	public long NpcAccountId
	{
		get
		{
			if (MatchedNpcInfo == null)
			{
				return 0L;
			}
			return MatchedNpcInfo.AccountId;
		}
	}

	public string NpcNick
	{
		get
		{
			if (MatchedNpcInfo == null)
			{
				return "NPC";
			}
			return MatchedNpcInfo.Nick;
		}
	}

	public int NpcLevel
	{
		get
		{
			if (MatchedNpcInfo == null)
			{
				return 0;
			}
			return MatchedNpcInfo.UserInfo.TeamLevel;
		}
	}

	public void Init(int[] heroIndices, int leaderHeroIndex, MatchedAccountInfo matchedNpcInfo, int chapterIndex, int dungeonIndex, MatchedAccountInfo myMatchInfo)
	{
		HeroIndices = heroIndices;
		LeaderHeroIndex = leaderHeroIndex;
		MatchedNpcInfo = matchedNpcInfo;
		OfflineArenaResultRequested = false;
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		int matchScore = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetMatchScore();
		MatchTierType selectedTier = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier();
		List<MatchTierData> tierDataListOfType = GameRoot.Instance.MatchTierDataContainer.GetTierDataListOfType(selectedTier);
		int tierIndex = ((!__Extension_Method_ICollection__.IsNullOrEmpty(tierDataListOfType)) ? tierDataListOfType[0].Index : 0);
		LeftTeamInfo = new PvpStatus.MatchUserInfo
		{
			AvatarIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex,
			MatchScore = matchScore,
			Rank = -1,
			TierIndex = tierIndex,
			Name = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
			TeamType = TeamType.Left,
			AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId
		};
		MyMatchInfo = myMatchInfo;
		_impl = new PvpContextImpl();
	}

	private void ReturnToLobby()
	{
		SingletonAuto<BattleContext>.instance.ReturnToLobby();
	}

	private void OnEnterBattle()
	{
		SceneAutoBehaviour<OrdealArenaManagement>.instance.Open();
		PvpStatus.MatchUserInfo matchUserInfo = new PvpStatus.MatchUserInfo(MatchedNpcInfo);
		matchUserInfo.TeamType = TeamType.Right;
		SceneAutoBehaviour<OrdealArenaManagement>.instance.OpenStatus(LeftTeamInfo, matchUserInfo);
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		preloader.EnterDungeon = new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, DungeonDifficultyType.Easy);
		preloader.PreloadDungeonMonster = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetPrealodCreateMonster();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]);
			if (cachedHeroInfo != null)
			{
				preloader.AddCreature(cachedHeroInfo, null, loadCreatureImage: true);
			}
		}
		if (MatchedNpcInfo == null)
		{
			return;
		}
		foreach (CachedHeroInfo value in MatchedNpcInfo.HeroInfos.Values)
		{
			preloader.AddCreature(value, null, loadCreatureImage: false);
		}
	}

	bool IDataProvider.CanNextDungeon()
	{
		return false;
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, DungeonDifficultyType.Easy);
	}

	void IDataProvider.OnEnterBattle()
	{
		OnEnterBattle();
	}

	void IDataProvider.OnWaveMoveEnd()
	{
		_impl.StartCountdown();
	}

	bool IDataProvider.CanStartBattle()
	{
		return _impl.CheckCountdownEnd();
	}

	BattleStartEffectData IDataProvider.GetBattleStartEffectData()
	{
		return _impl.GetBattleStartEffectData();
	}

	IPlay IDataProvider.GetBattlePlay()
	{
		return SceneAutoBehaviour<OfflinePlay>.instance;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return BattleType.OrdealArena;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex)?.GetBattleDefine(isScenario: false);
	}

	BattleFacade.BattleConditionBase IDataProvider.CreateBattleCondition()
	{
		return new ArenaCondition();
	}

	bool IDataProvider.IsOnline()
	{
		return false;
	}

	void IDataProvider.OnUpdate()
	{
	}

	bool IDataProvider.IsTowerBattle()
	{
		return false;
	}

	int IDataProvider.GetTowerFloor()
	{
		return 0;
	}

	List<CachedHeroInfo> IDataProvider.GetPreloadBattleLogs()
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]);
			if (cachedHeroInfo != null)
			{
				list.Add(cachedHeroInfo);
			}
		}
		if (MatchedNpcInfo != null)
		{
			foreach (CachedHeroInfo value in MatchedNpcInfo.HeroInfos.Values)
			{
				list.Add(value);
			}
		}
		return list;
	}

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return true;
	}

	List<IBattleLogRuntime> OfflinePlay.IDataProvider.CreateInitialBattleLog()
	{
		return new List<IBattleLogRuntime>
		{
			CreateEnterPlayerBattleLog_Me(),
			CreateEnterPlayerBattleLog_Npc()
		};
	}

	int OfflinePlay.IDataProvider.GetChapterIndex()
	{
		return ChapterIndex;
	}

	int OfflinePlay.IDataProvider.GetDungeonIndex()
	{
		return DungeonIndex;
	}

	DungeonDifficultyType OfflinePlay.IDataProvider.GetDifficulty()
	{
		return DungeonDifficultyType.Easy;
	}

	bool OfflinePlay.IDataProvider.IsScenarioDungeon()
	{
		return false;
	}

	int OfflinePlay.IDataProvider.GetRequestDungeonIndex()
	{
		return DungeonIndex;
	}

	OfflinePlay.ResponseData OfflinePlay.IDataProvider.GetResponseData()
	{
		return null;
	}

	int OfflinePlay.IDataProvider.GetWorldMapEventIndex()
	{
		return 0;
	}

	List<List<BattleCreatureCreateInfo>> OfflinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return null;
	}

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data == null)
		{
			return EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto;
		}
		return data.GetBattleDefine(isScenario: false)?.BattleMenu ?? (EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto);
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data == null)
		{
			return null;
		}
		MyGuildInfo info = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info;
		GuildArenaRankerInfo enemyGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo();
		return new GuildFlagData
		{
			EGuildFlagDisplayType = data.EGuildFlagDisplayType,
			LeftGuildFlag = (info?.Flag ?? 1),
			RightGuildFlag = (enemyGuildInfo?.Flag ?? 1),
			LeftGuildLogo = (info?.Logo ?? 1),
			RightGuildLogo = (enemyGuildInfo?.Logo ?? 1),
			LeftGuildFlagBG = (info?.Back ?? 1),
			RightGuildFlagBG = (enemyGuildInfo?.Back ?? 1),
			ExistLeftGuild = (info != null),
			ExistRightGuild = (enemyGuildInfo != null)
		};
	}

	void OfflinePlay.IDataProvider.HandleBattleBegin()
	{
	}

	void OfflinePlay.IDataProvider.HandleBattleEnd(TeamType winTeamType, BattleInstance battleInstance)
	{
		BattleEnd(winTeamType, battleInstance);
	}

	BattleInstanceExtraData OfflinePlay.IDataProvider.GetExtraData()
	{
		return null;
	}

	private PlayerEnterBattleLog CreateEnterPlayerBattleLog_Me()
	{
		return CampaignContext.CreateMyEnterPlayerBattleLog(HeroIndices, null, LeaderHeroIndex, null, null, null);
	}

	private PlayerEnterBattleLog CreateEnterPlayerBattleLog_Npc()
	{
		return CampaignUpdater.CreateNpcEnterBattleLog(NpcAccountId, MatchedNpcInfo);
	}

	protected void BattleEnd(TeamType winTeamType, BattleInstance battleInstance)
	{
		TeamType teamType = TeamType.Left;
		bool flag = winTeamType == teamType;
		if (flag)
		{
			battleInstance.CreatureContainer.GetAliveCreatureIndices(teamType);
		}
		int pureBattleTime = (int)(flag ? (battleInstance.GetElapsedPureBattleTimeMs() / 1000) : 0);
		int num = (flag ? battleInstance.GetRemainBattleTimeMs(includeExtraTime: false) : 0);
		string battleLogString = (flag ? ReplayUtil.CreateBattleLogString(battleInstance, JsonMarshaler.EncodeJsonString, compress: true) : null);
		string creatureInfoString = "";
		if (flag)
		{
			List<ResultCreatureInfo> creatureInfoList = BattleInstanceIntegrityChecker.GetCreatureInfoList(battleInstance);
			if (creatureInfoList != null)
			{
				creatureInfoString = Json.Serialize(BaseJsonMarshaler.EncodeArray(creatureInfoList, JsonMarshaler.EncodeJsonObject));
			}
		}
		GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestEndOrdeaArena(ChapterIndex, DungeonIndex, HeroIndices, flag, battleLogString, creatureInfoString, pureBattleTime, num);
	}
}
