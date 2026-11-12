using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NArea;
using NShared.NCampaign;
using NShared.NCampaign.Karma;
using NShared.NUtil;

namespace NShared;

public class BattleLogCreator
{
	private BattleInstance _battleInstance;

	public BattleLogCreator(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
	}

	public List<BattleLog> Create(List<BattleCreatureCreateInfo> battlCreatureCreateInfoList)
	{
		return CreateInitializeDungeonBattleLog(battlCreatureCreateInfoList);
	}

	public WaveReadyBattleLog Create(BattleModeUpdater updater)
	{
		if (_battleInstance == null)
		{
			return null;
		}
		if (updater == null)
		{
			return null;
		}
		CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(updater.ChapterIndex, updater.DungeonIndex, updater.Difficulty, updater.Scenario, updater.WaveIndex);
		WaveReadyBattleLog waveReadyBattleLog = CreateWaveReadyBattleLog(updater, data, 0);
		if (waveReadyBattleLog == null)
		{
			return null;
		}
		return waveReadyBattleLog;
	}

	public WaveReadyBattleLog Create(int waveIndex, int totalWaveIndex)
	{
		if (_battleInstance == null)
		{
			return null;
		}
		CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(_battleInstance.ChapterIndex, _battleInstance.DungeonIndex, (DungeonDifficultyType)_battleInstance.Difficulty, _battleInstance.IsScenario, waveIndex);
		WaveReadyBattleLog waveReadyBattleLog = CreateWaveReadyBattleLog(null, data, totalWaveIndex);
		if (waveReadyBattleLog == null)
		{
			return null;
		}
		return waveReadyBattleLog;
	}

	private List<BattleLog> CreateInitializeDungeonBattleLog(List<BattleCreatureCreateInfo> battlCreatureCreateInfoList)
	{
		if (_battleInstance == null)
		{
			return null;
		}
		if (battlCreatureCreateInfoList == null || battlCreatureCreateInfoList.Count <= 0)
		{
			return null;
		}
		string fieldName = GameRoot.Instance.CampaignDungeonDataContainer.GetFieldName(_battleInstance.ChapterIndex, _battleInstance.DungeonIndex);
		CampaignFieldData data = GameRoot.Instance.CampaignFieldDataContainer.GetData(fieldName);
		if (data == null)
		{
			return null;
		}
		TeamType teamType = TeamType.Left;
		CampaignWaveData waveData = _battleInstance.GetWaveData();
		if (waveData == null)
		{
			return null;
		}
		List<BattleCreatureCreateInfo> creatureCreateInfo = GetCreatureCreateInfo(data.GetBattleAreaDataList(), teamType, waveData, battlCreatureCreateInfoList.ToArray());
		if (creatureCreateInfo == null)
		{
			return null;
		}
		BattlePlayer playerByTeam = _battleInstance.PlayerContainer.GetPlayerByTeam(teamType);
		InitializeDungeonBattleLog runtime = new InitializeDungeonBattleLog
		{
			PlayerId = playerByTeam.Id,
			CreateInfos = creatureCreateInfo.ToArray(),
			ChapterIndex = _battleInstance.ChapterIndex,
			DungeonIndex = _battleInstance.DungeonIndex
		};
		BattleLog item = _battleInstance.BattleLogContainer.AddLog(runtime);
		return new List<BattleLog> { item };
	}

