using System.Collections.Generic;
using NGame2.NBattleField;
using NShared;
using NShared.NCampaign;
using NShared.NCampaign.Karma;
using NVespa;

namespace NGame2.NBattleThread;

public class ThreadContext
{
	public BattleFacade.CampaignCacheData CampaignCache;

	public BattleFacade.BattleCacheData BattleCache = new BattleFacade.BattleCacheData();

	public BattleFacade.EclipseMessageCacheData EclipseMessageCache;

	public BattleFacade.EclipseResultCacheData EclipseResultCache;

	public BattleFacade.KarmaCacheData KarmaCache;

	public IncreaseDic<long, BattleFacade.PlayerCacheData> PlayerDic = new IncreaseDic<long, BattleFacade.PlayerCacheData>();

	public IncreaseDic<int, BattleFacade.CreatureCacheData> CreatureDic = new IncreaseDic<int, BattleFacade.CreatureCacheData>();

	public IncreaseDic<TeamType, BattleFacade.CreatureCacheData> EnvironmentCreatureDic = new IncreaseDic<TeamType, BattleFacade.CreatureCacheData>();

	public IncreaseDic<TeamType, int> KillTeamMeterDic = new IncreaseDic<TeamType, int>();

	public IncreaseDic<TeamType, int> CurrentWaveTeamKillMeterDic = new IncreaseDic<TeamType, int>();

	public IncreaseDic<TeamType, int> TotalKillTeamMeterDic = new IncreaseDic<TeamType, int>();

	public Queue<BaseBattleCallback> BattleCallbackQueue = new Queue<BaseBattleCallback>();

	public void AssignCampaignCache(CampaignUpdater campaignUpdater, bool isWaveReady, bool isWaveEnd, bool isWavePause)
	{
		CampaignCache = new BattleFacade.CampaignCacheData();
		CampaignCache.IsBattleEnd = campaignUpdater.IsBattleEnd();
		CampaignCache.ChapterIndex = campaignUpdater.ChapterIndex;
		CampaignCache.DungeonIndex = campaignUpdater.DungeonIndex;
		CampaignCache.Difficulty = (int)campaignUpdater.Difficulty;
		CampaignCache.WaveIndex = campaignUpdater.WaveIndex;
		CampaignCache.TotalWaveIndex = campaignUpdater.TotalWaveIndex;
		CampaignCache.Scenario = campaignUpdater.Scenario;
		CampaignCache.IsWaveReady = isWaveReady;
		CampaignCache.IsWaveEnd = isWaveEnd;
		CampaignCache.IsWavePause = isWavePause;
		CampaignCache.Winner = campaignUpdater.GetWinner();
	}

	public void AssignEclipseCache(EclipseOfflineModeUpdater eclipseUpdater)
	{
		if (eclipseUpdater.Get_ContinuousBattleChanged())
		{
			BattleFacade.EclipseMessageCacheData eclipseCache = GetEclipseCache();
			eclipseCache.IsContinuousBattleChanged = eclipseUpdater.Get_ContinuousBattleChanged();
			eclipseCache.IsContinuousBattle = eclipseUpdater.Get_ContinuousBattle();
		}
		if (eclipseUpdater.Get_EclipseStatusChanged())
		{
			BattleFacade.EclipseMessageCacheData eclipseCache2 = GetEclipseCache();
			eclipseCache2.SendEclipseStatusChanged = eclipseUpdater.Get_EclipseStatusChanged();
			eclipseCache2.SendEclipseStatus = eclipseUpdater.Get_EclipseStatus();
			eclipseUpdater.Init_EclipseStatus();
			EclipseResultCache = new BattleFacade.EclipseResultCacheData();
			EclipseResultCache.PlayTime = eclipseUpdater.ResultData_PlayTime;
			EclipseResultCache.TotalDamage = eclipseUpdater.ResultData_TotalDamage;
			EclipseResultCache.ChapterIndex = eclipseUpdater.ResultData_ChapterIndex;
			EclipseResultCache.DungeonIndex = eclipseUpdater.ResultData_DungeonIndex;
			EclipseResultCache.WaveIndex = eclipseUpdater.ResultData_WaveIndex;
			EclipseResultCache.MaxWaveIndex = eclipseUpdater.ResultData_MaxWaveIndex;
			EclipseResultCache.CreatureLevel = eclipseUpdater.ResultData_CreatureLevel;
			EclipseResultCache.DeckIndex = eclipseUpdater.ResultData_DeckIndex;
			if (eclipseCache2.SendEclipseStatus == EclipseStatus.BattleContinue || eclipseCache2.SendEclipseStatus == EclipseStatus.BattleTeamEndOn || eclipseCache2.SendEclipseStatus == EclipseStatus.BattleTeamEndOff || eclipseCache2.SendEclipseStatus == EclipseStatus.BattleOut || eclipseCache2.SendEclipseStatus == EclipseStatus.BattleEnd)
			{
				EclipseResultCache.BattleLogString = eclipseUpdater.BattleLogString;
				EclipseResultCache.CreatureInfoString = eclipseUpdater.CreatureInfoString;
			}
		}
	}

