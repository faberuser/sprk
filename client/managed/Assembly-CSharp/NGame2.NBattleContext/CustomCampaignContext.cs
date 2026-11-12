using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NScene.NPreloader;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleContext;

public class CustomCampaignContext : SingletonAuto<CustomCampaignContext>, OfflinePlay.IDataProvider, IDataProvider
{
	private PlayerEnterBattleLog _enterPlayerBattleLog;

	private Action<TeamType> _onBattleEnd;

	public int ChapterIndex { get; private set; }

	public int DungeonIndex { get; private set; }

	public DungeonDifficultyType DifficultType { get; private set; }

	public void Init(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficultType)
	{
		SprkArenaPractice.Clear();
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		DifficultType = difficultType;
	}

	public void SetEnterPlayerBattleLog(PlayerEnterBattleLog enterPlayerBattleLog)
	{
		_enterPlayerBattleLog = enterPlayerBattleLog;
	}

	public void SetBattleEndCallback(Action<TeamType> onBattleEnd)
	{
		_onBattleEnd = onBattleEnd;
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		preloader.EnterDungeon = new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, DungeonDifficultyType.Easy);
		preloader.PreloadDungeonMonster = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetPrealodCreateMonster();
		bool loadCreatureImage = _enterPlayerBattleLog.PlayerCreateInfo.Id == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		BattleCreatureCreateInfo[] creatureCreateInfos = _enterPlayerBattleLog.CreatureCreateInfos;
		foreach (BattleCreatureCreateInfo battleCreatureCreateInfo in creatureCreateInfos)
		{
			preloader.AddCreature(battleCreatureCreateInfo.HeroInfo, battleCreatureCreateInfo.AddExtraSkill, loadCreatureImage);
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
		return SceneAutoBehaviour<OfflinePlay>.instance;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return BattleType.CustomCampaign;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data == null)
		{
			return SprkArenaPractice.DisplayDefine(null);
		}
		return SprkArenaPractice.DisplayDefine(data.GetBattleDefine(isScenario: false));
	}

	BattleFacade.BattleConditionBase IDataProvider.CreateBattleCondition()
	{
		return new CampaignCondition();
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
		if (_enterPlayerBattleLog == null)
		{
			return null;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		BattleCreatureCreateInfo[] creatureCreateInfos = _enterPlayerBattleLog.CreatureCreateInfos;
		foreach (BattleCreatureCreateInfo battleCreatureCreateInfo in creatureCreateInfos)
		{
			list.Add(battleCreatureCreateInfo.HeroInfo);
		}
		return list;
	}

	List<IBattleLogRuntime> OfflinePlay.IDataProvider.CreateInitialBattleLog()
	{
		return new List<IBattleLogRuntime>
		{
			_enterPlayerBattleLog,
			CreateEnterPlayerBattleLog_Enemy()
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
		return DifficultType;
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

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data == null)
		{
			return SprkArenaPractice.Menu(EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto);
		}
		BattleDefineData battleDefine = data.GetBattleDefine(isScenario: false);
		if (battleDefine == null)
		{
			return SprkArenaPractice.Menu(EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto);
		}
		return SprkArenaPractice.Menu(battleDefine.BattleMenu);
	}

	void OfflinePlay.IDataProvider.HandleBattleBegin()
	{
	}

	void OfflinePlay.IDataProvider.HandleBattleEnd(TeamType winTeamType, BattleInstance battleInstance)
	{
		if (_onBattleEnd != null)
		{
			_onBattleEnd(winTeamType);
		}
	}

	List<List<BattleCreatureCreateInfo>> OfflinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return null;
	}

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return true;
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		return null;
	}

	BattleInstanceExtraData OfflinePlay.IDataProvider.GetExtraData()
	{
		return null;
	}

	private PlayerEnterBattleLog CreateEnterPlayerBattleLog_Enemy()
	{
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
		playerEnterBattleLog.PlayerCreateInfo = new BattlePlayerCreateInfo();
		playerEnterBattleLog.PlayerCreateInfo.Id = 0L;
		playerEnterBattleLog.PlayerCreateInfo.TeamType = TeamType.Right;
		return playerEnterBattleLog;
	}
}
