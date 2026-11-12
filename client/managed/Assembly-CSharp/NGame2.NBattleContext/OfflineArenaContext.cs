using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleField.NReplay;
using NGame2.NContentsInfo;
using NGame2.NGuild;
using NGame2.NMatch;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NCampaign;
using NShared.NLocalization;
using NShared.SetOfflineMatchResult;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NBattleContext;

public class OfflineArenaContext : SingletonAuto<OfflineArenaContext>, OfflinePlay.IDataProvider, IDataProvider
{
	public bool OfflineArenaResultRequested;

	private PvpStatus.MatchUserInfo LeftTeamInfo;

	public ReplayTeamInfo MyTeamInfo;

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

	public long MyAccountId
	{
		get
		{
			if (MyMatchInfo == null)
			{
				return 0L;
			}
			return MyMatchInfo.AccountId;
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
		ArenaBattleInfo arenaBattleInfo = SingletonAuto<MatchArena>.instance.GetArenaBattleInfo(MatchPvpType.Normal);
		RankResultInfo rankResultInfo = SingletonAuto<MatchArena>.instance.GetRankResultInfo(MatchPvpType.Normal);
		LeftTeamInfo = new PvpStatus.MatchUserInfo
		{
			AvatarIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex,
			MatchScore = arenaBattleInfo.MatchScore,
			Rank = (rankResultInfo?.TotalRank ?? (-1)),
			TierIndex = arenaBattleInfo.WorldTierIndex,
			Name = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
			TeamType = TeamType.Left,
			AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId
		};
		MyTeamInfo = new ReplayTeamInfo
		{
			AvaterIndex = LeftTeamInfo.AvatarIndex,
			MatchScore = LeftTeamInfo.MatchScore,
			TierRank = LeftTeamInfo.Rank,
			TeamType = LeftTeamInfo.TeamType,
			NickName = LeftTeamInfo.Name,
			TierIndex = LeftTeamInfo.WorldTierIndex,
			AccountId = LeftTeamInfo.AccountId
		};
		List<ReplayTeamHeroInfo> list = new List<ReplayTeamHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]);
			ReplayTeamHeroInfo item = new ReplayTeamHeroInfo
			{
				Index = cachedHeroInfo.HeroIndex,
				Star = cachedHeroInfo.Star
			};
			list.Add(item);
		}
		MyMatchInfo = myMatchInfo;
		MyTeamInfo.HeroInfos = list.ToArray();
		_impl = new PvpContextImpl();
	}

	public ReplayTeamHeroInfo[] GetMatchNpcReplayHeroInfos()
	{
		List<ReplayTeamHeroInfo> list = new List<ReplayTeamHeroInfo>();
		foreach (CachedHeroInfo value in MatchedNpcInfo.HeroInfos.Values)
		{
			list.Add(new ReplayTeamHeroInfo
			{
				Index = value.HeroIndex,
				Star = value.Star
			});
		}
		return list.ToArray();
	}

	private void ReturnToLobby()
	{
		SingletonAuto<BattleContext>.instance.ReturnToLobby();
	}

	private void OnEnterBattle()
	{
		SceneAutoBehaviour<ArenaManagement>.instance.Open();
		PvpStatus.MatchUserInfo matchUserInfo = new PvpStatus.MatchUserInfo(MatchedNpcInfo);
		matchUserInfo.TeamType = TeamType.Right;
		MatchArenaType arenaType = MatchArenaType.Normal;
		if (MatchedNpcInfo.ArenaType == MatchArenaType.GuildArena)
		{
			arenaType = MatchArenaType.GuildArena;
		}
		SceneAutoBehaviour<ArenaManagement>.instance.OpenStatus(LeftTeamInfo, matchUserInfo, arenaType);
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		preloader.EnterDungeon = new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, NShared.DungeonDifficultyType.Easy);
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
		return new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, NShared.DungeonDifficultyType.Easy);
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
		return BattleType.Arena;
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

	NShared.DungeonDifficultyType OfflinePlay.IDataProvider.GetDifficulty()
	{
		return NShared.DungeonDifficultyType.Easy;
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

	protected virtual void BattleEnd(TeamType winTeamType, BattleInstance battleInstance)
	{
		if (OfflineArenaResultRequested)
		{
			return;
		}
		OfflineArenaResultRequested = true;
		TeamType teamType = TeamType.Left;
		TeamType winner = winTeamType;
		bool win = winner == teamType;
		int[] aliveCreatureIndices = battleInstance.CreatureContainer.GetAliveCreatureIndices(teamType);
		int playTime = (int)(battleInstance.GlobalTimer.GetCurrentTimeMs() / 1000);
		List<ResultCreatureInfo> creatureInfos = (win ? BattleInstanceIntegrityChecker.GetCreatureInfoList(battleInstance) : null);
		string battleLogString = (win ? ReplayUtil.CreateBattleLogString(battleInstance, JsonMarshaler.EncodeJsonString, compress: true) : null);
		if (SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.GuildArena)
		{
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestSetGuildArenaMatchResult(win, playTime, aliveCreatureIndices, battleLogString, creatureInfos, null);
			return;
		}
		SingletonUpdater<MatchManager>.instance.RequestOfflineArenaResult(win, playTime, aliveCreatureIndices, battleLogString, creatureInfos, delegate(NShared.SetOfflineMatchResult.Response response)
		{
			if (response == null || response.MatchResult == null)
			{
				MessagePopup.OpenError(LocalizationManager.GetString(Match.MATCH_OFFLINE_PVP_RESULT_FAILED), ReturnToLobby);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult, ReturnToLobby);
			}
			else if (response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result, ReturnToLobby);
			}
			else
			{
				MatchResultInfo matchResult = response.MatchResult;
				SceneAutoBehaviour<ArenaManagement>.instance.OpenPvpResult_Normal(matchResult);
				List<ReplayTeamHeroInfo> list = new List<ReplayTeamHeroInfo>();
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(HeroIndices); i++)
				{
					CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(HeroIndices[i]);
					if (cachedHeroInfo != null)
					{
						list.Add(new ReplayTeamHeroInfo
						{
							Index = cachedHeroInfo.HeroIndex,
							Star = cachedHeroInfo.Star
						});
					}
				}
				MatchPvpType pvpType = matchResult.GetPvpType();
				ArenaBattleInfo arenaBattleInfo = SingletonAuto<MatchArena>.instance.GetArenaBattleInfo(pvpType);
				int totalRank = SingletonAuto<MatchArena>.instance.GetRankResultInfo(pvpType).TotalRank;
				ArenaReplayInfo arenaReplayInfo = new ArenaReplayInfo();
				arenaReplayInfo.LeftTeam = new ReplayTeamInfo();
				arenaReplayInfo.LeftTeam.AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
				arenaReplayInfo.LeftTeam.NickName = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick;
				arenaReplayInfo.LeftTeam.Level = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel;
				arenaReplayInfo.LeftTeam.HeroInfos = list.ToArray();
				arenaReplayInfo.LeftTeam.AvaterIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.AvatarHeroIndex;
				arenaReplayInfo.LeftTeam.MatchScore = arenaBattleInfo.MatchScore;
				arenaReplayInfo.LeftTeam.TierIndex = arenaBattleInfo.WorldTierIndex;
				arenaReplayInfo.LeftTeam.TierRank = totalRank;
				arenaReplayInfo.LeftTeam.TeamType = TeamType.Left;
				arenaReplayInfo.RightTeam = new ReplayTeamInfo();
				arenaReplayInfo.RightTeam.AccountId = NpcAccountId;
				arenaReplayInfo.RightTeam.NickName = NpcNick;
				arenaReplayInfo.RightTeam.Level = NpcLevel;
				arenaReplayInfo.RightTeam.HeroInfos = GetMatchNpcReplayHeroInfos();
				arenaReplayInfo.RightTeam.AvaterIndex = MatchedNpcInfo.AvatarHeroIndex;
				arenaReplayInfo.RightTeam.MatchScore = MatchedNpcInfo.BattleInfo.MatchScore;
				arenaReplayInfo.RightTeam.TierIndex = MatchedNpcInfo.TierIndex;
				arenaReplayInfo.RightTeam.TierRank = totalRank;
				arenaReplayInfo.RightTeam.TeamType = TeamType.Right;
				arenaReplayInfo.WinTeam = winner;
				arenaReplayInfo.PlayTimeMs = (int)battleInstance.GlobalTimer.GetCurrentTimeMs();
				arenaReplayInfo.RecordedTime = Vespa_DateTime_Extension_Method.ToNetworkString2(NGame2.NUI.NWindow.Common.GetUTCNow());
				arenaReplayInfo.WinMatchScore = (win ? matchResult.GainedMatchScore : 0);
				arenaReplayInfo.LoseMatchScore = ((!win) ? (-matchResult.GainedMatchScore) : 0);
				arenaReplayInfo.ChapterIndex = ChapterIndex;
				arenaReplayInfo.DungeonIndex = DungeonIndex;
				ReplayType replayType = pvpType.ToReplayType();
				SingletonAuto<ReplayManager>.instance.RequestSaveReplay(battleInstance, replayType, arenaReplayInfo, new long[1] { GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id }, null);
			}
		});
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
}