	public void AssignKarmaCache(KarmaUpdater karmaUpdater)
	{
		KarmaCache = new BattleFacade.KarmaCacheData();
		KarmaCache.ClearWaveCount = karmaUpdater.MaxWaveIndex;
		KarmaCache.ClearLevel = karmaUpdater.MaxStageLevel;
		AddCurrentWaveTeamKillCache(karmaUpdater.BattleInstance);
		AddTotalKillTeamCache(karmaUpdater.BattleInstance);
		int currentWaveKillMeter = karmaUpdater.BattleInstance.Snapshot.StatisticsManager.GetCurrentWaveKillMeter(TeamType.Left);
		KarmaCache.CurrentWaveKillCount = currentWaveKillMeter;
		int totalKillTeamMeter = karmaUpdater.BattleInstance.Snapshot.StatisticsManager.GetTotalKillTeamMeter(TeamType.Left);
		KarmaCache.TotalKillCount = totalKillTeamMeter;
	}

	public BattleFacade.EclipseMessageCacheData GetEclipseCache()
	{
		if (EclipseMessageCache == null)
		{
			EclipseMessageCache = new BattleFacade.EclipseMessageCacheData();
		}
		return EclipseMessageCache;
	}

	public void AssignCache(BattleInstance battleInstance)
	{
		if (battleInstance != null)
		{
			AssignBattleCache(battleInstance);
			long[] allPlayerIds = battleInstance.PlayerContainer.GetAllPlayerIds();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allPlayerIds); i++)
			{
				long playerId = allPlayerIds[i];
				BattlePlayer player = battleInstance.PlayerContainer.GetPlayer(playerId);
				AddPlayerCache(player);
			}
			for (int j = 0; j < battleInstance.CreatureContainer.GetCreatureCount(); j++)
			{
				BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(j);
				AddCreatureCache(battleInstance, creatureByIndex);
			}
			AddEnvironmentCreatureCache(battleInstance);
			AddKillTeamCache(battleInstance);
		}
	}

	private void AssignBattleCache(BattleInstance battleInstance)
	{
		BattleCache.IsBattleStarted = battleInstance.IsBattleStarted();
		BattleCache.IsBattleEnded = battleInstance.IsBattleEnded();
		BattleCache.IsBattleEnabled = battleInstance.IsBattleEnabled();
		BattleCache.ChapterIndex = battleInstance.Snapshot.ChapterIndex;
		BattleCache.DungeonIndex = battleInstance.Snapshot.DungeonIndex;
		BattleCache.WaveIndex = battleInstance.Snapshot.WaveIndex;
		BattleCache.TotalWaveIndex = battleInstance.Snapshot.TotalWaveIndex;
		BattleCache.MaxWaveIndex = battleInstance.Snapshot.MaxWaveIndex;
		BattleCache.InfiniteBattleTime = battleInstance.InfiniteBattleTime;
		BattleCache.MaxBattleTime = battleInstance.MaxBattleTime;
		BattleCache.ElapsedBattleTimeMs = battleInstance.GetElapsedBattleTimeMs();
		BattleCache.RemainedBattleTimeMs = battleInstance.GetRemainBattleTimeMs(includeExtraTime: false);
		BattleCache.RemainedBattleTimeMsIncludeExtraTime = battleInstance.GetRemainBattleTimeMs(includeExtraTime: true);
		BattleCache.GlobalCurrentTimeMs = battleInstance.GlobalTimer.GetCurrentTimeMs();
		BattleCache.MainCurrentTimeMs = battleInstance.MainTimer.GetCurrentTimeMs();
		BattleCache.IsInSoloTime = battleInstance.IsInSoloTime();
		BattleCache.BattleArea = battleInstance.BattleArea;
		BattleCache.BattleType = battleInstance.DungeonData.BattleType;
	}

	private void AddPlayerCache(BattlePlayer battlePlayer)
	{
		if (battlePlayer != null && !PlayerDic.ContainsKey(battlePlayer.Id))
		{
			BattleFacade.PlayerCacheData playerCacheData = new BattleFacade.PlayerCacheData();
			playerCacheData.Id = battlePlayer.Id;
			playerCacheData.TeamType = battlePlayer.TeamType;
			if (playerCacheData.IsMe)
			{
				BattleFacade.PlayerCacheData.MyTeamType = battlePlayer.TeamType;
			}
			PlayerDic.Add(playerCacheData.Id, playerCacheData);
		}
	}

	private void AddCreatureCache(BattleInstance battleInstance, BattleCreature battleCreature)
	{
		if (battleCreature == null || CreatureDic.ContainsKey(battleCreature.Uid))
		{
			return;
		}
		BattleFacade.CreatureCacheData creatureCacheData = new BattleFacade.CreatureCacheData(battleCreature.Uid);
		creatureCacheData.CreatureData = battleCreature.CreatureData;
		creatureCacheData.CreateInfo = battleCreature.CreateInfo;
		creatureCacheData.HeroInfo = battleCreature.CreateInfo.HeroInfo;
		creatureCacheData.CreatedTime = battleCreature.CreatedTime;
		long playerId = battleCreature.PlayerId;
		creatureCacheData.Player = PlayerDic.GETVALUE(playerId, BattleFacade.PlayerCacheData.NullCache);
		creatureCacheData.TeamType = battleCreature.TeamController.TeamType;
		creatureCacheData.Position = battleCreature.TransformController.Position;
		creatureCacheData.Direction = battleCreature.TransformController.Direction;
		creatureCacheData.IsDead = battleCreature.StatController.IsDead();
		creatureCacheData.IsResting = battleCreature.IsResting;
		creatureCacheData.Hp = battleCreature.StatController.Hp;
		creatureCacheData.MaxHp = battleCreature.StatController.GetFinalStat(StatType.MaxHp);
		creatureCacheData.HpRatioM = battleCreature.StatController.HpRatioM;
		creatureCacheData.Mp = battleCreature.StatController.Mp;
		creatureCacheData.MpRatioM = battleCreature.StatController.MpRatioM;
		creatureCacheData.MaxMp = battleCreature.StatController.GetFinalStat(StatType.MaxMp);
		creatureCacheData.MoveSpeedMms = battleCreature.TransformController.GetSpeedMms();
		creatureCacheData.AttackSpeed = battleCreature.StatController.MultiplyAttackSpeed(1000);
		creatureCacheData.AutoAiSkillIndex = battleCreature.BehaviorController.GetAutoAiSkillIndex();
		creatureCacheData.GroupIndex = battleCreature.GroupIndex;
		SkillController skillController = battleCreature.SkillController;
		creatureCacheData.IsInSoloSkill = skillController.IsInSoloSkill();
		if (skillController.ManualTarget != null)
		{
			creatureCacheData.ManualTargetUid = skillController.ManualTarget.Uid;
		}
		if (skillController.ReservedSkill != null)
		{
			creatureCacheData.ReservedSkillIndex = skillController.ReservedSkill.Index;
			creatureCacheData.ReservedSkillSlotIndex = skillController.ReservedSkillSlotIndex;
			creatureCacheData.AutoReservedSkill = skillController.AutoReserved;
			if (skillController.ReservedSkillManualTarget != null)
			{
				creatureCacheData.ReservedSkillManualTargetUid = skillController.ReservedSkillManualTarget.Uid;
			}
		}
		if (skillController.UsingSkillInfo != null)
		{
			creatureCacheData.UsingSkillIndex = skillController.UsingSkillInfo.SkillData.Index;
			if (skillController.UsingSkillInfo.FiredSkillData != null)
			{
				creatureCacheData.UsingFiredSkillIndex = skillController.UsingSkillInfo.FiredSkillData.Index;
				creatureCacheData.UsingSkillSlotIndex = skillController.UsingSkillInfo.SkillSlotIndex;
			}
		}
		if (skillController.CooltimeManager != null)
		{
			creatureCacheData.CooltimeSkillIndices = skillController.CooltimeManager.GetCooltimeSkillIndices();
		}
		if (skillController.SkillCheckModifierContainer != null)
		{
			creatureCacheData.SuppressedUsingCheckSkillIndices = skillController.SkillCheckModifierContainer.GetSuppressedSkillIndices(SkillCheckSuppressType.UsingSkillCheck);
			creatureCacheData.DisabledAutoUseSkillIndices = skillController.SkillCheckModifierContainer.GetDisabledAutoUseSkillIndices();
		}
		if (skillController.SkillChangerContainer != null)
		{
			creatureCacheData.ChangedSkillIndices = skillController.SkillChangerContainer.GetChangedSkillIndices();
		}
		BattleStatisticsManager statisticsManager = battleInstance.Snapshot.StatisticsManager;
		if (statisticsManager.GetCreatureInfoDictionary().TryGetValue(creatureCacheData.Uid, out var value))
		{
			creatureCacheData.EnterTimeMs = value.EnterTimeMs;
			creatureCacheData.EnterWaveIndex = value.WaveIndex;
		}
		creatureCacheData.TotalDamage = statisticsManager.TotalDamageMeter(creatureCacheData.Uid);
		creatureCacheData.TotalDamaged = statisticsManager.TotalDamagedMeter(creatureCacheData.Uid);
		creatureCacheData.TotalHeal = statisticsManager.TotalHealMeter(creatureCacheData.Uid);
		creatureCacheData.TotalHealed = statisticsManager.TotalHealedMeter(creatureCacheData.Uid);
		creatureCacheData.TotalManaHeal = statisticsManager.TotalManaHealMeter(creatureCacheData.Uid);
		creatureCacheData.TotalManaRestored = statisticsManager.TotalManaRestoredMeter(creatureCacheData.Uid);
		creatureCacheData.TotalManaRestoredInUnitTime = battleCreature.ManaHistoryTracker.GetAllRecordSum();
		creatureCacheData.TotalThreatInUnitTime = battleCreature.ThreatHistoryTracker.GetAllRecordSum();
		creatureCacheData.ManaAddictLevel = battleCreature.StateController.GetManaAddictLevel();
		SoulWeaponSkillController soulWeaponSkillController = battleCreature.SoulWeaponSkillController;
		if (soulWeaponSkillController.EquippedSoulWeapon)
		{
			creatureCacheData.IsEquippedSoulWeapon = true;
			creatureCacheData.IsUseableSoulWeaponSkill = soulWeaponSkillController.IsUseableSkill;
			creatureCacheData.SoulWeaponUsableCount = soulWeaponSkillController.UseableSkillCount;
			creatureCacheData.SoulWeaponStackRatioM = soulWeaponSkillController.StackRatioM;
			creatureCacheData.SoulWeaponStackCount = soulWeaponSkillController.StackCount;
			creatureCacheData.SoulWeaponMaxStackCount = soulWeaponSkillController.MaxStackCount;
			creatureCacheData.SoulWeaponCanUseSkillResult = soulWeaponSkillController.SkillUseResult;
		}
		creatureCacheData.IsReservedSoulWeaponSkill = skillController.ReservedSkillSlotIndex == 10;
		CreatureDic.Add(creatureCacheData.Uid, creatureCacheData);
	}

	private void AddEnvironmentCreatureCache(BattleInstance battleInstance)
	{
		for (TeamType teamType = TeamType.Left; teamType < TeamType.All; teamType++)
		{
			if (EnvironmentCreatureDic.ContainsKey(teamType))
			{
				continue;
			}
			BattleCreature environmentCreatureOfTeam = battleInstance.GetEnvironmentCreatureOfTeam(teamType);
			if (environmentCreatureOfTeam == null || (environmentCreatureOfTeam.CreateInfo != null && !environmentCreatureOfTeam.CreateInfo.HasDungeonSkill))
			{
				continue;
			}
			BattleFacade.CreatureCacheData creatureCacheData = new BattleFacade.CreatureCacheData(environmentCreatureOfTeam.Uid);
			creatureCacheData.CreatureData = environmentCreatureOfTeam.CreatureData;
			creatureCacheData.CreateInfo = environmentCreatureOfTeam.CreateInfo;
			creatureCacheData.HeroInfo = environmentCreatureOfTeam.CreateInfo.HeroInfo;
			creatureCacheData.GroupIndex = environmentCreatureOfTeam.GroupIndex;
			creatureCacheData.TeamType = teamType;
			creatureCacheData.DungeonSkillStateInfos = environmentCreatureOfTeam.DungeonSkillController.GetDungeonSkillStateInfos();
			creatureCacheData.HasDungeonSkill = creatureCacheData.DungeonSkillStateInfos != null && creatureCacheData.DungeonSkillStateInfos.Length != 0;
			SkillController skillController = environmentCreatureOfTeam.SkillController;
			creatureCacheData.IsInSoloSkill = skillController.IsInSoloSkill();
			if (skillController.ReservedSkill != null)
			{
				creatureCacheData.ReservedSkillIndex = skillController.ReservedSkill.Index;
				creatureCacheData.ReservedSkillSlotIndex = skillController.ReservedSkillSlotIndex;
				if (skillController.ReservedSkillManualTarget != null)
				{
					creatureCacheData.ReservedSkillManualTargetUid = skillController.ReservedSkillManualTarget.Uid;
				}
			}
			if (skillController.UsingSkillInfo != null)
			{
				creatureCacheData.UsingSkillIndex = skillController.UsingSkillInfo.SkillData.Index;
				if (skillController.UsingSkillInfo.FiredSkillData != null)
				{
					creatureCacheData.UsingFiredSkillIndex = skillController.UsingSkillInfo.FiredSkillData.Index;
					creatureCacheData.UsingSkillSlotIndex = skillController.UsingSkillInfo.SkillSlotIndex;
				}
			}
			if (skillController.CooltimeManager != null)
			{
				creatureCacheData.CooltimeSkillIndices = skillController.CooltimeManager.GetCooltimeSkillIndices();
			}
			if (skillController.SkillChangerContainer != null)
			{
				creatureCacheData.ChangedSkillIndices = skillController.SkillChangerContainer.GetChangedSkillIndices();
			}
			EnvironmentCreatureDic.Add(teamType, creatureCacheData);
		}
	}

	private void AddKillTeamCache(BattleInstance battleInstance)
	{
		for (TeamType teamType = TeamType.Left; teamType < TeamType.All; teamType++)
		{
			int killTeamMeter = battleInstance.Snapshot.StatisticsManager.GetKillTeamMeter(teamType);
			KillTeamMeterDic.Add(teamType, killTeamMeter);
		}
	}

	private void AddCurrentWaveTeamKillCache(BattleInstance battleInstance)
	{
		for (TeamType teamType = TeamType.Left; teamType < TeamType.All; teamType++)
		{
			int currentWaveKillMeter = battleInstance.Snapshot.StatisticsManager.GetCurrentWaveKillMeter(teamType);
			CurrentWaveTeamKillMeterDic.Add(teamType, currentWaveKillMeter);
		}
	}

	private void AddTotalKillTeamCache(BattleInstance battleInstance)
	{
		for (TeamType teamType = TeamType.Left; teamType < TeamType.All; teamType++)
		{
			int totalKillTeamMeter = battleInstance.Snapshot.StatisticsManager.GetTotalKillTeamMeter(teamType);
			TotalKillTeamMeterDic.Add(teamType, totalKillTeamMeter);
		}
	}
}
