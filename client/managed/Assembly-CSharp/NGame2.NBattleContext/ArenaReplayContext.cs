using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NBattleContext;

public class ArenaReplayContext : SingletonAuto<ArenaReplayContext>, RePlay.IDataProvider, IDataProvider
{
	public ArenaReplayInfo ReplayInfo;

	public BaseBattleLog[] BattleLogs;

	public void Init(ArenaReplayInfo replayInfo, BaseBattleLog[] battleLogs)
	{
		ReplayInfo = replayInfo;
		BattleLogs = battleLogs;
	}

	private void OnEnterBattle()
	{
		SceneAutoBehaviour<ArenaManagement>.instance.Open();
		if (ReplayInfo != null)
		{
			PvpStatus.MatchUserInfo userInfo = new PvpStatus.MatchUserInfo(ReplayInfo.LeftTeam);
			PvpStatus.MatchUserInfo userInfo2 = new PvpStatus.MatchUserInfo(ReplayInfo.RightTeam);
			SceneAutoBehaviour<ArenaManagement>.instance.OpenStatus(userInfo, userInfo2, ReplayInfo.ArenaType);
		}
	}

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		return Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetBattleDefine()?.BattleMenu ?? (EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto);
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		_ = ReplayInfo.RightTeam.AccountId;
		_ = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		preloader.EnterDungeon = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetEnterDungeon();
		preloader.PreloadDungeonMonster = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetPrealodCreateMonster();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BattleLogs); i++)
		{
			BaseBattleLog baseBattleLog = BattleLogs[i];
			if (baseBattleLog == null || baseBattleLog.Type != BattleLogType.PlayerEnter)
			{
				continue;
			}
			try
			{
				PlayerEnterBattleLog playerEnterBattleLog = JsonMarshaler.Parse<PlayerEnterBattleLog>(baseBattleLog.Data);
				if (playerEnterBattleLog != null)
				{
					BattleCreatureCreateInfo[] creatureCreateInfos = playerEnterBattleLog.CreatureCreateInfos;
					foreach (BattleCreatureCreateInfo battleCreatureCreateInfo in creatureCreateInfos)
					{
						preloader.AddCreature(battleCreatureCreateInfo.HeroInfo, battleCreatureCreateInfo.AddExtraSkill, loadCreatureImage: false);
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	bool IDataProvider.CanNextDungeon()
	{
		return false;
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return new EnterDungeonData(ReplayInfo.ChapterIndex, ReplayInfo.DungeonIndex, ReplayInfo.DungeonIndex, ReplayInfo.Difficulty);
	}

	void IDataProvider.OnEnterBattle()
	{
		OnEnterBattle();
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
		return null;
	}

	IPlay IDataProvider.GetBattlePlay()
	{
		return SceneAutoBehaviour<RePlay>.instance;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return BattleType.Arena;
	}

	List<CachedHeroInfo> IDataProvider.GetPreloadBattleLogs()
	{
		if (BattleLogs == null)
		{
			return null;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BattleLogs); i++)
		{
			BaseBattleLog baseBattleLog = BattleLogs[i];
			if (baseBattleLog == null || baseBattleLog.Type != BattleLogType.PlayerEnter)
			{
				continue;
			}
			try
			{
				PlayerEnterBattleLog playerEnterBattleLog = JsonMarshaler.Parse<PlayerEnterBattleLog>(baseBattleLog.Data);
				if (playerEnterBattleLog != null)
				{
					BattleCreatureCreateInfo[] creatureCreateInfos = playerEnterBattleLog.CreatureCreateInfos;
					foreach (BattleCreatureCreateInfo battleCreatureCreateInfo in creatureCreateInfos)
					{
						list.Add(battleCreatureCreateInfo.HeroInfo);
					}
				}
			}
			catch (Exception)
			{
			}
		}
		return list;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(ReplayInfo.ChapterIndex, ReplayInfo.DungeonIndex)?.GetBattleDefine(isScenario: false);
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

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return false;
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		return null;
	}

	int RePlay.IDataProvider.GetChapterIndex()
	{
		return ReplayInfo.ChapterIndex;
	}

	int RePlay.IDataProvider.GetDungeonIndex()
	{
		return ReplayInfo.DungeonIndex;
	}

	NShared.DungeonDifficultyType RePlay.IDataProvider.GetDifficulty()
	{
		return ReplayInfo.Difficulty;
	}

	BaseBattleLog[] RePlay.IDataProvider.GetBattleLogs()
	{
		return BattleLogs;
	}

	void RePlay.IDataProvider.HandleReplayEnd()
	{
		string title = LocalizationManager.GetString(Battle.VIEW_REPLAY);
		string message = LocalizationManager.GetString(Battle.VIEW_REPLAY_CONFIRM);
		MessagePopup.Open(title, message, OnPlayAgain, OnExit);
	}

	private void OnPlayAgain()
	{
		SingletonAuto<BattleContext>.instance.SetArenaReplay(ReplayInfo, BattleLogs);
		SingletonAuto<BattleContext>.instance.MoveToBattleScene();
	}

	private void OnExit()
	{
		SingletonAuto<BattleContext>.instance.ReturnToLobby();
	}
}
