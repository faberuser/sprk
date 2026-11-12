using System;
using System.Collections.Generic;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NBattleContext;

public class CampaignReplayContext : SingletonAuto<CampaignReplayContext>, RePlay.IDataProvider, IDataProvider
{
	private CampaignDungeonData _dungeonData;

	public int ChapterIndex { get; private set; }

	public int DungeonIndex { get; private set; }

	public NShared.DungeonDifficultyType Difficulty { get; private set; }

	public BaseBattleLog[] BattleLogs { get; private set; }

	public void Init(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, BaseBattleLog[] battleLogs)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Difficulty = difficulty;
		BattleLogs = battleLogs;
		_dungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
	}

	public CampaignDungeonData GetCampaignDungeonData()
	{
		if (_dungeonData == null)
		{
			return null;
		}
		return _dungeonData;
	}

	public string GetFieldName()
	{
		return (_dungeonData != null) ? _dungeonData.BattleField : null;
	}

	public static void Preload(BattleScenePreloader preloader, int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, string fieldName, BaseBattleLog[] battleLogs)
	{
		preloader.EnterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, difficulty);
		preloader.PreloadDungeonMonster = chapterIndex != 0 && dungeonIndex != 0;
		preloader.FieldName = fieldName;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(battleLogs); i++)
		{
			BaseBattleLog baseBattleLog = battleLogs[i];
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

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		return Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetBattleDefine()?.BattleMenu ?? (EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto);
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		Preload(preloader, ChapterIndex, DungeonIndex, Difficulty, GetFieldName(), BattleLogs);
	}

	bool IDataProvider.CanNextDungeon()
	{
		return false;
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, Difficulty);
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
		return SceneAutoBehaviour<RePlay>.instance;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return BattleType.Campaign;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex)?.GetBattleDefine(isScenario: false);
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
					new List<CachedHeroInfo>();
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

	string IDataProvider.GetPreloadFieldName()
	{
		return GetFieldName();
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		if (ChapterIndex != 0)
		{
			return DungeonIndex != 0;
		}
		return false;
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		return null;
	}

	int RePlay.IDataProvider.GetChapterIndex()
	{
		return ChapterIndex;
	}

	int RePlay.IDataProvider.GetDungeonIndex()
	{
		return DungeonIndex;
	}

	NShared.DungeonDifficultyType RePlay.IDataProvider.GetDifficulty()
	{
		return Difficulty;
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
		SingletonAuto<BattleContext>.instance.SetCampaignReplay(ChapterIndex, DungeonIndex, Difficulty, BattleLogs);
		SingletonAuto<BattleContext>.instance.MoveToBattleScene();
	}

	private void OnExit()
	{
		SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
	}
}