	private List<BattleCreatureCreateInfo> GetCreatureCreateInfo(List<BattleAreaElementData> battleAreaDataList, TeamType teamType, CampaignWaveData waveData, BattleCreatureCreateInfo[] creatureCreateInfos)
	{
		if (battleAreaDataList == null || battleAreaDataList.Count == 0)
		{
			return null;
		}
		if (creatureCreateInfos == null)
		{
			return null;
		}
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		for (int i = 0; i < battleAreaDataList.Count; i++)
		{
			BattleAreaElementData battleAreaData = battleAreaDataList[i];
			if (battleAreaData == null)
			{
				continue;
			}
			List<BattleCreatureCreateInfo> list2 = new List<BattleCreatureCreateInfo>();
			list2.AddRange(Array.FindAll(creatureCreateInfos, (BattleCreatureCreateInfo info) => info.GroupIndex == battleAreaData.BattleAreaGroupIndex));
			if (list2.Count != 0)
			{
				if (waveData != null)
				{
					AddAllyWaveMember(waveData, battleAreaData.BattleAreaGroupIndex, list2);
				}
				int num = (battleAreaData.AreaTopM + battleAreaData.AreaBottomM) / 2;
				int centerBiasM = battleAreaData.CenterBiasM;
				int num2 = ((teamType == TeamType.Left) ? (-centerBiasM) : centerBiasM);
				Vector3 center = new Vector3(num2, 0L, num);
				int angle = battleAreaData.Angle;
				if (angle != 0)
				{
					int waveAreaMaxMx = GetWaveAreaMaxMx(waveData, battleAreaData.BattleAreaGroupIndex);
					int yAxisRadian = (int)((double)angle * Math.PI / 180.0 * 1000.0);
					Vector3 standardPos = new Vector3(waveAreaMaxMx, 0L, num);
					BattleCreatureContainer.SortHeroSpawnPosition(list2, teamType, center, standardPos, yAxisRadian);
				}
				else
				{
					BattleCreatureContainer.SortHeroSpawnPosition(list2, teamType, center);
				}
				list.AddRange(list2);
			}
		}
		if (waveData != null && waveData.AddHeroExtraSkill != null && waveData.AddHeroExtraSkill.Length != 0)
		{
			AddHeroExtraSkill(list, waveData);
		}
		return list;
	}

	private void AddHeroExtraSkill(List<BattleCreatureCreateInfo> creatureCreateInfoList, CampaignWaveData waveData)
	{
		List<ExtraSkillInfo> list = new List<ExtraSkillInfo>();
		foreach (BattleCreatureCreateInfo creatureCreateInfo in creatureCreateInfoList)
		{
			if (creatureCreateInfo.AddExtraSkill != null && creatureCreateInfo.AddExtraSkill.Length != 0)
			{
				list.AddRange(creatureCreateInfo.AddExtraSkill);
			}
			list.AddRangeFromIndex(waveData.AddHeroExtraSkill);
			creatureCreateInfo.AddExtraSkill = list.ToArray();
			list.Clear();
		}
	}

	private int GetWaveAreaMaxMx(CampaignWaveData waveData, int groupIndex)
	{
		if (groupIndex == 0)
		{
			return waveData.AreaPositionMx + waveData.AreaSizeMx / 2;
		}
		return waveData.AreaPositionMx2 + waveData.AreaSizeMx2 / 2;
	}

	private void AddAllyWaveMember(CampaignWaveData waveData, int groupIndex, List<BattleCreatureCreateInfo> groupedCreatureCreateInfoList)
	{
		List<CampaignWaveData.CreatureDetailInfo> creatureList = waveData.GetCreatureList(includeBoss: true, useGroupFilter: true, groupIndex);
		if (creatureList == null || creatureList.Count == 0)
		{
			return;
		}
		for (int i = 0; i < creatureList.Count; i++)
		{
			CampaignWaveData.CreatureDetailInfo waveCreature = creatureList[i];
			if (waveCreature.IsAlly && waveCreature.IsWaveMember && !groupedCreatureCreateInfoList.Any((BattleCreatureCreateInfo e) => e.HeroInfo.HeroIndex == waveCreature.Index))
			{
				groupedCreatureCreateInfoList.Add(BattleCreatureCreateInfo.Create(waveCreature, 0));
			}
		}
	}

