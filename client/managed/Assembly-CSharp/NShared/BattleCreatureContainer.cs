using System;
using System.Collections.Generic;
using NShared.NArea;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class BattleCreatureContainer
{
	private readonly BattleInstance _battleInstance;

	private readonly IndexableSnapshotDictionary<int, BattleCreature> _creatureDictionary;

	private readonly Dictionary<int, BattleCreatureStockData> _stockDataByCreatureUid = new Dictionary<int, BattleCreatureStockData>();

	private readonly Dictionary<int, ResultCreatureInfo> _destroyedCreatureInfos = new Dictionary<int, ResultCreatureInfo>();

	public BattleCreatureContainer(BattleInstance battleInstance)
	{
		_creatureDictionary = new IndexableSnapshotDictionary<int, BattleCreature>();
		_battleInstance = battleInstance;
	}

	public void Update(long currentTimeMs, int deltaTimeMs)
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			_creatureDictionary.GetValueByIndex(i).Update(currentTimeMs, deltaTimeMs);
		}
	}

	public List<BattleCreature> GetAllCreatures()
	{
		List<BattleCreature> list = new List<BattleCreature>();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			list.Add(valueByIndex);
		}
		return list;
	}

	public object Serialize(SerializedSnapshotContainer container)
	{
		return ((ISnapshotObject)_creatureDictionary)?.Serialize(container);
	}

	public BattleCreature Create(long playerId, BattleCreatureCreateInfo createInfo, int createdUid = 0)
	{
		if (createInfo == null)
		{
			return null;
		}
		Team teamOfPlayer = _battleInstance.GetTeamOfPlayer(playerId);
		ResultCreatureInfo initialCreatureInfo = _battleInstance.GetInitialCreatureInfo(createInfo.HeroInfo.HeroIndex, createInfo.WaveSlotIndex, teamOfPlayer.Id);
		if (initialCreatureInfo != null && initialCreatureInfo.Hp == 0L)
		{
			return null;
		}
		if (GameRoot.Instance.JsonLoader == null)
		{
			VespaDebug.LogError("TextLoader is null");
			return null;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(createInfo.HeroInfo.HeroIndex);
		if (data == null)
		{
			VespaDebug.LogError("CreatureData is null. index=" + createInfo.HeroInfo.HeroIndex);
			return null;
		}
		BattleCreatureStockData battleCreatureStockData = new BattleCreatureStockData();
		battleCreatureStockData.CreatureData = SprkArenaPractice.CombatTarget(data);
		battleCreatureStockData.CreateInfo = SprkArenaPractice.ProtectTarget(createInfo);
		battleCreatureStockData.InitialInfo = initialCreatureInfo;
		battleCreatureStockData.PlayerId = playerId;
		battleCreatureStockData.CreatedTime = _battleInstance.MainTimer.GetCurrentTimeMs();
		int uid = ((createdUid > 0) ? createdUid : (++_battleInstance.Snapshot.CreatureUid));
		BattleCreature battleCreature = new BattleCreature();
		battleCreature.Init(_battleInstance, battleCreatureStockData);
		battleCreature.InitSnapshot(uid);
		AddCreatureInternal(battleCreature);
		if (createdUid == 0)
		{
			battleCreature.OnCreated();
		}
		return battleCreature;
	}

	public bool RegisterCreated(BattleCreature creature)
	{
		if (creature == null)
		{
			return false;
		}
		int uid = creature.Uid;
		BattleCreatureStockData value = null;
		_stockDataByCreatureUid.TryGetValue(uid, out value);
		if (value == null)
		{
			VespaDebug.LogError("Failed to find battle creature stock data : uid=" + uid);
			return false;
		}
		creature.Init(_battleInstance, value);
		AddCreatureInternal(creature);
		return true;
	}

	private void AddCreatureInternal(BattleCreature creature)
	{
		if (creature != null)
		{
			int uid = creature.Uid;
			if (!_creatureDictionary.TryAdd(uid, creature))
			{
				VespaDebug.LogError("failed to add creature. creatureId=" + uid);
				_creatureDictionary.Remove(uid);
				_creatureDictionary.Add(uid, creature);
			}
			int waveIndex = _battleInstance.Snapshot.WaveIndex;
			_battleInstance.Snapshot.StatisticsManager.AddCreature(uid, creature.PlayerId, waveIndex, creature.CreateInfo, _battleInstance.GlobalTimer.GetCurrentTimeMs());
		}
	}

	public void UpdateRangeBonus(TeamType teamType)
	{
		BattleCreatureList aliveCreaturesOfTeam = GetAliveCreaturesOfTeam(teamType);
		if (aliveCreaturesOfTeam != null && aliveCreaturesOfTeam.Count != 0)
		{
			int num = 0;
			BattleCreatureList battleCreatureList = SortCreatureByWavePosition(aliveCreaturesOfTeam);
			for (int i = 0; i < battleCreatureList.Count; i++)
			{
				battleCreatureList[i].TransformController.SetRangeBonus(num);
				num++;
			}
		}
	}

	public int GetCreatureCount()
	{
		return _creatureDictionary.Count;
	}

	public BattleCreature GetCreatureByIndex(int index)
	{
		return _creatureDictionary.GetValueByIndex(index);
	}

	public BattleCreature GetCreatureByKey(string key)
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex != null && valueByIndex.SummonerUid == 0 && string.Equals(valueByIndex.GetCreatureKey(), key, StringComparison.OrdinalIgnoreCase))
			{
				return valueByIndex;
			}
		}
		return null;
	}

	public BattleCreature GetCreature(int creatureUid)
	{
		_creatureDictionary.TryGetValue(creatureUid, out var value);
		return value;
	}

	private void ForEachCreature(Action<BattleCreature> action)
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			action(valueByIndex);
		}
	}

	private void ForEachCreature(TeamType teamType, bool isAlly, Action<BattleCreature> action)
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamController.IsFriendly(teamType) == isAlly)
			{
				action(valueByIndex);
			}
		}
	}

	public BattleCreature GetCreatureOfPlayer(long playerId, int creatureIndex)
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.CreatureIndex == creatureIndex && valueByIndex.PlayerId == playerId)
			{
				return valueByIndex;
			}
		}
		return null;
	}

	public BattleCreatureList GetCreaturesOfPlayer(long playerId)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.PlayerId == playerId)
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public List<int> GetCreatureIndicesOfPlayer(long playerId)
	{
		List<int> list = new List<int>();
		BattleCreatureList creaturesOfPlayer = GetCreaturesOfPlayer(playerId);
		for (int i = 0; i < creaturesOfPlayer.Count; i++)
		{
			list.Add(creaturesOfPlayer[i].CreatureIndex);
		}
		return list;
	}

	public bool Destroy(int creatureUid, bool raiseEvent)
	{
		if (!_creatureDictionary.TryRemove(creatureUid, out var value))
		{
			return false;
		}
		if (value == null)
		{
			return false;
		}
		ResultCreatureInfo resultCreatureInfo = BattleInstanceIntegrityChecker.CreateCreatureInfo(value);
		if (resultCreatureInfo != null)
		{
			_destroyedCreatureInfos[creatureUid] = resultCreatureInfo;
		}
		if (raiseEvent)
		{
			_battleInstance.RaiseBattleEvent(new DestroyCreature
			{
				CreatureUid = creatureUid
			});
		}
		return true;
	}

	public Dictionary<int, ResultCreatureInfo> GetDestroyedCreatureInfos()
	{
		return _destroyedCreatureInfos;
	}

	public BattleCreatureList GetCreatureListOfRange(Vector3 center, int rangeMm)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (IsInRadius(valueByIndex, center, rangeMm))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public static bool IsInRadius(BattleCreature creature, Vector3 center, int rangeMm)
	{
		Vector3 position = creature.TransformController.Position;
		int sizeMm = creature.CreatureData.SizeMm;
		return IsInRadius(position, sizeMm, center, rangeMm);
	}

	public static bool IsInRadius(Vector3 creaturePos, int creatureSizeMm, Vector3 center, int rangeMm)
	{
		long num = (long)rangeMm + (long)creatureSizeMm;
		long num2 = num * num;
		return Vector3.DistanceSqrWithoutY(center, creaturePos) <= num2;
	}

	public static BattleCreatureList SortCreatureByWavePosition(BattleCreatureList creatureList)
	{
		return SortCreatureByWavePosition2(creatureList, (BattleCreature creature) => creature.CreateInfo.GetWavePositionScore());
	}

	public static BattleCreatureList SortCreatureByWavePosition2(BattleCreatureList list, Func<BattleCreature, long> selector)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		if (list == null || list.Count == 0)
		{
			return battleCreatureList;
		}
		battleCreatureList.AddRange(list);
		battleCreatureList.Sort(delegate(BattleCreature l, BattleCreature r)
		{
			if (l == r)
			{
				return 0;
			}
			long num = selector(l);
			long num2 = selector(r);
			if (num != num2)
			{
				return num2.CompareTo(num);
			}
			int num3 = 0;
			int value = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == l)
				{
					num3 = i;
				}
				if (list[i] == r)
				{
					value = i;
				}
			}
			return num3.CompareTo(value);
		});
		return battleCreatureList;
	}

	public static List<T> SortListByScore<T>(IList<T> list, Func<T, long> selector) where T : class
	{
		List<T> list2 = new List<T>();
		if (list == null || list.Count == 0)
		{
			return list2;
		}
		list2.AddRange(list);
		list2.Sort(delegate(T l, T r)
		{
			if (l == r)
			{
				return 0;
			}
			long num = selector(l);
			long num2 = selector(r);
			if (num != num2)
			{
				return num2.CompareTo(num);
			}
			int num3 = 0;
			int value = 0;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == l)
				{
					num3 = i;
				}
				if (list[i] == r)
				{
					value = i;
				}
			}
			return num3.CompareTo(value);
		});
		return list2;
	}

	public static void SortHeroSpawnPosition(IList<BattleCreatureCreateInfo> createInfoList, TeamType teamType, Vector3 center, EBattleFormation formation = EBattleFormation.Default)
	{
		List<BattleCreatureCreateInfo> list = SortListByScore(createInfoList, (BattleCreatureCreateInfo createInfo) => createInfo.GetWavePositionScore());
		List<Vector3> waveReadyPosition = BattleModeHelper.GetWaveReadyPosition(center, list.Count, teamType, formation);
		SortHeroSpawnPosition(list, waveReadyPosition, center);
	}

	public static void SortHeroSpawnPosition(IList<BattleCreatureCreateInfo> createInfoList, TeamType teamType, Vector3 center, Vector3 standardPos, int yAxisRadian, EBattleFormation formation = EBattleFormation.Default)
	{
		List<BattleCreatureCreateInfo> list = SortListByScore(createInfoList, (BattleCreatureCreateInfo createInfo) => createInfo.GetWavePositionScore());
		List<Vector3> waveReadyPosition = BattleModeHelper.GetWaveReadyPosition(center, list.Count, teamType, formation);
		for (int num = 0; num < waveReadyPosition.Count; num++)
		{
			Vector3 value = waveReadyPosition[num];
			value.YawPoint(standardPos, yAxisRadian);
			waveReadyPosition[num] = value;
		}
		SortHeroSpawnPosition(list, waveReadyPosition, center);
	}

	public static void SortHeroSpawnPosition(IList<BattleCreatureCreateInfo> createInfoList, TeamType teamType, Vector3 center, ExtendArea area, EBattleFormation formation = EBattleFormation.Default)
	{
		List<BattleCreatureCreateInfo> list = SortListByScore(createInfoList, (BattleCreatureCreateInfo createInfo) => createInfo.GetWavePositionScore());
		List<Vector3> waveReadyPosition = BattleModeHelper.GetWaveReadyPosition(center, list.Count, teamType, formation);
		area?.YawCreaturePositionList(waveReadyPosition);
		SortHeroSpawnPosition(list, waveReadyPosition, center);
	}

	private static void SortHeroSpawnPosition(IList<BattleCreatureCreateInfo> sortedCreateInfoList, IList<Vector3> heroPositionList, Vector3 center)
	{
		int num = 0;
		for (int i = 0; i < sortedCreateInfoList.Count; i++)
		{
			BattleCreatureCreateInfo battleCreatureCreateInfo = sortedCreateInfoList[i];
			Vector3 position = heroPositionList[num];
			battleCreatureCreateInfo.Position = GetHeroSpawnPosition(position, center, battleCreatureCreateInfo.OffsetMx, battleCreatureCreateInfo.OffsetMz);
			num++;
		}
	}

	private static Vector3 GetHeroSpawnPosition(Vector3 position, Vector3 center, int creatureOffsetMx, int creatureOffsetMz)
	{
		if (creatureOffsetMx != 0 || creatureOffsetMz != 0)
		{
			position = center + new Vector3(creatureOffsetMx, 0L, creatureOffsetMz);
		}
		return position;
	}

	public BattleCreatureList GetChildCreatureList(int summonerUid, bool aliveOnly)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.SummonerUid == summonerUid && (!aliveOnly || valueByIndex.IsAliveAndPlaying()))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public BattleCreatureList GetCreatures(BattleCreatureList exceptions = null)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex != null && (exceptions == null || !exceptions.Contains(valueByIndex)))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public BattleCreatureList GetAliveCreatures(BattleCreatureList exceptions = null)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.IsAliveAndPlaying() && (exceptions == null || !exceptions.Contains(valueByIndex)))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public BattleCreatureList GetAllCreaturesOfTeam(int teamId, BattleCreatureList exceptions = null)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamController.TeamId == teamId && (exceptions == null || !exceptions.Contains(valueByIndex)))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public int[] GetAllCreatureIndices(TeamType teamType)
	{
		List<int> list = new List<int>();
		BattleCreatureList allCreaturesOfTeam = GetAllCreaturesOfTeam(teamType);
		for (int i = 0; i < allCreaturesOfTeam.Count; i++)
		{
			BattleCreature battleCreature = allCreaturesOfTeam[i];
			if (battleCreature != null)
			{
				list.Add(battleCreature.CreatureIndex);
			}
		}
		return list.ToArray();
	}

	public BattleCreatureList GetAllCreaturesOfTeam(TeamType teamType, BattleCreatureList exceptions = null)
	{
		return GetAllCreaturesOfTeam((int)teamType, exceptions);
	}

	public BattleCreatureList GetAliveCreaturesOfTeam(int teamId, BattleCreatureList exceptions = null)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamController.TeamId == teamId && valueByIndex.IsAliveAndPlaying() && (exceptions == null || !exceptions.Contains(valueByIndex)))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public int[] GetAliveCreatureIndices(TeamType teamType)
	{
		List<int> list = new List<int>();
		BattleCreatureList aliveCreaturesOfTeam = GetAliveCreaturesOfTeam(teamType);
		for (int i = 0; i < aliveCreaturesOfTeam.Count; i++)
		{
			BattleCreature battleCreature = aliveCreaturesOfTeam[i];
			if (battleCreature != null)
			{
				list.Add(battleCreature.CreatureIndex);
			}
		}
		return list.ToArray();
	}

	public BattleCreatureList GetAliveCreaturesOfTeam(TeamType teamType, BattleCreatureList exceptions = null)
	{
		return GetAliveCreaturesOfTeam((int)teamType, exceptions);
	}

	public BattleCreatureList GetAliveCreaturesOfTeam(TeamType teamType, int groupIndex, BattleCreatureList exceptions = null)
	{
		return GetAliveCreaturesOfTeam((int)teamType, groupIndex, exceptions);
	}

	public BattleCreatureList GetAliveCreaturesOfTeam(int teamId, int groupIndex, BattleCreatureList exceptions = null)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamController.TeamId == teamId && valueByIndex.CreateInfo.GroupIndex == groupIndex && valueByIndex.IsAliveAndPlaying() && (exceptions == null || !exceptions.Contains(valueByIndex)))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public BattleCreatureList GetAliveCreaturesOfOtherTeam(int teamId, BattleCreatureList exceptions = null)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamController.TeamId != teamId && valueByIndex.IsAliveAndPlaying() && (exceptions == null || !exceptions.Contains(valueByIndex)))
			{
				battleCreatureList.Add(valueByIndex);
			}
		}
		return battleCreatureList;
	}

	public BattleCreatureList GetAliveCreaturesOfOtherTeam(TeamType teamType, BattleCreatureList exceptions = null)
	{
		return GetAliveCreaturesOfOtherTeam((int)teamType, exceptions);
	}

	public BattleCreatureList GetRestingCreatureOfTeam(TeamType teamType, int waveIndex)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamController.TeamType == teamType)
			{
				BattleCreatureRestingInfo restingInfo = valueByIndex.RestingInfo;
				if (restingInfo != null && (restingInfo.WakeUpWaveIndex == 0 || restingInfo.WakeUpWaveIndex == waveIndex))
				{
					battleCreatureList.Add(valueByIndex);
				}
			}
		}
		return battleCreatureList;
	}

	public int GetAliveCreatureCountOfTeam(int teamId)
	{
		int num = 0;
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (valueByIndex.TeamController.TeamId == teamId && valueByIndex.IsAliveAndPlaying())
			{
				num++;
			}
		}
		return num;
	}

	public int GetAliveCreatureCountOfTeam(TeamType teamType)
	{
		return GetAliveCreatureCountOfTeam((int)teamType);
	}

	public int GetAliveCreatureCountOfOtherTeam(int teamId, bool targetableOnly = false)
	{
		int num = 0;
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if ((!targetableOnly || valueByIndex.StateController.CanBeTargetedState(isAlly: false)) && valueByIndex.TeamController.TeamId != teamId && valueByIndex.IsAliveAndPlaying())
			{
				num++;
			}
		}
		return num;
	}

	public int GetAliveCreatureCountOfOtherTeam(TeamType teamType)
	{
		return GetAliveCreatureCountOfOtherTeam((int)teamType);
	}

	public bool AnyCreatureUsingSkill()
	{
		for (int i = 0; i < _creatureDictionary.Count; i++)
		{
			BattleCreature valueByIndex = _creatureDictionary.GetValueByIndex(i);
			if (!valueByIndex.StatController.IsDead() && valueByIndex.SkillController.UsingSkillInfo != null)
			{
				return true;
			}
		}
		return false;
	}

	public BattleCreatureList FindSkillTargetCreature(BattleCreature firedCreature, SkillData skillData, bool checkOnly, ISkillTriggerInfo triggerInfo, BattleCreature skillManualTarget = null, BattleCreature[] prevUsedTargets = null, BattleCreature[] prevFiredTargets = null)
	{
		if (skillData == null)
		{
			return null;
		}
		SkillTargetType skillTargetType = skillData.GetSkillTargetType(firedCreature);
		string[] targetStateType = skillData.TargetStateType;
		int totalTargetCount = skillData.GetTotalTargetCount(firedCreature);
		bool deadTarget = skillData.DeadTarget;
		bool ignoreBlock = skillData.IgnoreBlock;
		SkillTargetFlag targetFlag = skillData.TargetFlag;
		BattleCreature manualTarget = firedCreature.SkillController.ManualTarget;
		BattleCreatureList battleCreatureList = null;
		switch (skillTargetType)
		{
		case SkillTargetType.Trigger:
			battleCreatureList = BattleCreatureList.Alloc();
			if (triggerInfo != null && triggerInfo.TriggerCreature != null)
			{
				battleCreatureList.Add(triggerInfo.TriggerCreature);
			}
			return battleCreatureList;
		case SkillTargetType.TriggerTarget:
			battleCreatureList = BattleCreatureList.Alloc();
			if (triggerInfo != null && triggerInfo.TargetCreatures != null)
			{
				battleCreatureList.AddRange(triggerInfo.TargetCreatures);
			}
			return battleCreatureList;
		case SkillTargetType.PrevUsedTarget:
			battleCreatureList = BattleCreatureList.Alloc();
			if (prevUsedTargets != null && prevUsedTargets.Length != 0)
			{
				battleCreatureList.AddRange(prevUsedTargets);
			}
			return battleCreatureList;
		case SkillTargetType.PrevFiredTarget:
			battleCreatureList = BattleCreatureList.Alloc();
			if (prevFiredTargets != null && prevFiredTargets.Length != 0)
			{
				battleCreatureList.AddRange(prevFiredTargets);
			}
			return battleCreatureList;
		default:
			if (totalTargetCount == 1)
			{
				if (skillManualTarget != null && firedCreature.TeamController.IsFriendly(skillManualTarget.TeamController) == skillTargetType.IsFriendly())
				{
					battleCreatureList = BattleCreatureList.AllocWithCreature(skillManualTarget);
					battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
					if (battleCreatureList.Count == 1)
					{
						return battleCreatureList;
					}
				}
				if (manualTarget != null && firedCreature.TeamController.IsFriendly(manualTarget.TeamController) == skillTargetType.IsFriendly())
				{
					battleCreatureList = BattleCreatureList.AllocWithCreature(manualTarget);
					battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
					if (battleCreatureList.Count == 1)
					{
						return battleCreatureList;
					}
				}
			}
			switch (skillTargetType)
			{
			case SkillTargetType.Myself:
				battleCreatureList = BattleCreatureList.AllocWithCreature(firedCreature);
				battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
				return battleCreatureList;
			case SkillTargetType.Summoner:
			{
				BattleCreature creature = GetCreature(firedCreature.SummonerUid);
				if (creature == null)
				{
					return null;
				}
				battleCreatureList = BattleCreatureList.AllocWithCreature(creature);
				battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
				return battleCreatureList;
			}
			case SkillTargetType.Summonee:
				battleCreatureList = GetChildCreatureList(firedCreature.Uid, aliveOnly: false);
				battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
				return battleCreatureList;
			case SkillTargetType.SummonerTarget:
			{
				BattleCreature creature2 = GetCreature(firedCreature.SummonerUid);
				if (creature2 == null)
				{
					return null;
				}
				BattleCreature lastTarget = creature2.SkillController.LastTarget;
				if (lastTarget == null)
				{
					return null;
				}
				battleCreatureList = BattleCreatureList.AllocWithCreature(lastTarget);
				battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
				return battleCreatureList;
			}
			case SkillTargetType.SummoneeTarget:
			{
				BattleCreatureList childCreatureList = GetChildCreatureList(firedCreature.Uid, aliveOnly: false);
				if (childCreatureList.Count == 0)
				{
					return null;
				}
				BattleCreature lastTarget2 = childCreatureList[0].SkillController.LastTarget;
				if (lastTarget2 == null)
				{
					return null;
				}
				battleCreatureList = BattleCreatureList.AllocWithCreature(lastTarget2);
				battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
				return battleCreatureList;
			}
			default:
			{
				bool num = firedCreature.TeamController.IsTeamInverted();
				bool flag = skillTargetType.IsFriendly();
				if (num)
				{
					BattleCreatureList battleCreatureList2 = BattleCreatureList.Alloc();
					battleCreatureList2.Add(firedCreature);
					battleCreatureList = (flag ? GetAliveCreaturesOfOtherTeam(firedCreature.TeamController.TeamId) : GetAliveCreaturesOfTeam(firedCreature.TeamController.TeamId, battleCreatureList2));
				}
				else
				{
					battleCreatureList = (flag ? GetAliveCreaturesOfTeam(firedCreature.TeamController.TeamId) : GetAliveCreaturesOfOtherTeam(firedCreature.TeamController.TeamId));
				}
				battleCreatureList.FilterByDefault(firedCreature, skillTargetType, targetStateType, deadTarget, ignoreBlock, targetFlag);
				if (skillData.HasTargetFlag(SkillTargetFlag.ExcludePrevFiredTarget))
				{
					battleCreatureList.RemoveTarget(prevFiredTargets);
				}
				bool flag2 = skillManualTarget != null && battleCreatureList.Contains(skillManualTarget);
				bool flag3 = manualTarget != null && battleCreatureList.Contains(manualTarget);
				ReorderSkillTargetAndTruncate(skillData, firedCreature, checkOnly, triggerInfo, ref battleCreatureList);
				if ((flag2 || flag3) && battleCreatureList.Count > 0)
				{
					if ((skillManualTarget != null && battleCreatureList.Contains(skillManualTarget)) || (manualTarget != null && battleCreatureList.Contains(manualTarget)))
					{
						return battleCreatureList;
					}
					for (int num2 = battleCreatureList.Count - 2; num2 >= 0; num2--)
					{
						battleCreatureList[num2 + 1] = battleCreatureList[num2];
					}
					battleCreatureList[0] = (flag2 ? skillManualTarget : manualTarget);
				}
				return battleCreatureList;
			}
			}
		}
	}

	private static void ReorderSkillTargetAndTruncate(SkillData skillData, BattleCreature firedCreature, bool checkOnly, ISkillTriggerInfo triggerInfo, ref BattleCreatureList targetCreatureList)
	{
		if (skillData == null)
		{
			return;
		}
		SkillTargetType skillTargetType = skillData.GetSkillTargetType(firedCreature);
		int totalTargetCount = skillData.GetTotalTargetCount(firedCreature);
		if (skillTargetType.IsExcludeMe())
		{
			targetCreatureList.Remove(firedCreature);
		}
		if (firedCreature != null && firedCreature.SkillController.SkillTargetController.HasSkillTargetFilter())
		{
			bool ignoreFilterOnCheckOnly = firedCreature.SkillController.SkillTargetController.GetIgnoreFilterOnCheckOnly();
			bool flag = true;
			if (!ignoreFilterOnCheckOnly && skillData.HasTargetFlag(SkillTargetFlag.FilterOnCheckOnly))
			{
				flag = checkOnly;
			}
			if (flag)
			{
				SkillTargetFilterInfo skillTargetFilterInfo = new SkillTargetFilterInfo();
				skillTargetFilterInfo.SkillData = skillData;
				firedCreature.SkillController.SkillTargetController.FilterTarget(ref targetCreatureList, skillTargetFilterInfo);
				if (targetCreatureList == null || targetCreatureList.Count == 0)
				{
					return;
				}
			}
		}
		switch (skillTargetType)
		{
		case SkillTargetType.Ally:
			targetCreatureList.SortByDistance(firedCreature, near: true);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.Friend:
			targetCreatureList.SortByDistance(firedCreature, near: true);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.Enemy:
			targetCreatureList.SortBySkillDistance(firedCreature, skillData.GetMinRangeMm(firedCreature), skillData.GetMaxRangeMm(firedCreature), near: true);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.FarAlly:
		case SkillTargetType.FarEnemy:
		case SkillTargetType.FarFriend:
			targetCreatureList.SortByDistance(firedCreature, near: false);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.MinHpAlly:
		case SkillTargetType.MinHpEnemy:
		case SkillTargetType.MinHpFriend:
			targetCreatureList.SortByMinHpRatio();
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.RandomAlly:
		case SkillTargetType.RandomEnemy:
		case SkillTargetType.RandomFriend:
		{
			if (skillData.HasTargetFlag(SkillTargetFlag.IgnoreRangeCheckOnFire) && !checkOnly)
			{
				targetCreatureList.Shuffle();
				targetCreatureList.RemainIfNotZero(totalTargetCount);
				break;
			}
			int maxRangeMm = skillData.GetMaxRangeMm(firedCreature);
			int minRangeMm = skillData.GetMinRangeMm(firedCreature);
			if (maxRangeMm > 0 || minRangeMm > 0)
			{
				BattleCreature battleCreature = ((targetCreatureList.Count > 0) ? targetCreatureList[0] : null);
				targetCreatureList.FilterBySkillDistance(firedCreature, minRangeMm, maxRangeMm);
				if (targetCreatureList.Count == 0 && battleCreature != null && checkOnly && skillData.BaseAttack)
				{
					targetCreatureList.Add(battleCreature);
				}
			}
			if (!checkOnly)
			{
				targetCreatureList.Shuffle();
			}
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		}
		case SkillTargetType.MaxPhysicalPowerAlly:
		case SkillTargetType.MaxPhysicalPowerEnemy:
		case SkillTargetType.MaxPhysicalPowerFriend:
			targetCreatureList.SortByMaxStat(StatType.PhysicalAttack);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.MaxMagicalPowerAlly:
		case SkillTargetType.MaxMagicalPowerEnemy:
		case SkillTargetType.MaxMagicalPowerFriend:
			targetCreatureList.SortByMaxStat(StatType.MagicalAttack);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.MaxPhysicalDefenseAlly:
		case SkillTargetType.MaxPhysicalDefenseEnemy:
		case SkillTargetType.MaxPhysicalDefenseFriend:
			targetCreatureList.SortByMaxStat(StatType.PhysicalDefense);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.MaxMagicalDefenseAlly:
		case SkillTargetType.MaxMagicalDefenseEnemy:
		case SkillTargetType.MaxMagicalDefenseFriend:
			targetCreatureList.SortByMaxStat(StatType.MagicalDefense);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.MinPhysicalDefenseAlly:
		case SkillTargetType.MinPhysicalDefenseEnemy:
		case SkillTargetType.MinPhysicalDefenseFriend:
			targetCreatureList.SortByMinStat(StatType.PhysicalDefense);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.MinMagicalDefenseAlly:
		case SkillTargetType.MinMagicalDefenseEnemy:
		case SkillTargetType.MinMagicalDefenseFriend:
			targetCreatureList.SortByMinStat(StatType.MagicalDefense);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.HighestTotalDamageAlly:
		case SkillTargetType.HighestTotalDamageEnemy:
		case SkillTargetType.HighestTotalDamageFriend:
		{
			BattleStatisticsManager statisticsManager2 = firedCreature?.BattleInstance.Snapshot.StatisticsManager;
			targetCreatureList.SortByTotalDamage(statisticsManager2, highest: true);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		}
		case SkillTargetType.LowestTotalDamageAlly:
		case SkillTargetType.LowestTotalDamageEnemy:
		case SkillTargetType.LowestTotalDamageFriend:
		{
			BattleStatisticsManager statisticsManager = firedCreature?.BattleInstance.Snapshot.StatisticsManager;
			targetCreatureList.SortByTotalDamage(statisticsManager, highest: false);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		}
		case SkillTargetType.HighestThreatener:
		case SkillTargetType.HighestThreatenerAlly:
			targetCreatureList.SortByThreatPoint(highest: true);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		case SkillTargetType.LowestThreatener:
		case SkillTargetType.LowestThreatenerAlly:
			targetCreatureList.SortByThreatPoint(highest: false);
			targetCreatureList.RemainIfNotZero(totalTargetCount);
			break;
		default:
			VespaDebug.LogError("DO NOT USE THIS FUNCTION WITH targetType=" + skillTargetType);
			break;
		}
	}

	public static SkillRangeResultType IsInSkillRange(BattleCreature fireCreature, BattleCreature targetCreature, int minDistanceMm, int maxDistanceMm)
	{
		if (fireCreature == null || targetCreature == null)
		{
			return SkillRangeResultType.Fail;
		}
		Vector3 position = fireCreature.TransformController.Position;
		Vector3 position2 = targetCreature.TransformController.Position;
		int sizeMm = targetCreature.CreatureData.SizeMm;
		int rangeBonusMm = fireCreature.TransformController.GetRangeBonusMm();
		if (rangeBonusMm > 0)
		{
			maxDistanceMm += rangeBonusMm;
		}
		long num = Vector3.DistanceWithoutY(position, position2);
		if (minDistanceMm != 0)
		{
			int num2 = sizeMm + minDistanceMm;
			if (num < num2)
			{
				return SkillRangeResultType.TooNear;
			}
		}
		if (maxDistanceMm != 0)
		{
			int num3 = sizeMm + maxDistanceMm;
			if (num > num3)
			{
				return SkillRangeResultType.TooFar;
			}
		}
		return SkillRangeResultType.Success;
	}

	public BattleCreatureList GetCreatureListOfAngle(Vector3 position, Vector3 direction, BattleCreatureList targetCreatures, int angle)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < targetCreatures.Count; i++)
		{
			BattleCreature battleCreature = targetCreatures[i];
			Vector3 v = battleCreature.TransformController.Position - position;
			if (v.approximatelyZero || ((angle < 0 || Vector3.Angle(direction, v) <= angle) && (angle >= 0 || Vector3.Angle(-direction, v) <= -angle)))
			{
				battleCreatureList.Add(battleCreature);
			}
		}
		return battleCreatureList;
	}

	public BattleCreatureList GetCreatureListOfWidth(Vector3 position, Vector3 direction, BattleCreatureList targetCreatures, int widthMm)
	{
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		if ((long)widthMm * (long)widthMm > 0)
		{
			for (int i = 0; i < targetCreatures.Count; i++)
			{
				BattleCreature battleCreature = targetCreatures[i];
				if (IsInWidth(battleCreature, position, direction, widthMm))
				{
					battleCreatureList.Add(battleCreature);
				}
			}
		}
		return battleCreatureList;
	}

	public static bool IsInWidth(BattleCreature creature, Vector3 position, Vector3 direction, int widthMm)
	{
		long num = (long)widthMm * (long)widthMm;
		Vector3 position2 = creature.TransformController.Position;
		Vector3 v = position2 - position;
		if (!v.approximatelyZero)
		{
			Vector3 v2 = position + direction * Vector3.DotProduct(v, direction) / 1000L / 1000L;
			if (Vector3.DistanceSqrWithoutY(position2, v2) > num)
			{
				return false;
			}
		}
		return true;
	}

	public BattleCreatureList GetCreatureListOfRange(BattleCreature firedCreature, BattleCreatureList targetCreatures, int minRangeMm, int maxRangeMm)
	{
		if (firedCreature == null)
		{
			return targetCreatures;
		}
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		for (int i = 0; i < targetCreatures.Count; i++)
		{
			BattleCreature battleCreature = targetCreatures[i];
			if (IsInSkillRange(firedCreature, battleCreature, minRangeMm, maxRangeMm) == SkillRangeResultType.Success)
			{
				battleCreatureList.Add(battleCreature);
			}
		}
		return battleCreatureList;
	}

	public Vector3 GetWakeUpPosition(BattleCreature wakeUpCreature, bool wakeUpFromAlly, WakeUpPivotType wakeUpPivotType, int wakeUpOffsetMm)
	{
		Vector3 position = wakeUpCreature.TransformController.Position;
		Vector3 sumPosition = Vector3.zero;
		int sumCount = 0;
		long minMx = long.MaxValue;
		long maxMx = long.MinValue;
		ForEachCreature(wakeUpCreature.TeamController.TeamType, wakeUpFromAlly, delegate(BattleCreature creature)
		{
			if (creature != wakeUpCreature && !creature.IsResting && !creature.StatController.IsDead())
			{
				Vector3 position2 = creature.TransformController.Position;
				sumPosition += position2;
				sumCount++;
				minMx = System.Math.Min(minMx, position2.Mx);
				maxMx = System.Math.Max(maxMx, position2.Mx);
			}
		});
		if (sumCount > 0)
		{
			Vector3 vector = sumPosition / sumCount;
			Vector3 vector2 = wakeUpCreature.TeamController.Team.Direction * (wakeUpFromAlly ? 1 : (-1));
			long num = wakeUpOffsetMm * vector2.Mx / 1000;
			switch (wakeUpPivotType)
			{
			case WakeUpPivotType.Center:
				return new Vector3(num + vector.Mx, 0L, vector.Mz);
			case WakeUpPivotType.Front:
				return new Vector3(num + ((vector2.Mx > 0) ? maxMx : minMx), 0L, vector.Mz);
			case WakeUpPivotType.Back:
				return new Vector3(num + ((vector2.Mx > 0) ? minMx : maxMx), 0L, vector.Mz);
			}
		}
		return position;
	}
}