	private WaveReadyBattleLog CreateWaveReadyBattleLog(BattleModeUpdater updater, CampaignWaveData waveData, int totalWaveIndex)
	{
		if (_battleInstance == null)
		{
			return null;
		}
		BattleCreatureList aliveCreaturesOfTeam = _battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Left);
		BattleCreatureList aliveCreaturesOfTeam2 = _battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Right);
		if (aliveCreaturesOfTeam.Count == 0 && aliveCreaturesOfTeam2.Count == 0)
		{
			return null;
		}
		if (updater != null && updater is CampaignUpdater { DelayedCreatureLogList: not null } campaignUpdater)
		{
			campaignUpdater.DelayedCreatureLogList.Clear();
		}
		Dictionary<int, ExtendArea> battleAreaList = _battleInstance.BattleArea.GetBattleAreaList();
		if (battleAreaList == null || battleAreaList.Count == 0)
		{
			return null;
		}
		List<WaveCenterPositionInfo> defaultWaveCenterPositionList = GetDefaultWaveCenterPositionList(battleAreaList);
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		List<BattleCreatureCreateInfo> list2 = new List<BattleCreatureCreateInfo>();
		if (waveData != null)
		{
			foreach (KeyValuePair<int, ExtendArea> item in battleAreaList)
			{
				int groupIndex = item.Key;
				ExtendArea value = item.Value;
				BattleCreatureList aliveCreaturesOfTeam3 = _battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Left, groupIndex);
				BattleCreatureList aliveCreaturesOfTeam4 = _battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Right, groupIndex);
				if (aliveCreaturesOfTeam3.Count == 0 && aliveCreaturesOfTeam4.Count == 0)
				{
					continue;
				}
				CampaignWaveData.WaveAreaInfo waveAreaInfo = waveData.GetWaveAreaInfoInfo(groupIndex);
				if (waveAreaInfo == null)
				{
					continue;
				}
				List<BattleCreatureCreateInfo> list3 = new List<BattleCreatureCreateInfo>();
				List<BattleCreatureCreateInfo> list4 = new List<BattleCreatureCreateInfo>();
				int centerMz = (value.MaxMz + value.MinMz) / 2;
				Vector3 waveCenterPosition = BattleModeHelper.GetWaveCenterPosition(centerMz, waveAreaInfo.AreaPositionMx, waveAreaInfo.AreaPositionIsRelative, aliveCreaturesOfTeam3, aliveCreaturesOfTeam4);
				List<CampaignWaveData.CreatureDetailInfo> creatureList = waveData.GetCreatureList(includeBoss: true, useGroupFilter: true, groupIndex);
				if (creatureList != null && creatureList.Count > 0)
				{
					list4.AddRange(CreateCreatureCreateInfoList(updater, creatureList, isAlly: true, waveAreaInfo.AreaPositionMx, centerMz, groupIndex));
					list3.AddRange(CreateCreatureCreateInfoList(updater, creatureList, isAlly: false, waveAreaInfo.AreaPositionMx, centerMz, groupIndex));
					BattleCreatureContainer.SortHeroSpawnPosition(list4, TeamType.Left, waveCenterPosition, value, _battleInstance.GetBattleFormation());
					BattleCreatureContainer.SortHeroSpawnPosition(list3, TeamType.Right, waveCenterPosition);
					list3.ForEach(delegate(BattleCreatureCreateInfo e)
					{
						e.Position += Vector3.right * waveAreaInfo.RightTeamOffsetMx / 1000L;
					});
				}
				defaultWaveCenterPositionList.Find((WaveCenterPositionInfo pos) => pos.GroupIndex == groupIndex)?.SetPosition(waveCenterPosition);
				list2.AddRange(list4);
				list.AddRange(list3);
			}
		}
		return new WaveReadyBattleLog
		{
			ChapterIndex = _battleInstance.ChapterIndex,
			DungeonIndex = _battleInstance.DungeonIndex,
			Difficulty = (DungeonDifficultyType)_battleInstance.Difficulty,
			WaveIndex = (waveData?.WaveIndex ?? 0),
			MaxWaveIndex = (updater?.MaxWaveIndex ?? 0),
			MaxStageLevel = (updater?.MaxStageLevel ?? 0),
			TotalWaveIndex = (updater?.TotalWaveIndex ?? totalWaveIndex),
			MonsterCreateInfos = list.ToArray(),
			AllianceCreateInfos = list2.ToArray(),
			WaveCenterPositionInfos = defaultWaveCenterPositionList.ToArray()
		};
	}

	private List<WaveCenterPositionInfo> GetDefaultWaveCenterPositionList(Dictionary<int, ExtendArea> battleAreaDic)
	{
		List<WaveCenterPositionInfo> list = new List<WaveCenterPositionInfo>();
		foreach (KeyValuePair<int, ExtendArea> item2 in battleAreaDic)
		{
			int key = item2.Key;
			ExtendArea value = item2.Value;
			int z = (value.MaxMz + value.MinMz) / 2;
			WaveCenterPositionInfo item = new WaveCenterPositionInfo(key, 0, 0, z);
			list.Add(item);
		}
		return list;
	}

	private List<BattleCreatureCreateInfo> CreateCreatureCreateInfoList(BattleModeUpdater updater, List<CampaignWaveData.CreatureDetailInfo> creatureList, bool isAlly, int areaPositionMx, int centerMz, int groupIndex)
	{
		if (_battleInstance == null)
		{
			return null;
		}
		Random random = new Random((int)DateTime.UtcNow.Ticks + (isAlly ? 1 : 0));
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		KarmaUpdater karmaUpdater = updater as KarmaUpdater;
		for (int i = 0; i < creatureList.Count; i++)
		{
			CampaignWaveData.CreatureDetailInfo creatureDetailInfo = creatureList[i];
			CampaignWaveData.CreatureDetailWritable creatureDetailWritable = null;
			if (creatureDetailInfo.IsAlly != isAlly || creatureDetailInfo.IsWaveMember || creatureDetailInfo.GroupIndex != groupIndex)
			{
				continue;
			}
			if (updater != null)
			{
				if (updater is EclipseOfflineModeUpdater)
				{
					int currentCreatureLevel = (updater as EclipseOfflineModeUpdater).GetCurrentCreatureLevel();
					if (currentCreatureLevel > 0)
					{
						int eclipseDungeonIncreaseLevelStep = EclipseOfflineModeUpdater.GetEclipseDungeonIncreaseLevelStep();
						if (creatureDetailWritable == null)
						{
							creatureDetailWritable = creatureDetailInfo.CloneWritable();
							creatureDetailInfo = creatureDetailWritable;
						}
						creatureDetailWritable._Level += currentCreatureLevel * eclipseDungeonIncreaseLevelStep;
					}
				}
				else if (karmaUpdater != null)
				{
					List<int> list2 = new List<int>();
					int punishmentGroupIndex = karmaUpdater.PunishmentGroupIndex;
					IList<int> allEnemyPowerUpSkills = GameRoot.Instance.KarmaEnemyPowerUpDataContainer.GetAllEnemyPowerUpSkills(punishmentGroupIndex);
					int[] unlockSkills = GameRoot.Instance.KarmaEnemyUnlockSkillDataContainer.GetUnlockSkills(punishmentGroupIndex, _battleInstance.Snapshot.MaxStageLevel);
					if (allEnemyPowerUpSkills != null && allEnemyPowerUpSkills.Count > 0)
					{
						list2.AddRange(allEnemyPowerUpSkills);
					}
					if (unlockSkills != null && unlockSkills.Length != 0)
					{
						list2.AddRange(unlockSkills);
					}
					if (creatureDetailWritable == null)
					{
						creatureDetailWritable = creatureDetailInfo.CloneWritable();
						creatureDetailInfo = creatureDetailWritable;
					}
					if (creatureDetailWritable.AddExtraSkill == null)
					{
						creatureDetailWritable._AddExtraSkill = list2.ToArray();
					}
					else
					{
						creatureDetailWritable._AddExtraSkill = creatureDetailWritable.AddExtraSkill.Concat(list2).ToArray();
					}
				}
			}
			int randomSeed = ((updater != null && updater.ForcedRandomSeed >= 0) ? updater.ForcedRandomSeed : random.Next());
			BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.Create(creatureDetailInfo, randomSeed);
			if (battleCreatureCreateInfo != null)
			{
				if (updater != null && creatureDetailInfo.DelayMs > 0)
				{
					long playerId = _battleInstance.PlayerContainer.GetPlayerByTeam((!isAlly) ? TeamType.Right : TeamType.Left)?.Id ?? 0;
					battleCreatureCreateInfo.Position = new Vector3(areaPositionMx + creatureDetailInfo.OffsetMx, 0L, centerMz + creatureDetailInfo.OffsetMz);
					(updater as CampaignUpdater).DelayedCreatureLogList.Add(new CreatureCreateBattleLog
					{
						CreateInfo = battleCreatureCreateInfo,
						PlayerId = playerId
					});
				}
				else
				{
					list.Add(battleCreatureCreateInfo);
				}
			}
		}
		return list;
	}
}
