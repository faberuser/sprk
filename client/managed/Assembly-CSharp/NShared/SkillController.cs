using System;
using System.Collections.Generic;
using NShared.NOperationRuntime;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class SkillController : SnapshotObject
{
	public const int BaseAttackIntervalMs = 1000;

	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<CooltimeManager> _cooltimeManagerHolder;

	private readonly SnapshotObjectMember<PassiveSkillContainer> _passiveSkillContainerHolder;

	private readonly SnapshotObjectMember<SkillChangerContainer> _skillChangerContainerHolder;

	private readonly SnapshotObjectMember<SkillModifierContainer> _modifierContainerHolder;

	private readonly SnapshotObjectMember<OperationQueue> _operationQueueHolder;

	private readonly SnapshotObjectMember<SkillTargetController> _skillTargetControllerHolder;

	private readonly SnapshotObjectMember<SkillCheckModifierContainer> _skillCheckModifierContainerHolder;

	private readonly SnapshotObjectMember<UsingSkillInfo> _usingSkillInfoHolder;

	private readonly SnapshotObjectMember<long> _baseAttackCheckedTimeMsHolder;

	private readonly SnapshotObjectMember<SkillData> _reservedSkillHolder;

	private readonly SnapshotObjectMember<int> _reservedSkillSlotIndexHolder;

	private readonly SnapshotObjectMember<long> _lastReservedSkillCheckTimeMsHolder;

	private readonly SnapshotObjectMember<long> _lastReservedSoulWeaponSkillCheckTimeMsHolder;

	private readonly SnapshotObjectMember<bool> _autoReservedHolder;

	private readonly SnapshotObjectMember<BattleCreature> _reservedSkillManualTargetHolder;

	private readonly SnapshotObjectMember<BattleCreature> _manualTargetHolder;

	private readonly SnapshotObjectMember<BattleCreature> _lastTargetHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public CooltimeManager CooltimeManager => _cooltimeManagerHolder.Get();

	public PassiveSkillContainer PassiveSkillContainer => _passiveSkillContainerHolder.Get();

	public SkillChangerContainer SkillChangerContainer => _skillChangerContainerHolder.Get();

	public SkillModifierContainer ModifierContainer => _modifierContainerHolder.Get();

	public OperationQueue OperationQueue => _operationQueueHolder.Get();

	public SkillTargetController SkillTargetController => _skillTargetControllerHolder.Get();

	public UsingSkillInfo UsingSkillInfo => _usingSkillInfoHolder.Get();

	public SkillData ReservedSkill => _reservedSkillHolder.Get();

	public int ReservedSkillSlotIndex => _reservedSkillSlotIndexHolder.Get();

	public BattleCreature ReservedSkillManualTarget => _reservedSkillManualTargetHolder.Get();

	public bool AutoReserved => _autoReservedHolder.Get();

	public BattleCreature ManualTarget => _manualTargetHolder.Get();

	public BattleCreature LastTarget => _lastTargetHolder.Get();

	public SkillCheckModifierContainer SkillCheckModifierContainer => _skillCheckModifierContainerHolder.Get();

	public SkillController()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_cooltimeManagerHolder = new SnapshotObjectMember<CooltimeManager>(this);
		_passiveSkillContainerHolder = new SnapshotObjectMember<PassiveSkillContainer>(this);
		_skillChangerContainerHolder = new SnapshotObjectMember<SkillChangerContainer>(this);
		_modifierContainerHolder = new SnapshotObjectMember<SkillModifierContainer>(this);
		_operationQueueHolder = new SnapshotObjectMember<OperationQueue>(this);
		_skillTargetControllerHolder = new SnapshotObjectMember<SkillTargetController>(this);
		_skillCheckModifierContainerHolder = new SnapshotObjectMember<SkillCheckModifierContainer>(this);
		_usingSkillInfoHolder = new SnapshotObjectMember<UsingSkillInfo>(this);
		_baseAttackCheckedTimeMsHolder = new SnapshotObjectMember<long>(this, -10000L);
		_reservedSkillHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_reservedSkillSlotIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_lastReservedSkillCheckTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_autoReservedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_reservedSkillManualTargetHolder = new SnapshotObjectMember<BattleCreature>(this);
		_manualTargetHolder = new SnapshotObjectMember<BattleCreature>(this);
		_lastTargetHolder = new SnapshotObjectMember<BattleCreature>(this);
		_lastReservedSoulWeaponSkillCheckTimeMsHolder = new SnapshotObjectMember<long>(this, -10000L);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_cooltimeManagerHolder.Alloc().Init(owner);
		_passiveSkillContainerHolder.Alloc().Init(owner);
		_skillChangerContainerHolder.Alloc().Init(owner);
		_modifierContainerHolder.Alloc().Init(owner);
		_operationQueueHolder.Alloc().Init(owner);
		_skillTargetControllerHolder.Alloc().Init(owner);
		_skillCheckModifierContainerHolder.Alloc().Init(owner);
	}

	public void Update(int deltaTimeMs)
	{
		if (!Owner.BattleInstance.IsBattleEnabled() || Owner.IsResting)
		{
			return;
		}
		if (!Owner.BattleInstance.IsBattleClosing())
		{
			UpdateBaseAttack();
			UpdateTimerTrigger();
			UpdateReservedSoulWeaponSkill();
			UpdateReservedSkill();
		}
		UpdateCooltime(deltaTimeMs);
		UpdateManualTarget();
		for (int i = 0; i < 5; i++)
		{
			UpdateUsingDurationalSkill((i == 0) ? deltaTimeMs : 0, out var nextSkillActivated);
			if (!nextSkillActivated)
			{
				break;
			}
		}
		UpdateOperationQueue(deltaTimeMs);
		PassiveSkillContainer.Update(deltaTimeMs);
		SkillChangerContainer.Update(deltaTimeMs);
		SkillTargetController.Update(deltaTimeMs);
		SkillCheckModifierContainer.Update(deltaTimeMs);
	}

	private SkillData GetChangedSkillData(SkillData skillData)
	{
		int index = skillData.Index;
		int resultSkillIndex = 0;
		if (SkillChangerContainer.GetChangedSkillIndex(skillData.Index, out resultSkillIndex) && resultSkillIndex != skillData.Index)
		{
			skillData = GameRoot.Instance.SkillDataContainer.GetData(resultSkillIndex);
			if (skillData == null)
			{
				VespaDebug.LogError("Changed skillData is null. originalSkillIndex=" + index + " changedSkillIndex=" + resultSkillIndex);
			}
		}
		return skillData;
	}

	public void SetManualTarget(BattleCreature targetCreature)
	{
		_manualTargetHolder.Set(targetCreature);
	}

	private void UpdateManualTarget()
	{
		BattleCreature battleCreature = _manualTargetHolder.Get();
		if (battleCreature != null)
		{
			if (battleCreature.StatController.IsDead())
			{
				SetManualTarget(null);
			}
			else if (!battleCreature.StateController.CanBeTargetedState(Owner))
			{
				SetManualTarget(null);
			}
		}
	}

	public bool IsConditionActive(BattleCreature firedCreature, SkillData skillData)
	{
		if (skillData == null)
		{
			return false;
		}
		Condition[] activeCondition = skillData.GetActiveCondition();
		if (activeCondition == null || activeCondition.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < activeCondition.Length; i++)
		{
			Condition condition = activeCondition[i];
			string modifiedString = ModifierContainer.GetModifiedString(skillData.Index, 0, OperationFieldType.SkillActiveCondition, 0, condition.ConditionString);
			if (modifiedString != condition.ConditionString)
			{
				condition = Condition.GetCondition(modifiedString);
				if (condition == null)
				{
					return true;
				}
			}
			if (!IsConditionActive(condition))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsConditionActive(Condition condition)
	{
		string attributeName = condition.AttributeName;
		OperationInfo operatinInfo = new OperationInfo
		{
			Creature = Owner
		};
		long testValue = OperationValueReaderFactory.Read(attributeName, null, operatinInfo);
		long checkValue = BaseJsonMarshaler.CastToLong(condition.CompareValue);
		return Condition.CheckCompareResult(condition.CheckType, checkValue, testValue);
	}

	private void UpdateCooltime(int deltaTimeMs)
	{
		if (BattleConstant.StopAllOperationInSoloTime)
		{
			if (Owner.BattleInstance.IsInSoloTime())
			{
				return;
			}
		}
		else if (!CanUpdateTimer())
		{
			return;
		}
		if (Owner.StateController.CanUpdateCooltime())
		{
			CooltimeManager.Update(deltaTimeMs);
		}
	}

	public void InitDungeonePassiveEnvironment()
	{
		CampaignDungeonData dungeonData = Owner.BattleInstance.DungeonData;
		if (dungeonData == null)
		{
			return;
		}
		int[] dungeonPassiveEnvironmentIndices = dungeonData.DungeonPassiveEnvironmentIndices;
		if (dungeonPassiveEnvironmentIndices != null && dungeonPassiveEnvironmentIndices.Length != 0)
		{
			foreach (int key in dungeonPassiveEnvironmentIndices)
			{
				DungeonPassiveEnvironmentData data = GameRoot.Instance.DungeonPassiveEnvironmentDataContainer.GetData(key);
				SetDungeonPassiveEnviroment(data);
			}
		}
	}

	private void SetDungeonPassiveEnviroment(DungeonPassiveEnvironmentData data)
	{
		if (data != null && (data.TeamType == TeamType.All || data.TeamType == Owner.TeamController.TeamType) && data.GroupIndex == Owner.GroupIndex)
		{
			SetDungeonPassiveEnviromentFilter(data.FilterValues);
			SetDungeonPassiveEnviromentState(data.StateValues);
		}
	}

	private void SetDungeonPassiveEnviromentFilter(string[] values)
	{
		if (values != null && values.Length != 0)
		{
			DelegateFilterTargetByState delegateFilterTargetByState = SnapshotObjectPool<DelegateFilterTargetByState>.Alloc();
			delegateFilterTargetByState.OnInit(values);
			SkillTargetController.AddTargetFilter(delegateFilterTargetByState);
		}
	}

	private void SetDungeonPassiveEnviromentState(string[] values)
	{
		if (values == null || values.Length == 0)
		{
			return;
		}
		string valueString = ArgParser.GetValueString(values, 0);
		int valueInt = ArgParser.GetValueInt(values, 1);
		if (!string.IsNullOrEmpty(valueString) && valueInt != 0)
		{
			StateData data = GameRoot.Instance.StateDataContainer.GetData(valueString);
			if (data != null)
			{
				Owner.StateController.AddState(data, valueInt, Owner);
			}
		}
	}

	public void InitPassiveSkill()
	{
		bool flag = !Owner.CreateInfo.IsAi;
		List<IExtraSkillData> list = null;
		for (int i = 1; i <= 5; i++)
		{
			int num = Owner.CreateInfo.HeroInfo.GetSkillLevel(i, flag);
			if (num == 0)
			{
				break;
			}
			int skillIndexBySlot = Owner.CreatureData.GetSkillIndexBySlot(i);
			if (skillIndexBySlot == 0)
			{
				continue;
			}
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot);
			if (data == null)
			{
				VespaDebug.LogError("SkillData not found. skillIndex=" + skillIndexBySlot);
				continue;
			}
			if (data.TriggerType != SkillTriggerType.None)
			{
				if (flag)
				{
					int skillSlotAddedLevel = Owner.StatController.GetSkillSlotAddedLevel(i);
					num += skillSlotAddedLevel;
				}
				if (list == null)
				{
					list = new List<IExtraSkillData>();
				}
				list.Add(new DefaultExtraSkillData
				{
					SkillIndex = skillIndexBySlot,
					SkillLevel = num
				});
			}
			int skillExtend = Owner.CreateInfo.HeroInfo.GetSkillExtend(i);
			if (skillExtend == 0)
			{
				continue;
			}
			int[] skillExtend2 = Owner.CreatureData.GetSkillExtend(i);
			if (skillExtend2 == null || skillExtend2.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < skillExtend2.Length && j < skillExtend; j++)
			{
				int skillIndex = skillExtend2[j];
				if (list == null)
				{
					list = new List<IExtraSkillData>();
				}
				list.Add(new DefaultExtraSkillData
				{
					SkillIndex = skillIndex,
					SkillLevel = 1
				});
			}
		}
		TranscendSkillPageInfo transcendSkillPageInfo = Owner.CreateInfo.HeroInfo.GetTranscendSkillPageInfo();
		if (transcendSkillPageInfo != null)
		{
			foreach (CreatureData.TranscendSkill transcendSkillData in transcendSkillPageInfo.TranscendSkillDatas)
			{
				if (transcendSkillData != null)
				{
					if (list == null)
					{
						list = new List<IExtraSkillData>();
					}
					list.Add(new DefaultExtraSkillData
					{
						SkillIndex = transcendSkillData.TranscendSkillIndex,
						SkillLevel = 1
					});
				}
			}
		}
		List<ExtraSkillInfo> list2 = new List<ExtraSkillInfo>();
		list2.AddRangeFromIndex(Owner.CreatureData.ExtraSkill);
		if (Owner.CreateInfo.AddExtraSkill != null)
		{
			list2.AddRange(Owner.CreateInfo.AddExtraSkill);
		}
		for (int k = 0; k < list2.Count; k++)
		{
			ExtraSkillInfo extraSkillInfo = list2[k];
			SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(extraSkillInfo.SkillIndex);
			if (data2 != null && data2.TriggerType != SkillTriggerType.None)
			{
				if (list == null)
				{
					list = new List<IExtraSkillData>();
				}
				int num2 = extraSkillInfo.SkillLevel;
				if (num2 == 0)
				{
					num2 = Owner.CreateInfo.HeroInfo.Level;
				}
				list.Add(new DefaultExtraSkillData
				{
					SkillIndex = extraSkillInfo.SkillIndex,
					SkillLevel = num2
				});
			}
		}
		InitCustomSkills(list);
	}

	public void InitGuildSkill()
	{
		InitCustomSkills(Owner.CreateInfo.GuildSkills);
	}

	public void InitAccountBuff()
	{
		InitCustomSkills(Owner.CreateInfo.AccountBuffs);
	}

	public void InitClassBuff()
	{
		ClassBuffDataBase[] classBuffDataBases = Owner.CreateInfo.ClassBuffDataBases;
		if (classBuffDataBases == null || classBuffDataBases.Length == 0)
		{
			return;
		}
		List<IExtraSkillData> list = new List<IExtraSkillData>();
		list.Clear();
		ClassBuffDataBase[] array = classBuffDataBases;
		foreach (ClassBuffDataBase classBuffDataBase in array)
		{
			if (classBuffDataBase != null && Owner.CreatureData != null && Owner.CreatureData.TagType != CreatureTagType.None && Owner.CreatureData.TagType != CreatureTagType.End && Owner.CreatureData.TagType == (CreatureTagType)classBuffDataBase.CreatureType)
			{
				list.Add(classBuffDataBase);
			}
		}
		InitCustomSkills(list);
	}

	public void InitGuildArenaBuff()
	{
		InitCustomSkills(Owner.CreateInfo.GuildArenaBuffDatasBases);
	}

	public void InitSoulWeaponSkill()
	{
		CachedHeroInfo heroInfo = Owner.CreateInfo.HeroInfo;
		if (heroInfo == null || heroInfo.GetSoulWeaponInfo() == null)
		{
			return;
		}
		int[] soulWeaponActiveConditionSkillIndices = heroInfo.GetSoulWeaponActiveConditionSkillIndices();
		List<int> list = null;
		if (soulWeaponActiveConditionSkillIndices != null && soulWeaponActiveConditionSkillIndices.Length != 0)
		{
			if (list == null)
			{
				list = new List<int>();
			}
			list.AddRange(soulWeaponActiveConditionSkillIndices);
		}
		int[] soulWeaponAddSkillIndices = heroInfo.GetSoulWeaponAddSkillIndices();
		if (soulWeaponAddSkillIndices != null && soulWeaponAddSkillIndices.Length != 0)
		{
			if (list == null)
			{
				list = new List<int>();
			}
			list.AddRange(soulWeaponAddSkillIndices);
		}
		if (list == null || list.Count == 0)
		{
			return;
		}
		List<IExtraSkillData> list2 = new List<IExtraSkillData>();
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i];
			if (GameRoot.Instance.SkillDataContainer.GetData(num) != null)
			{
				list2.Add(new DefaultExtraSkillData
				{
					SkillIndex = num,
					SkillLevel = 1
				});
			}
		}
		InitCustomSkills(list2);
	}

	public void InitOrdealArenaSkill()
	{
		InitCustomSkills(Owner.CreateInfo.OrdealArenaSkills);
	}

	public void InitAffixSkill()
	{
		int uid = Owner.Uid;
		TeamType teamType = Owner.TeamController.TeamType;
		int index = Owner.CreatureData.Index;
		List<IExtraSkillData> list = new List<IExtraSkillData>();
		List<IExtraSkillData> skillAffixDataBase = Owner.BattleInstance.AffixContainer.GetSkillAffixDataBase(teamType, index, uid);
		if (skillAffixDataBase != null && skillAffixDataBase.Count > 0)
		{
			list.AddRange(skillAffixDataBase);
		}
		List<IExtraSkillData> flaskSkillList = Owner.BattleInstance.ShardFlaskManager.GetFlaskSkillList(teamType, Owner);
		if (flaskSkillList != null && flaskSkillList.Count > 0)
		{
			list.AddRange(flaskSkillList);
		}
		InitCustomSkills(list);
	}

	private void InitCustomSkills(IList<IExtraSkillData> extraSkillDatas)
	{
		if (extraSkillDatas == null)
		{
			return;
		}
		for (int i = 0; i < extraSkillDatas.Count; i++)
		{
			IExtraSkillData extraSkillData = extraSkillDatas[i];
			int skillIndex = extraSkillData.SkillIndex;
			if (skillIndex != 0 && extraSkillData.CheckActiveCondition(Owner.BattleInstance.DungeonData))
			{
				int skillLevel = extraSkillData.SkillLevel;
				SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
				if (data == null)
				{
					Action<object> logError = VespaDebug.LogError;
					string[] obj = new string[6]
					{
						"Failed to find SkillData : CreatureIndex=",
						Owner.CreatureData.Index.ToString(),
						" Index=",
						null,
						null,
						null
					};
					int num = skillIndex;
					obj[3] = num.ToString();
					obj[4] = " Level=";
					obj[5] = skillLevel.ToString();
					logError(string.Concat(obj));
				}
				else if (data.TriggerType == SkillTriggerType.None)
				{
					UseSkill(data, new SkillUseOption(checkOnly: false, checkLevel: false), null, skillLevel);
				}
				else
				{
					PassiveSkillContainer.Register(data, skillLevel);
				}
			}
		}
	}

	private void UpdateTimerTrigger()
	{
		if (Owner.IsDummyCreature() || !CanPassiveSkill() || Owner.BattleInstance.IsInSoloTime())
		{
			return;
		}
		OnTimer onTimer = SnapshotObjectPool<OnTimer>.Alloc();
		onTimer.TriggerCreature = Owner;
		onTimer.TargetCreatures = new BattleCreature[1] { Owner };
		int deltaTimeMs = Owner.BattleInstance.DeltaTimeMs;
		using TemporaryList<SkillData> temporaryList = PassiveSkillContainer.UpdateTriggeredTimerSkillList(deltaTimeMs);
		for (int i = 0; i < temporaryList.Count; i++)
		{
			SkillData skillData = temporaryList[i];
			UseSkill(skillData, new SkillUseOption(checkOnly: false, checkLevel: false), onTimer);
		}
	}

	public void ApplyFiredPassiveSkillToCreature(BattleCreature newCreature)
	{
		using TemporaryList<PassiveSkillInfo> temporaryList = PassiveSkillContainer.GetPassiveSkillsByTrigger(SkillTriggerType.OnStart);
		if (temporaryList.Count == 0)
		{
			return;
		}
		OnStart onStart = SnapshotObjectPool<OnStart>.Alloc();
		onStart.TriggerCreature = Owner;
		for (int i = 0; i < temporaryList.Count; i++)
		{
			SkillData skillData = temporaryList[i].SkillData;
			BattleCreatureList battleCreatureList = Owner.BattleInstance.CreatureContainer.FindSkillTargetCreature(Owner, skillData, checkOnly: false, onStart);
			if (battleCreatureList != null && battleCreatureList.Contains(newCreature))
			{
				int userSkillLevel = Owner.GetUserSkillLevel(skillData.Index);
				BattleCreature[] targetCreatures = new BattleCreature[1] { newCreature };
				FireSingleSkill(skillData, skillData, null, userSkillLevel, onStart, targetCreatures, null);
			}
		}
	}

	private bool CanFireBaseAttack()
	{
		if (Owner.StatController.IsDead())
		{
			return false;
		}
		if (Owner.BattleInstance.IsBattleClosingOrDisabled())
		{
			return false;
		}
		if (UsingSkillInfo != null)
		{
			return false;
		}
		if (Owner.TransformController.IsManualMoving())
		{
			return false;
		}
		return true;
	}

	public void BeginAutoSkillInitialCooltime()
	{
		using TemporaryList<PassiveSkillInfo> temporaryList = PassiveSkillContainer.GetPassiveSkillsByTrigger(SkillTriggerType.OnAuto);
		for (int i = 0; i < temporaryList.Count; i++)
		{
			SkillData skillData = temporaryList[i].SkillData;
			if (skillData != null)
			{
				int num = skillData.InitialCoolTimeMs / 2;
				int max = num * 3;
				int durationTimeMs = Owner.NextRandom(num, max, "BeginAutoSkillInitialCooltime " + skillData);
				CooltimeManager.BeginCooltime(skillData.Index, durationTimeMs, applyCoolTimeModifier: true);
			}
		}
	}

	public void BeginSkillInitialCooltime()
	{
		List<int> skillIndexList = Owner.GetSkillIndexList(includeExtra: false);
		for (int i = 0; i < skillIndexList.Count; i++)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndexList[i]);
			if (data != null && data.InitialCoolTimeMs > 0)
			{
				CooltimeManager.BeginCooltime(skillIndexList[i], data.InitialCoolTimeMs, applyCoolTimeModifier: true);
			}
		}
	}

	private void UpdateBaseAttack()
	{
		if (Owner.IsDummyCreature())
		{
			return;
		}
		int baseSkillIndex = Owner.CreatureData.BaseSkillIndex;
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(baseSkillIndex);
		if (data == null)
		{
			return;
		}
		long currentTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs();
		if (!CanFireBaseAttack())
		{
			_baseAttackCheckedTimeMsHolder.Set(currentTimeMs);
			return;
		}
		BattleCreature[] targetCreatures;
		SkillUseResult skillUseResult = CanUseSkill(data, new SkillUseOption(checkOnly: true, checkLevel: false), out targetCreatures);
		if ((skillUseResult == SkillUseResult.Success || skillUseResult == SkillUseResult.TargetTooFar) && targetCreatures != null && targetCreatures.Length != 0)
		{
			long num = currentTimeMs - _baseAttackCheckedTimeMsHolder.Get();
			int num2 = Owner.StatController.MultiplyAttackSpeed((int)num);
			int coolTimeMs = data.GetCoolTimeMs(Owner);
			if (num2 < coolTimeMs)
			{
				ApproachToBaseAttack(data, targetCreatures[0]);
			}
			else if (!FireAutoSkill() && !ApproachToBaseAttack(data, targetCreatures[0]) && (!data.GetSkillTargetType(Owner).IsRandom() || CanUseSkill(data, new SkillUseOption(checkOnly: false, checkLevel: false), out targetCreatures) == SkillUseResult.Success))
			{
				int userSkillLevel = Owner.GetUserSkillLevel(baseSkillIndex);
				FireSkill(data, userSkillLevel, null, targetCreatures, null, null, isIgnoreCoolTime: false, isIgnoreSoloTime: false, isCanBeCancel: false, isBaseAttack: true);
				_baseAttackCheckedTimeMsHolder.Set(currentTimeMs);
			}
		}
	}

	private bool CanPassiveSkill()
	{
		return Owner.StateController.CanUsePassiveSkill();
	}

	private bool FireAutoSkill()
	{
		SkillData skillData = null;
		BattleCreature[] targetCreatures = null;
		int num = 0;
		if (!CanPassiveSkill())
		{
			return false;
		}
		using (TemporaryList<PassiveSkillInfo> temporaryList = PassiveSkillContainer.GetPassiveSkillsByTrigger(SkillTriggerType.OnAuto))
		{
			for (int i = 0; i < temporaryList.Count; i++)
			{
				SkillData skillData2 = temporaryList[i].SkillData;
				if (CanUseSkill(skillData2, new SkillUseOption(), out var targetCreatures2) == SkillUseResult.Success)
				{
					num++;
					if (skillData == null)
					{
						skillData = skillData2;
						targetCreatures = targetCreatures2;
					}
					else if (Owner.NextRandom(100, "FireAutoSkill " + skillData) <= 100 / num)
					{
						skillData = skillData2;
						targetCreatures = targetCreatures2;
					}
				}
			}
		}
		if (skillData != null)
		{
			int userSkillLevel = Owner.GetUserSkillLevel(skillData.Index);
			userSkillLevel = ((userSkillLevel <= 0) ? 1 : userSkillLevel);
			FireSkill(skillData, userSkillLevel, null, targetCreatures);
			return true;
		}
		return false;
	}

	private bool ApproachToBaseAttack(SkillData baseSkillData, BattleCreature targetCreature)
	{
		if (targetCreature == null)
		{
			return false;
		}
		int maxRangeMm = baseSkillData.GetMaxRangeMm(Owner);
		if (BattleCreatureContainer.IsInSkillRange(Owner, targetCreature, 0, maxRangeMm) == SkillRangeResultType.Success)
		{
			Owner.TransformController.ViewPosition(targetCreature.TransformController.Position);
			return false;
		}
		if (Owner.CreatureData.IsStaticPosition())
		{
			return true;
		}
		int num = targetCreature.CreatureData.SizeMm + maxRangeMm;
		int rangeBonusMm = Owner.TransformController.GetRangeBonusMm();
		if (rangeBonusMm > 0)
		{
			num += rangeBonusMm;
		}
		Owner.TransformController.MoveToCreature(targetCreature, num, 0, 0L);
		return true;
	}

	public SkillUseResult UseSkill(SkillData skillData, SkillUseOption option, ISkillTriggerInfo triggerInfo, int forcedLevel = 0, bool isActiveSkill = false, int skillSlotIndex = 0)
	{
		skillData = GetChangedSkillData(skillData);
		if (skillData == null)
		{
			return SkillUseResult.Fail;
		}
		BattleCreature[] targetCreatures = null;
		SkillUseResult skillUseResult = CanUseSkill(skillData, option, out targetCreatures);
		if (skillUseResult != SkillUseResult.Success)
		{
			return skillUseResult;
		}
		if (option != null && option.CheckMp)
		{
			int mp = skillData.GetMp(Owner);
			Owner.StatController.ConsumeMp(mp, 0L);
		}
		if (option != null && option.CheckReqHpRatio)
		{
			int modifiedInt = ModifierContainer.GetModifiedInt(skillData.Index, 0, OperationFieldType.SkillReqHpRatio, 0, skillData.ReqHpRatio);
			Owner.StatController.ConsumeHpRatio(modifiedInt, 0L);
		}
		if (targetCreatures.Length != 0 && skillData.IsDurationalSkill())
		{
			Vector3 position = targetCreatures[0].TransformController.Position;
			Owner.TransformController.ViewPosition(position);
		}
		int num = forcedLevel;
		if (num == 0)
		{
			num = Owner.GetUserSkillLevel(skillData.Index);
			num = ((num <= 0) ? 1 : num);
		}
		FireSkill(skillData, num, triggerInfo, targetCreatures, null, option?.ManualTarget, option?.IsIgnoreCoolTime ?? false, option?.IsIgnoreSoloTime ?? false, option?.IsCanBeCancel ?? false, isBaseAttack: false, checkChangedSkill: false, skillSlotIndex);
		if (isActiveSkill)
		{
			BattleCreatureList aliveCreatures = Owner.BattleInstance.CreatureContainer.GetAliveCreatures();
			for (int i = 0; i < aliveCreatures.Count; i++)
			{
				OnActiveSkillTrigger onActiveSkillTrigger = SnapshotObjectPool<OnActiveSkillTrigger>.Alloc();
				onActiveSkillTrigger.TriggerCreature = Owner;
				onActiveSkillTrigger.TargetCreatures = targetCreatures;
				onActiveSkillTrigger.SkillIndex = skillData.Index;
				onActiveSkillTrigger.SetSkillTargetType(aliveCreatures[i], Owner);
				aliveCreatures[i].SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onActiveSkillTrigger);
			}
		}
		return SkillUseResult.Success;
	}

	public bool CanFireNextSkill()
	{
		if (UsingSkillInfo == null)
		{
			return false;
		}
		if (UsingSkillInfo.FiredSkillData == null)
		{
			return false;
		}
		SkillData skillData = UsingSkillInfo.SkillData;
		if (skillData == null || skillData.NextIndex == 0)
		{
			return false;
		}
		int nextIndex = skillData.NextIndex;
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(nextIndex);
		if (data == null || data.TriggerType != SkillTriggerType.NextActiveSkill)
		{
			return false;
		}
		return true;
	}

	public bool FireNextSkill()
	{
		if (!CanFireNextSkill())
		{
			return false;
		}
		UsingSkillInfo.NextActivated = true;
		return true;
	}

	public bool UseAvailableSkill()
	{
		foreach (int skillIndex in Owner.GetSkillIndexList(includeExtra: false))
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
			if (data == null)
			{
				VespaDebug.LogError("skill data not found : " + skillIndex);
			}
			else if (data.TriggerType == SkillTriggerType.None && UseSkill(data, new SkillUseOption(), null) == SkillUseResult.Success)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInSoloTime()
	{
		if (UsingSkillInfo != null && UsingSkillInfo.IsSoloTime())
		{
			return true;
		}
		return false;
	}

	public bool IsInSoloSkill()
	{
		if (UsingSkillInfo != null && UsingSkillInfo.SkillData.GetSoloTimeMs(UsingSkillInfo.IsIgnoreSoloTime) > 0)
		{
			return true;
		}
		if (UsingSkillInfo != null && UsingSkillInfo.FiredSkillData.GetSoloTimeMs(UsingSkillInfo.IsIgnoreSoloTime) > 0)
		{
			return true;
		}
		return false;
	}

	public bool CanUpdateTimer()
	{
		if (IsInSoloSkill())
		{
			return true;
		}
		if (!Owner.BattleInstance.IsInSoloTime())
		{
			return true;
		}
		return false;
	}

	public void UseSkillByTrigger_ReleaseIfNotUsed<T>(T triggerInfo) where T : class, ISkillTriggerInfo, new()
	{
		UseSkillByTriggerInternal(triggerInfo, out var oTriggerUsed);
		if (!oTriggerUsed)
		{
			SnapshotObjectPool<T>.ReleaseImmediately(triggerInfo);
		}
	}

	public void UseSkillByTrigger(ISkillTriggerInfo triggerInfo)
	{
		UseSkillByTriggerInternal(triggerInfo, out var _);
	}

	private void UseSkillByTriggerInternal(ISkillTriggerInfo triggerInfo, out bool oTriggerUsed)
	{
		if (!CanPassiveSkill())
		{
			oTriggerUsed = false;
			return;
		}
		if (triggerInfo == null)
		{
			oTriggerUsed = false;
			return;
		}
		bool flag = triggerInfo.TriggerType != SkillTriggerType.OnStart;
		if (flag && (!Owner.BattleInstance.IsBattleEnabled() || Owner.IsResting))
		{
			oTriggerUsed = false;
			return;
		}
		SkillTriggerType triggerType = triggerInfo.TriggerType;
		_ = UsingSkillInfo;
		if (triggerType == SkillTriggerType.None)
		{
			VespaDebug.LogError("TriggerType is none");
		}
		using TemporaryList<PassiveSkillInfo> temporaryList = PassiveSkillContainer.GetPassiveSkillsByTrigger(triggerType);
		if (temporaryList.Count == 0)
		{
			oTriggerUsed = false;
			return;
		}
		oTriggerUsed = false;
		for (int i = 0; i < temporaryList.Count; i++)
		{
			PassiveSkillInfo passiveSkillInfo = temporaryList[i];
			if (passiveSkillInfo == null)
			{
				continue;
			}
			SkillData skillData = passiveSkillInfo.SkillData;
			if (skillData == null || !triggerInfo.CheckTriggerValue(Owner, skillData) || !CheckTriggerRatio(skillData, "UseSkillByTrigger "))
			{
				continue;
			}
			int num = ((passiveSkillInfo.SkillLevel != 0) ? passiveSkillInfo.SkillLevel : Owner.GetUserSkillLevel(skillData.Index));
			if (num > 0)
			{
				BattleCreature[] targetCreatures = Owner.BattleInstance.CreatureContainer.FindSkillTargetCreature(Owner, skillData, checkOnly: false, triggerInfo)?.ToArray();
				if (CanUseSkill(skillData, new SkillUseOption(checkOnly: true, checkLevel: false, checkMp: true, isEventSkill: false, null, flag), out targetCreatures, targetCreatures) == SkillUseResult.Success)
				{
					oTriggerUsed = true;
					FireSkill(skillData, num, triggerInfo, targetCreatures);
				}
			}
		}
	}

	public bool CheckTriggerRatio(SkillData skillData, string randomDesc)
	{
		int num = Owner.NextRandom(1000, randomDesc + skillData);
		int modifiedInt = ModifierContainer.GetModifiedInt(skillData.Index, 0, OperationFieldType.SkillTriggerRatio, 0, skillData.TriggerRatio);
		return num < modifiedInt;
	}

	private void FireSkill(SkillData skillData, int skillLevel, ISkillTriggerInfo triggerInfo, BattleCreature[] targetCreatures, SkillData firedSkillData = null, BattleCreature manualTarget = null, bool isIgnoreCoolTime = false, bool isIgnoreSoloTime = false, bool isCanBeCancel = false, bool isBaseAttack = false, bool checkChangedSkill = true, int skillSlotIndex = 0)
	{
		if (checkChangedSkill)
		{
			skillData = GetChangedSkillData(skillData);
		}
		if (skillData == null)
		{
			return;
		}
		if (skillData.FollowLevelIndex != 0)
		{
			int userSkillLevel = Owner.GetUserSkillLevel(skillData.FollowLevelIndex);
			if (userSkillLevel != 0)
			{
				skillLevel = userSkillLevel;
			}
		}
		BattleCreature battleCreature = ((targetCreatures != null && targetCreatures.Length != 0) ? targetCreatures[0] : null);
		Vector3 position = Owner.TransformController.Position;
		Vector3 vector = battleCreature?.TransformController.Position ?? position;
		long num = Vector3.Distance(position, vector);
		int durationMs = skillData.GetDurationMs((int)num);
		bool flag = skillData.IsDurationalSkill();
		if (flag)
		{
			StopUsingDurationalSkill(isCancel: true);
			Owner.TransformController.StopMove(raiseEvent: false);
		}
		using TemporaryList<DelegateStateModifier> temporaryList = default(TemporaryList<DelegateStateModifier>);
		string[] stateType = skillData.StateType;
		if (stateType != null && flag)
		{
			string[] array = stateType;
			foreach (string text in array)
			{
				StateData data = GameRoot.Instance.StateDataContainer.GetData(text);
				if (data == null)
				{
					VespaDebug.LogError("Fire skill state not found : " + text);
					continue;
				}
				DelegateStateModifier delegateStateModifier = new DelegateStateModifier
				{
					StateData = data,
					FiredCreature = Owner
				};
				temporaryList.Add(delegateStateModifier);
				Owner.StateController.AddModifier(delegateStateModifier);
			}
		}
		HashSet<string> clearStateTypeList = skillData.GetClearStateTypeList(Owner);
		if (clearStateTypeList != null)
		{
			foreach (string item in clearStateTypeList)
			{
				Owner.StateController.ClearState(item);
			}
		}
		_lastTargetHolder.Set(battleCreature);
		long currentTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs();
		UsingSkillInfo usingSkillInfo = SnapshotObjectPool<UsingSkillInfo>.Alloc();
		usingSkillInfo.Init();
		usingSkillInfo.SkillData = skillData;
		usingSkillInfo.FiredSkillData = firedSkillData ?? skillData;
		usingSkillInfo.SkillLevel = skillLevel;
		usingSkillInfo.TriggerInfo = triggerInfo;
		usingSkillInfo.UsedTargetCreatures = targetCreatures;
		usingSkillInfo.FiredTargetCreatures = targetCreatures;
		usingSkillInfo.UsedTargetPosition = vector;
		usingSkillInfo.StartedTimeMs = currentTimeMs;
		usingSkillInfo.DurationTimeMs = durationMs;
		usingSkillInfo.TickIndex = 0;
		usingSkillInfo.OnAttackEventApplied = false;
		usingSkillInfo.StateModifiers = temporaryList.ToArray();
		usingSkillInfo.ManualTarget = manualTarget;
		usingSkillInfo.IsIgnoreCoolTime = isIgnoreCoolTime;
		usingSkillInfo.IsIgnoreSoloTime = isIgnoreSoloTime;
		usingSkillInfo.IsCanBeCancel = isCanBeCancel;
		usingSkillInfo.IsBaseAttack = isBaseAttack;
		usingSkillInfo.SkillSlotIndex = skillSlotIndex;
		usingSkillInfo.InitModified(Owner);
		BeginFireSkill beginFireSkill = new BeginFireSkill();
		beginFireSkill.CreatureUid = Owner.Uid;
		beginFireSkill.SkillId = skillData.Index;
		beginFireSkill.FiredSkillId = firedSkillData?.Index ?? skillData.Index;
		beginFireSkill.FirePosition = Owner.TransformController.Position;
		beginFireSkill.FirstTargetCreatureUid = battleCreature?.Uid ?? 0;
		beginFireSkill.TargetPosition = vector;
		beginFireSkill.DurationTimeMs = durationMs;
		beginFireSkill.IsIgnoreSoloTime = isIgnoreSoloTime;
		beginFireSkill.SkillSlotIndex = skillSlotIndex;
		Owner.RaiseBattleEvent(beginFireSkill);
		if (flag)
		{
			_usingSkillInfoHolder.Set(usingSkillInfo);
		}
		if (skillData.TickTime == null || skillData.TickTime.Length == 0)
		{
			FireSingleSkill(skillData, firedSkillData, null, skillLevel, triggerInfo, targetCreatures, usingSkillInfo);
			if (!isIgnoreCoolTime)
			{
				StartCooltime(skillData);
			}
		}
		if (!flag && skillData.NextIndex > 0)
		{
			UseNextSkill(usingSkillInfo);
		}
	}

	public void FireSingleSkill(SkillData skillData, SkillData firedSkillData, SkillTickTime tickTime, int skillLevel, ISkillTriggerInfo triggerInfo, BattleCreature[] targetCreatures, UsingSkillInfo usingSkillInfo, Vector3 forcedFiredPosition = default(Vector3))
	{
		if (skillData == null || targetCreatures == null || targetCreatures.Length == 0)
		{
			return;
		}
		FireSkill fireSkill = new FireSkill();
		Owner.QueueBattleEvent(fireSkill);
		ApplySkillOperationToTarget(skillData, firedSkillData ?? skillData, tickTime, skillLevel, triggerInfo, targetCreatures, usingSkillInfo, forcedFiredPosition, out var skillOperationInfoList, out var addedOperationInfoList);
		fireSkill.CreatureUid = Owner.Uid;
		fireSkill.SkillId = skillData.Index;
		fireSkill.FiredSkillId = firedSkillData?.Index ?? skillData.Index;
		fireSkill.FirstTargetCreatureUid = targetCreatures[0].Uid;
		fireSkill.TargetPosition = usingSkillInfo?.UsedTargetPosition ?? Vector3.zero;
		fireSkill.TickIndex = tickTime?.TickIndex ?? 0;
		fireSkill.OperationInfos = skillOperationInfoList?.ToArray();
		Owner.RaiseBattleEvent(fireSkill);
		if (triggerInfo == null)
		{
			OnFireSkill onFireSkill = SnapshotObjectPool<OnFireSkill>.Alloc();
			onFireSkill.TriggerCreature = Owner;
			onFireSkill.TargetCreatures = targetCreatures;
			onFireSkill.SkillIndex = skillData.Index;
			Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onFireSkill);
		}
		if (usingSkillInfo != null)
		{
			if (addedOperationInfoList != null)
			{
				usingSkillInfo.OperationInfos.AddRange(addedOperationInfoList);
			}
			if (usingSkillInfo.DurationTimeMs <= 0)
			{
				RaiseEndFireSkill(usingSkillInfo, isCancel: false);
			}
		}
	}

	private void ApplySkillOperationToTarget(SkillData skillData, SkillData firedSkillData, SkillTickTime tickTime, int skillLevel, ISkillTriggerInfo triggerInfo, BattleCreature[] targetCreatures, UsingSkillInfo usingSkillInfo, Vector3 forcedFiredPosition, out List<FireSkill.OperationInfo> skillOperationInfoList, out List<OperationInfo> addedOperationInfoList)
	{
		skillOperationInfoList = null;
		addedOperationInfoList = null;
		if (skillData == null)
		{
			return;
		}
		List<OperationData> list = null;
		List<OperationData> list2 = null;
		for (int i = 1; i <= 8; i++)
		{
			OperationData operationDataUnsafe = skillData.GetOperationDataUnsafe(i);
			if (operationDataUnsafe == null)
			{
				continue;
			}
			int reqSkillIndex = operationDataUnsafe.ReqSkillIndex;
			if (reqSkillIndex != 0 && Owner.GetUserSkillLevel(reqSkillIndex) <= 0)
			{
				continue;
			}
			if (operationDataUnsafe.HasFlag(OperationFlag.Disabled))
			{
				if (!ModifierContainer.GetModifiedBool(skillData.Index, i, OperationFieldType.Enabled, 0))
				{
					continue;
				}
			}
			else if (ModifierContainer.GetModifiedBool(skillData.Index, i, OperationFieldType.Disabled, 0))
			{
				continue;
			}
			if (tickTime == null || tickTime.OperationNo == null || tickTime.OperationNo.Contains(i))
			{
				if (operationDataUnsafe.HasFlag(OperationFlag.IgnoreRetarget))
				{
					list2 = list2 ?? new List<OperationData>();
					list2.Add(operationDataUnsafe);
				}
				else
				{
					list = list ?? new List<OperationData>();
					list.Add(operationDataUnsafe);
				}
			}
		}
		if (list != null || list2 != null)
		{
			skillOperationInfoList = new List<FireSkill.OperationInfo>();
			addedOperationInfoList = new List<OperationInfo>();
			ApplyValidSkillOperationToTarget(list, skillData, firedSkillData, tickTime, skillLevel, triggerInfo, targetCreatures, usingSkillInfo, forcedFiredPosition, ref skillOperationInfoList, ref addedOperationInfoList);
			if (usingSkillInfo != null)
			{
				ApplyValidSkillOperationToTarget(list2, skillData, firedSkillData, tickTime, skillLevel, triggerInfo, usingSkillInfo.UsedTargetCreatures, usingSkillInfo, forcedFiredPosition, ref skillOperationInfoList, ref addedOperationInfoList);
			}
		}
	}

	private void ApplyValidSkillOperationToTarget(List<OperationData> validOperationDataList, SkillData skillData, SkillData firedSkillData, SkillTickTime tickTime, int skillLevel, ISkillTriggerInfo triggerInfo, BattleCreature[] targetCreatures, UsingSkillInfo usingSkillInfo, Vector3 forcedFiredPosition, ref List<FireSkill.OperationInfo> skillOperationInfoList, ref List<OperationInfo> addedOperationInfoList)
	{
		if (validOperationDataList == null || validOperationDataList.Count == 0 || targetCreatures == null || targetCreatures.Length == 0)
		{
			return;
		}
		bool isTeamInverted = Owner.TeamController.IsTeamInverted();
		bool isIgnoreSoloTime = usingSkillInfo?.IsIgnoreSoloTime ?? false;
		Vector3 vector = usingSkillInfo?.UsedTargetPosition ?? Vector3.zero;
		int soloTimeMs = usingSkillInfo?.GetRemainSoloTimeMs() ?? 0;
		Vector3 firedPosition = (forcedFiredPosition.approximatelyZero ? Owner.TransformController.Position : forcedFiredPosition);
		bool isBaseAttack = usingSkillInfo?.IsBaseAttack ?? false;
		foreach (BattleCreature battleCreature in targetCreatures)
		{
			using TemporaryList<ModifiedFiredOperationInfo> temporaryList = default(TemporaryList<ModifiedFiredOperationInfo>);
			for (int j = 0; j < validOperationDataList.Count; j++)
			{
				OperationData operationData = validOperationDataList[j];
				if (operationData == null)
				{
					continue;
				}
				BattleCreature battleCreature2 = battleCreature;
				BattleCreature[] array = targetCreatures;
				if (operationData.HasFlag(OperationFlag.RetargetOnReserve))
				{
					BattleCreatureList battleCreatureList = Owner.BattleInstance.CreatureContainer.FindSkillTargetCreature(Owner, skillData, checkOnly: false, null);
					if (battleCreatureList != null && battleCreatureList.Count > 0)
					{
						battleCreature2 = battleCreatureList[0];
						array = battleCreatureList.ToArray();
					}
				}
				Vector3 firedTargetPosition = battleCreature2?.TransformController.Position ?? vector;
				FiredOperationInfo firedOperationInfo = SnapshotObjectPool<FiredOperationInfo>.Alloc();
				firedOperationInfo.OperationData = operationData;
				firedOperationInfo.SkillData = skillData;
				firedOperationInfo.FiredSkillData = firedSkillData;
				firedOperationInfo.SkillLevel = skillLevel;
				firedOperationInfo.SoloTimeMs = soloTimeMs;
				firedOperationInfo.FiredCreature = Owner;
				firedOperationInfo.TargetCreature = battleCreature2;
				firedOperationInfo.TargetCreatures = array;
				firedOperationInfo.FiredPosition = firedPosition;
				firedOperationInfo.FiredTargetPosition = firedTargetPosition;
				firedOperationInfo.UsedTargetPosition = vector;
				firedOperationInfo.TriggerInfo = triggerInfo;
				firedOperationInfo.TickIndex = tickTime?.TickIndex ?? 0;
				firedOperationInfo.IsBaseAttack = isBaseAttack;
				firedOperationInfo.Init();
				int fixedDelayMs = 0;
				int distanceDelayMs = 0;
				int distanceFixedDelayMs = 0;
				if (!ModifyFiredOperation(operationData.OperationNo, firedOperationInfo, usingSkillInfo, out fixedDelayMs, out distanceDelayMs, out distanceFixedDelayMs))
				{
					SnapshotObjectPool<FiredOperationInfo>.ReleaseImmediately(firedOperationInfo);
					continue;
				}
				ModifiedFiredOperationInfo modifiedFiredOperationInfo = new ModifiedFiredOperationInfo();
				modifiedFiredOperationInfo.IsValid = true;
				modifiedFiredOperationInfo.FiredOperationInfo = firedOperationInfo;
				modifiedFiredOperationInfo.FixedDelayMs = fixedDelayMs;
				modifiedFiredOperationInfo.DistanceDelayMs = distanceDelayMs;
				modifiedFiredOperationInfo.DistanceFixedDelayMs = distanceFixedDelayMs;
				temporaryList.Add(modifiedFiredOperationInfo);
			}
			if (temporaryList.Count == 0)
			{
				continue;
			}
			HashSet<byte> hashSet = null;
			for (int k = 0; k < temporaryList.Count; k++)
			{
				ModifiedFiredOperationInfo modifiedFiredOperationInfo2 = temporaryList[k];
				FiredOperationInfo firedOperationInfo2 = modifiedFiredOperationInfo2.FiredOperationInfo;
				OperationData operationData2 = firedOperationInfo2.OperationData;
				if (operationData2 == null)
				{
					continue;
				}
				int num = ((firedOperationInfo2.SkillData != null) ? firedOperationInfo2.SkillData.Index : 0);
				if (num == 0)
				{
					continue;
				}
				int operationNo = operationData2.OperationNo;
				byte originalValue = operationData2.Group;
				byte modifiedInt = ModifierContainer.GetModifiedInt(num, operationNo, OperationFieldType.Group, 0, originalValue);
				if (modifiedInt == 0)
				{
					continue;
				}
				hashSet = hashSet ?? new HashSet<byte>();
				if (hashSet.Contains(modifiedInt))
				{
					continue;
				}
				hashSet.Add(modifiedInt);
				List<ModifiedFiredOperationInfo> list = new List<ModifiedFiredOperationInfo>();
				list.Add(modifiedFiredOperationInfo2);
				for (int l = k + 1; l < temporaryList.Count; l++)
				{
					ModifiedFiredOperationInfo modifiedFiredOperationInfo3 = temporaryList[l];
					if (modifiedFiredOperationInfo3.FiredOperationInfo.OperationData.Group == modifiedInt)
					{
						list.Add(modifiedFiredOperationInfo3);
					}
				}
				CheckOperationGroupCondition(skillData, list);
			}
			for (int m = 0; m < temporaryList.Count; m++)
			{
				ModifiedFiredOperationInfo modifiedFiredOperationInfo4 = temporaryList[m];
				if (!modifiedFiredOperationInfo4.IsValid)
				{
					continue;
				}
				FiredOperationInfo firedOperationInfo3 = modifiedFiredOperationInfo4.FiredOperationInfo;
				int fixedDelayMs2 = modifiedFiredOperationInfo4.FixedDelayMs;
				int distanceDelayMs2 = modifiedFiredOperationInfo4.DistanceDelayMs;
				int distanceFixedDelayMs2 = modifiedFiredOperationInfo4.DistanceFixedDelayMs;
				OperationInfo operationInfo = OperationQueue.Add(firedOperationInfo3, fixedDelayMs2, distanceDelayMs2, distanceFixedDelayMs2);
				if (operationInfo != null)
				{
					operationInfo.IsIgnoreSoloTime = isIgnoreSoloTime;
					operationInfo.IsTeamInverted = isTeamInverted;
					if (usingSkillInfo != null)
					{
						usingSkillInfo.OperationCreate++;
					}
					int operationNo2 = firedOperationInfo3.OperationNo;
					int targetCreatureUid = ((firedOperationInfo3.TargetCreature != null) ? firedOperationInfo3.TargetCreature.Uid : 0);
					Vector3 firedTargetPosition2 = firedOperationInfo3.FiredTargetPosition;
					addedOperationInfoList.Add(operationInfo);
					skillOperationInfoList.Add(new FireSkill.OperationInfo
					{
						OperationUid = operationInfo.Uid,
						OperationNo = operationNo2,
						TargetCreatureUid = targetCreatureUid,
						TargetPosition = firedTargetPosition2,
						SoloTimeMs = soloTimeMs,
						IsIgnoreSoloTime = isIgnoreSoloTime
					});
				}
			}
		}
	}

	private bool ModifyFiredOperation(int operationNo, FiredOperationInfo firedOperationInfo, UsingSkillInfo usingSkillInfo, out int fixedDelayMs, out int distanceDelayMs, out int distanceFixedDelayMs)
	{
		fixedDelayMs = 0;
		distanceDelayMs = 0;
		distanceFixedDelayMs = 0;
		if (firedOperationInfo == null)
		{
			return false;
		}
		OperationData operationData = firedOperationInfo.OperationData;
		if (operationData == null)
		{
			return false;
		}
		fixedDelayMs = operationData.GetFixedDelayMs();
		distanceDelayMs = operationData.GetDistanceDelayMs();
		distanceFixedDelayMs = operationData.GetDistanceFixedDelayMs();
		int skillIndex = operationData.SkillIndex;
		IOperationModifier modifier = GameRoot.Instance.OperationModifierContainer.GetModifier(skillIndex);
		if (modifier != null)
		{
			OperationModifierArg operationModifierArg = new OperationModifierArg();
			operationModifierArg.TickIndex = usingSkillInfo?.TickIndex ?? 0;
			operationModifierArg.OperationIndex = usingSkillInfo?.OperationCreate ?? 0;
			operationModifierArg.Info = firedOperationInfo;
			operationModifierArg.FixedDelayMs = fixedDelayMs;
			operationModifierArg.DistanceDelayMs = distanceDelayMs;
			modifier.Modify(operationModifierArg);
			fixedDelayMs = operationModifierArg.FixedDelayMs;
			distanceDelayMs = operationModifierArg.DistanceDelayMs;
		}
		if (ModifierContainer.GetModifiedT(skillIndex, operationNo, OperationFieldType.ConditionType, 0, operationData.ConditionType) == OperationConditionType.None)
		{
			return true;
		}
		if (operationData.HasFlag(OperationFlag.WatchCondition) && operationData.DurationMs > 0)
		{
			return true;
		}
		if (operationData.HasDelayedConditionCheckFlag())
		{
			return true;
		}
		bool flag = CheckMeetCondition(ModifierContainer, operationData, firedOperationInfo, firedOperationInfo.TargetCreature);
		if (operationData.ConditionActionType != OperationConditionActionType.None)
		{
			RunConditionAction(flag, operationData, firedOperationInfo);
		}
		else if (!flag)
		{
			return false;
		}
		return true;
	}

	public static bool CheckMeetCondition(SkillModifierContainer skillModifier, OperationData operationData, FiredOperationInfo firedOperationInfo, BattleCreature appliedCreature)
	{
		int skillIndex = operationData.SkillIndex;
		int operationNo = operationData.OperationNo;
		OperationConditionType modifiedT = skillModifier.GetModifiedT(skillIndex, operationNo, OperationFieldType.ConditionType, 0, operationData.ConditionType);
		int num = ((operationData.ConditionValues != null) ? operationData.ConditionValues.Length : 0);
		using ArrayPool<string>.AutoArrayPoolReleaser autoArrayPoolReleaser = ArrayPool<string>.AllocAuto(num);
		for (int i = 0; i < num; i++)
		{
			string modifiedString = skillModifier.GetModifiedString(skillIndex, operationNo, OperationFieldType.ConditionValue, i, operationData.ConditionValues[i]);
			autoArrayPoolReleaser[i] = modifiedString;
		}
		bool reverseCheck = operationData.ReverseCheck;
		return GameRoot.Instance.OperationConditionFactory.MeetCondition(modifiedT, firedOperationInfo, appliedCreature, autoArrayPoolReleaser, reverseCheck);
	}

	public static void RunConditionAction(bool meetCondition, OperationData operationData, FiredOperationInfo firedOperationInfo)
	{
		OperationConditionActionType conditionActionType = operationData.ConditionActionType;
		if (conditionActionType != OperationConditionActionType.None)
		{
			int operationNo = operationData.OperationNo;
			IOperationConditionAction action = GameRoot.Instance.OperationConditionActionFactory.GetAction(conditionActionType);
			if (action != null)
			{
				string[] conditionActionValues = (meetCondition ? operationData.ConditionTrueActionValues : operationData.ConditionFalseActionValues);
				action.Run(operationNo, firedOperationInfo, conditionActionValues);
			}
		}
	}

	private void CheckOperationGroupCondition(SkillData skillData, List<ModifiedFiredOperationInfo> modifiedInfoList)
	{
		if (modifiedInfoList == null || modifiedInfoList.Count == 0 || skillData == null)
		{
			return;
		}
		int index = skillData.Index;
		OperationGroupConditionType operationGroupConditionType = OperationGroupConditionType.None;
		for (int i = 0; i < modifiedInfoList.Count; i++)
		{
			FiredOperationInfo firedOperationInfo = modifiedInfoList[i].FiredOperationInfo;
			if (firedOperationInfo == null)
			{
				continue;
			}
			OperationData operationData = firedOperationInfo.OperationData;
			if (operationData != null)
			{
				int operationNo = operationData.OperationNo;
				OperationGroupConditionType groupConditionType = operationData.GroupConditionType;
				operationGroupConditionType = ModifierContainer.GetModifiedT(index, operationNo, OperationFieldType.GroupConditionType, 0, groupConditionType);
				if (operationGroupConditionType != OperationGroupConditionType.None)
				{
					break;
				}
			}
		}
		if (operationGroupConditionType == OperationGroupConditionType.None)
		{
			VespaDebug.LogError("GroupConditionType is None : skill=" + skillData);
			return;
		}
		IOperationGroupCondition condition = GameRoot.Instance.OperationGroupConditionFactory.GetCondition(operationGroupConditionType);
		if (condition == null)
		{
			VespaDebug.LogError("GroupCondition data is null: " + operationGroupConditionType);
		}
		else
		{
			condition.Check(modifiedInfoList);
		}
	}

	public void FireProjectileSkill(SkillData skillData, int skillLevel, BattleCreature[] targetCreatures, Vector3 firedPosition)
	{
		HashSet<int> usedSkillIndices = new HashSet<int>();
		FireProjectileSkillInternal(skillData, skillLevel, targetCreatures, firedPosition, usedSkillIndices);
	}

	private void FireProjectileSkillInternal(SkillData skillData, int skillLevel, BattleCreature[] targetCreatures, Vector3 firedPosition, HashSet<int> usedSkillIndices)
	{
		if (skillData != null && !usedSkillIndices.Contains(skillData.Index))
		{
			usedSkillIndices.Add(skillData.Index);
			FireSingleSkill(skillData, skillData, null, skillLevel, null, targetCreatures, null, firedPosition);
			if (skillData.NextIndex != 0)
			{
				SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillData.NextIndex);
				FireProjectileSkillInternal(data, skillLevel, targetCreatures, firedPosition, usedSkillIndices);
			}
		}
	}

	private void UpdateUsingDurationalSkill(int deltaTimeMs, out bool nextSkillActivated)
	{
		nextSkillActivated = false;
		if (UsingSkillInfo == null)
		{
			return;
		}
		SkillData skillData = UsingSkillInfo.SkillData;
		if (skillData == null)
		{
			StopUsingDurationalSkill(isCancel: true);
			return;
		}
		if (Owner.StatController.IsDead())
		{
			StopUsingDurationalSkill(isCancel: true);
			return;
		}
		_ = UsingSkillInfo.SkillLevel;
		_ = UsingSkillInfo.TriggerInfo;
		_ = UsingSkillInfo.FiredSkillData;
		if (CanUpdateTimer())
		{
			int deltaTimeMs2 = Owner.StatController.MultiplyAttackSpeed(deltaTimeMs);
			UsingSkillInfo.Update(deltaTimeMs2);
			BattleCreature[] firedTargetCreatures = UsingSkillInfo.FiredTargetCreatures;
			if (skillData.GetViewTargetWhileUse(Owner) && firedTargetCreatures != null && firedTargetCreatures.Length != 0 && firedTargetCreatures[0] != null)
			{
				Vector3 position = firedTargetCreatures[0].TransformController.Position;
				Owner.TransformController.ViewPosition(position);
			}
		}
		bool num = UsingSkillInfo.IsUsing();
		bool nextActivated = UsingSkillInfo.NextActivated;
		bool isSkillLeft = UsingSkillInfo.IsSkillLeft;
		if ((!num && !isSkillLeft) || nextActivated)
		{
			UsingSkillInfo usingSkillInfo = UsingSkillInfo;
			StopUsingDurationalSkill(isCancel: false);
			nextSkillActivated = UseNextSkill(usingSkillInfo);
			return;
		}
		if (!UsingSkillInfo.OnAttackEventApplied && UsingSkillInfo.IsActivated())
		{
			UsingSkillInfo.OnAttackEventApplied = true;
			if (UsingSkillInfo.IsBaseAttack)
			{
				Owner.StatController.RecoverHpByStat(StatType.HpOnAttack, StatType.HpRatioOnAttack, HealFlag.HpOnAttack | HealFlag.Supplemently);
				Owner.StatController.RecoverMpByStat(StatType.MpOnAttack, StatType.MpRatioOnAttack, ChangeMp.ReasonType.MpOnAttack, 1000L);
			}
		}
		SnapshotList<SkillTickTime> skillTickTimeList = UsingSkillInfo.SkillTickTimeList;
		if (skillTickTimeList == null || skillTickTimeList.Count == 0)
		{
			UsingSkillInfo.IsSkillLeft = false;
			return;
		}
		while (skillTickTimeList != null && UsingSkillInfo.TickIndex < skillTickTimeList.Count)
		{
			SkillTickTime skillTickTime = skillTickTimeList[UsingSkillInfo.TickIndex];
			if (UsingSkillInfo.ElapsedTimeMs >= skillTickTime.TickMs)
			{
				FireUsingDurationalSkill(skillTickTime);
				UsingSkillInfo.TickIndex++;
				if (UsingSkillInfo.TickIndex < skillTickTimeList.Count)
				{
					UsingSkillInfo.IsSkillLeft = true;
				}
				else
				{
					UsingSkillInfo.IsSkillLeft = false;
				}
				continue;
			}
			break;
		}
	}

	private bool UseNextSkill(UsingSkillInfo usingSkillInfo)
	{
		if (usingSkillInfo == null)
		{
			return false;
		}
		SkillData skillData = usingSkillInfo.SkillData;
		if (skillData == null)
		{
			return false;
		}
		if (skillData.NextIndex <= 0)
		{
			return false;
		}
		BattleCreature manualTarget = usingSkillInfo.ManualTarget;
		int nextIndex = skillData.NextIndex;
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(nextIndex);
		if (data == null)
		{
			return false;
		}
		if (ModifierContainer.GetModifiedBool(nextIndex, 0, OperationFieldType.Disabled, 0))
		{
			return false;
		}
		if (!PassiveSkillContainer.IsToggleEnabled(data))
		{
			return false;
		}
		if (!CheckTriggerRatio(data, "UseNextSkill "))
		{
			return false;
		}
		BattleCreature[] targetCreatures = null;
		SkillUseOption skillUseOption = new SkillUseOption();
		skillUseOption.CheckOnly = false;
		skillUseOption.CheckLevel = false;
		skillUseOption.CheckMp = true;
		skillUseOption.CheckReqHpRatio = true;
		skillUseOption.IsEventSkill = false;
		skillUseOption.ManualTarget = manualTarget;
		skillUseOption.PrevUsedTargets = usingSkillInfo.UsedTargetCreatures;
		skillUseOption.PrevFiredTargets = usingSkillInfo.FiredTargetCreatures;
		skillUseOption.CheckBattleEnabled = !data.HasTriggerFlag(SkillTriggerFlag.CanUseOnWaveEnd);
		if (CanUseSkill(data, skillUseOption, out targetCreatures) != SkillUseResult.Success)
		{
			return false;
		}
		int skillLevel = usingSkillInfo.SkillLevel;
		ISkillTriggerInfo triggerInfo = usingSkillInfo.TriggerInfo;
		SkillData firedSkillData = usingSkillInfo.FiredSkillData;
		bool isBaseAttack = usingSkillInfo.IsBaseAttack;
		FireSkill(data, skillLevel, triggerInfo, targetCreatures, firedSkillData, manualTarget, isIgnoreCoolTime: false, isIgnoreSoloTime: false, isCanBeCancel: false, isBaseAttack);
		return true;
	}

	private void UpdateOperationQueue(int deltaTimeMs)
	{
		OperationQueue.Update(deltaTimeMs);
	}

	private void FireUsingDurationalSkill(SkillTickTime tickTime)
	{
		if (UsingSkillInfo == null)
		{
			return;
		}
		SkillData skillData = UsingSkillInfo.SkillData;
		if (skillData != null)
		{
			BattleCreature[] array = UsingSkillInfo.UsedTargetCreatures;
			if (skillData.GetRetargetOnFire(Owner) && !Owner.BattleInstance.IsBattleClosingOrDisabled())
			{
				BattleCreature manualTarget = UsingSkillInfo.ManualTarget;
				BattleCreature[] usedTargetCreatures = UsingSkillInfo.UsedTargetCreatures;
				BattleCreature[] firedTargetCreatures = UsingSkillInfo.FiredTargetCreatures;
				array = Owner.BattleInstance.CreatureContainer.FindSkillTargetCreature(Owner, skillData, checkOnly: false, null, manualTarget, usedTargetCreatures, firedTargetCreatures)?.ToArray();
				UsingSkillInfo.FiredTargetCreatures = array;
			}
			int skillLevel = UsingSkillInfo.SkillLevel;
			ISkillTriggerInfo triggerInfo = UsingSkillInfo.TriggerInfo;
			FireSingleSkill(skillData, UsingSkillInfo.FiredSkillData, tickTime, skillLevel, triggerInfo, array, UsingSkillInfo);
		}
	}

	public void CancelUsingDurationalSkillOnWaveEnd()
	{
		if (UsingSkillInfo != null && UsingSkillInfo.SkillData.CancelOnWaveEnd)
		{
			StopUsingDurationalSkill(isCancel: true);
		}
	}

	public void StopUsingDurationalSkill(bool isCancel)
	{
		if (UsingSkillInfo == null)
		{
			return;
		}
		if (UsingSkillInfo.StateModifiers != null)
		{
			DelegateStateModifier[] stateModifiers = UsingSkillInfo.StateModifiers;
			foreach (DelegateStateModifier stateModifier in stateModifiers)
			{
				Owner.StateController.RemoveModifier(stateModifier);
			}
		}
		if (UsingSkillInfo.OperationInfos != null)
		{
			foreach (OperationInfo item in (IEnumerable<OperationInfo>)UsingSkillInfo.OperationInfos)
			{
				if (item.WhileSkillUse)
				{
					item.Expired = true;
				}
			}
		}
		if (!UsingSkillInfo.IsIgnoreCoolTime)
		{
			StartCooltime(UsingSkillInfo.SkillData);
		}
		RaiseEndFireSkill(UsingSkillInfo, isCancel);
		UsingSkillInfo usingSkillInfo = UsingSkillInfo;
		_usingSkillInfoHolder.Set(null);
		if ((usingSkillInfo.SkillData?.HasTriggerFlag(SkillTriggerFlag.UseNextSkillOnCancel) ?? false) && isCancel)
		{
			UseNextSkill(usingSkillInfo);
		}
	}

	private void StartCooltime(SkillData skillData)
	{
		if (skillData != null)
		{
			int baseSkillIndex = Owner.CreatureData.BaseSkillIndex;
			int index = skillData.Index;
			if (index != baseSkillIndex)
			{
				int coolTimeMs = skillData.GetCoolTimeMs(Owner);
				CooltimeManager.BeginCooltime(index, coolTimeMs, applyCoolTimeModifier: false);
			}
		}
	}

	private void RaiseEndFireSkill(UsingSkillInfo usingSkillInfo, bool isCancel)
	{
		EndFireSkill endFireSkill = new EndFireSkill();
		endFireSkill.CreatureUid = Owner.Uid;
		endFireSkill.SkillId = usingSkillInfo.SkillData.Index;
		endFireSkill.IsCancel = isCancel;
		Owner.RaiseBattleEvent(endFireSkill);
	}

	public SkillUseResult CanUseSkill(SkillData skillData, SkillUseOption option, BattleCreature[] forcedTargetCreatures = null)
	{
		BattleCreature[] targetCreatures;
		return CanUseSkill(skillData, option, out targetCreatures, forcedTargetCreatures);
	}

	public SkillUseResult CanUseSkill(SkillData skillData, SkillUseOption option, out BattleCreature[] targetCreatures, BattleCreature[] forcedTargetCreatures = null)
	{
		if (option == null)
		{
			option = SkillUseOption.NullValue;
		}
		targetCreatures = forcedTargetCreatures;
		if (skillData == null)
		{
			return SkillUseResult.Fail;
		}
		CreatureTagType creatureTagType = skillData.CreatureTagType;
		if (creatureTagType != CreatureTagType.None && Owner.CreatureData.TagType != creatureTagType)
		{
			return SkillUseResult.CreatureTagTypeMismatch;
		}
		if (!skillData.HasTriggerFlag(SkillTriggerFlag.CanUseWhenDied) && Owner.StatController.IsDead())
		{
			return SkillUseResult.Died;
		}
		if (!skillData.HasTriggerFlag(SkillTriggerFlag.CanUseWhenDisabledSkillSlot) && Owner.SkillController.SkillCheckModifierContainer.IsDisabledSkill(skillData.Index, SkillDisablerType.DisableSkillSlot))
		{
			return SkillUseResult.DisableSkillSlot;
		}
		if (option != null && option.CheckStateCount && Owner.SkillController.SkillCheckModifierContainer.IsDisabledSkill(skillData.Index, SkillDisablerType.StateCount))
		{
			return SkillUseResult.NotEnoughStateCount;
		}
		if (option != null && option.CheckMp && !option.IsEventSkill)
		{
			int mp = skillData.GetMp(Owner);
			if (mp > 0 && Owner.StatController.Mp < mp)
			{
				return SkillUseResult.NotEnoughMp;
			}
		}
		if (option != null && option.CheckBattleEnabled)
		{
			if (Owner.BattleInstance.IsBattleClosingOrDisabled())
			{
				return SkillUseResult.BattleDisabled;
			}
			if (skillData.TriggerType == SkillTriggerType.None && Owner.BattleInstance.IsWaveStartedTime())
			{
				return SkillUseResult.BattleDisabled;
			}
		}
		if (option != null && option.CheckLevel && !option.IsEventSkill && Owner.GetUserSkillLevel(skillData.Index) <= 0)
		{
			return SkillUseResult.InvalidSkillLevel;
		}
		if (skillData.ReqIndex > 0 && Owner.GetUserSkillLevel(skillData.ReqIndex) <= 0)
		{
			return SkillUseResult.InvalidSkillLevel;
		}
		bool flag = skillData.HasTriggerFlag(SkillTriggerFlag.CancelUsingSkill);
		if (flag && skillData.CancelableSkillIndex != null && skillData.CancelableSkillIndex.Length != 0 && UsingSkillInfo != null && UsingSkillInfo.FiredSkillData != null)
		{
			bool flag2 = false;
			for (int i = 0; i < skillData.CancelableSkillIndex.Length; i++)
			{
				if (skillData.CancelableSkillIndex[i] == UsingSkillInfo.SkillData.Index)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				flag = false;
			}
		}
		if (!flag && skillData.IsDurationalSkill() && !option.IsEventSkill && UsingSkillInfo != null && !UsingSkillInfo.IsBaseAttack)
		{
			if (UsingSkillInfo.FiredSkillData.TriggerType == SkillTriggerType.None && !UsingSkillInfo.IsCanBeCanceled())
			{
				return SkillUseResult.AlreadyUsing;
			}
			bool flag3 = UsingSkillInfo.FiredSkillData.HasTriggerFlag(SkillTriggerFlag.CanNotCancelBySlotSkill);
			if (skillData.TriggerType != SkillTriggerType.None || flag3)
			{
				return SkillUseResult.AlreadyUsing;
			}
		}
		if (!Owner.StateController.CheckSkillState(skillData, Owner.CreatureData.BaseSkillIndex == skillData.Index) && !option.IsEventSkill)
		{
			return SkillUseResult.InvalidState;
		}
		if (targetCreatures == null || targetCreatures.Length == 0)
		{
			BattleCreatureList battleCreatureList = Owner.BattleInstance.CreatureContainer.FindSkillTargetCreature(Owner, skillData, option?.CheckOnly ?? false, null, option?.ManualTarget, option?.PrevUsedTargets, option?.PrevFiredTargets);
			if (battleCreatureList == null || battleCreatureList.Count == 0)
			{
				return SkillUseResult.InvalidTarget;
			}
			targetCreatures = battleCreatureList.ToArray();
		}
		if (skillData.TargetType.NeedCheckDeadTarget() && targetCreatures != null && targetCreatures.Length != 0 && targetCreatures[0].StatController.IsDead() != skillData.DeadTarget)
		{
			return SkillUseResult.InvalidTarget;
		}
		if (!skillData.HasTargetFlag(SkillTargetFlag.IgnoreRangeCheckOnFire))
		{
			int minRangeMm = skillData.GetMinRangeMm(Owner);
			int maxRangeMm = skillData.GetMaxRangeMm(Owner);
			if (maxRangeMm > 0 || minRangeMm > 0)
			{
				SkillUseResult result = SkillUseResult.TargetTooFar;
				BattleCreature battleCreature = null;
				BattleCreature[] array = targetCreatures;
				foreach (BattleCreature battleCreature2 in array)
				{
					if (maxRangeMm > 0 && BattleCreatureContainer.IsInSkillRange(Owner, battleCreature2, 0, maxRangeMm) != SkillRangeResultType.Success)
					{
						result = SkillUseResult.TargetTooFar;
						continue;
					}
					if (minRangeMm > 0 && BattleCreatureContainer.IsInSkillRange(Owner, battleCreature2, minRangeMm, 99999999) != SkillRangeResultType.Success)
					{
						result = SkillUseResult.TargetTooNear;
						continue;
					}
					battleCreature = battleCreature2;
					break;
				}
				if (battleCreature == null)
				{
					return result;
				}
			}
		}
		if (CooltimeManager.IsInCooltime(skillData.Index) && !option.IsEventSkill)
		{
			return SkillUseResult.Cooltime;
		}
		if (!IsConditionActive(Owner, skillData))
		{
			return SkillUseResult.ConditionFail;
		}
		return SkillUseResult.Success;
	}

	public bool IsUsingForceImmuneSkill()
	{
		if (UsingSkillInfo == null)
		{
			return false;
		}
		return UsingSkillInfo.SkillData?.HasTriggerFlag(SkillTriggerFlag.ForceImmune) ?? false;
	}

	public void CancelMagicalSkill(int firedSkillIndex = 0)
	{
		if (UsingSkillInfo == null || UsingSkillInfo.SkillData.AttrType != SkillAttrType.Magical)
		{
			return;
		}
		if (UsingSkillInfo.SkillData.Index != firedSkillIndex)
		{
			StopUsingDurationalSkill(isCancel: true);
			return;
		}
		VespaDebug.LogError("[CancelMagicalSkill] Same skill : " + firedSkillIndex);
		if (BattleConstant.EnableSameSkillCancel)
		{
			StopUsingDurationalSkill(isCancel: true);
		}
	}

	public void CancelPhysicalSkill(int firedSkillIndex = 0)
	{
		if (UsingSkillInfo == null || UsingSkillInfo.SkillData.AttrType != SkillAttrType.Physical)
		{
			return;
		}
		if (UsingSkillInfo.SkillData.Index != firedSkillIndex)
		{
			StopUsingDurationalSkill(isCancel: true);
			return;
		}
		VespaDebug.LogError("[CancelPhysicalSkill] Same skill : " + firedSkillIndex);
		if (BattleConstant.EnableSameSkillCancel)
		{
			StopUsingDurationalSkill(isCancel: true);
		}
	}

	public void CancelBaseAttack()
	{
		if (UsingSkillInfo != null && UsingSkillInfo.SkillData != null && UsingSkillInfo.SkillData.BaseAttack)
		{
			StopUsingDurationalSkill(isCancel: true);
		}
	}

	public void ResetBaseAttack()
	{
		_baseAttackCheckedTimeMsHolder.Set(-10000L);
	}

	public void ResetOnWaveEnd()
	{
		CancelMagicalSkill();
		CancelPhysicalSkill();
		ResetBaseAttack();
		OperationQueue.ClearOnWaveEnd();
	}

	public bool ReserveSkill(SkillData skillData, int skillSlotIndex, int manualTargetUid, bool autoReserved)
	{
		if (Owner.StateController.RejectReservedSkill())
		{
			return false;
		}
		if (skillData != null)
		{
			CancelReservedSkill();
			_reservedSkillHolder.Set(skillData);
			_reservedSkillSlotIndexHolder.Set(skillSlotIndex);
			_autoReservedHolder.Set(autoReserved);
			BattleCreature data = ((manualTargetUid > 0) ? Owner.BattleInstance.CreatureContainer.GetCreature(manualTargetUid) : null);
			_reservedSkillManualTargetHolder.Set(data);
			ReserveSkill reserveSkill = new ReserveSkill();
			reserveSkill.CreatureUid = Owner.Uid;
			reserveSkill.SkillId = skillData.Index;
			Owner.RaiseBattleEvent(reserveSkill);
			return true;
		}
		return false;
	}

	public bool CancelReservedSkill()
	{
		SkillData skillData = _reservedSkillHolder.Get();
		if (skillData != null)
		{
			_reservedSkillHolder.Set(null);
			_reservedSkillSlotIndexHolder.Set(0);
			_reservedSkillManualTargetHolder.Set(null);
			_autoReservedHolder.Set(data: false);
			CancelReservedSkill cancelReservedSkill = new CancelReservedSkill();
			cancelReservedSkill.CreatureUid = Owner.Uid;
			cancelReservedSkill.SkillId = skillData.Index;
			Owner.RaiseBattleEvent(cancelReservedSkill);
			return true;
		}
		return false;
	}

	private void UpdateReservedSkill()
	{
		SkillData skillData = _reservedSkillHolder.Get();
		if (skillData == null || ReservedSkillSlotIndex == 10)
		{
			return;
		}
		long currentTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs();
		if (currentTimeMs - _lastReservedSkillCheckTimeMsHolder.Get() < 200)
		{
			return;
		}
		_lastReservedSkillCheckTimeMsHolder.Set(currentTimeMs);
		BattleCreature battleCreature = _reservedSkillManualTargetHolder.Get();
		if (battleCreature != null && !battleCreature.StateController.CanBeTargetedState(Owner))
		{
			CancelReservedSkill();
			return;
		}
		if (skillData.HasTriggerFlag(SkillTriggerFlag.FireNextSkillOnReserve) && skillData.NextIndex >= 0)
		{
			int nextIndex = skillData.NextIndex;
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(nextIndex);
			if (data != null && UsingSkillInfo != null && data == UsingSkillInfo.SkillData && FireNextSkill())
			{
				CancelReservedSkill();
				return;
			}
		}
		int reservedSkillSlotIndex = ReservedSkillSlotIndex;
		SkillUseOption option = new SkillUseOption(checkOnly: false, checkLevel: true, checkMp: true, isEventSkill: false, battleCreature);
		if (UseSkill(skillData, option, null, 0, isActiveSkill: true, reservedSkillSlotIndex) == SkillUseResult.Success)
		{
			if (Owner.DungeonSkillController != null)
			{
				Owner.DungeonSkillController.IncreaseUseSkillCount(reservedSkillSlotIndex, skillData.Index);
			}
			OnFireSlotSkill onFireSlotSkill = SnapshotObjectPool<OnFireSlotSkill>.Alloc();
			onFireSlotSkill.TriggerCreature = Owner;
			onFireSlotSkill.TargetCreatures = new BattleCreature[1] { Owner };
			onFireSlotSkill.SkillIndex = skillData.Index;
			onFireSlotSkill.SlotIndex = reservedSkillSlotIndex;
			Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onFireSlotSkill);
			CancelReservedSkill();
		}
	}

	public bool ReserveSoulWeaponSkill(SkillData skillData, int skillSlotIndex, int manualTargetUid, bool autoReserved)
	{
		if (Owner.StateController.RejectReservedSkill())
		{
			return false;
		}
		if (!Owner.SoulWeaponSkillController.IsValidUseCount())
		{
			return false;
		}
		if (skillData != null)
		{
			CancelReservedSkill();
			_reservedSkillHolder.Set(skillData);
			_reservedSkillSlotIndexHolder.Set(skillSlotIndex);
			_autoReservedHolder.Set(autoReserved);
			BattleCreature data = ((manualTargetUid > 0) ? Owner.BattleInstance.CreatureContainer.GetCreature(manualTargetUid) : null);
			_reservedSkillManualTargetHolder.Set(data);
			ReserveSkill reserveSkill = new ReserveSkill();
			reserveSkill.CreatureUid = Owner.Uid;
			reserveSkill.SkillId = skillData.Index;
			Owner.RaiseBattleEvent(reserveSkill);
			return true;
		}
		return false;
	}

	public void UpdateReservedSoulWeaponSkill()
	{
		SkillData skillData = _reservedSkillHolder.Get();
		if (skillData == null || ReservedSkillSlotIndex != 10)
		{
			return;
		}
		long currentTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs();
		if (currentTimeMs - _lastReservedSoulWeaponSkillCheckTimeMsHolder.Get() < 200)
		{
			return;
		}
		_lastReservedSoulWeaponSkillCheckTimeMsHolder.Set(currentTimeMs);
		BattleCreature battleCreature = _reservedSkillManualTargetHolder.Get();
		if (battleCreature != null && !battleCreature.StateController.CanBeTargetedState(Owner))
		{
			CancelReservedSkill();
		}
		else if (Owner.SoulWeaponSkillController.CanUseSoulWeaponSkill())
		{
			_ = ReservedSkillSlotIndex;
			if (UseSkill(skillData, SkillUseOption.GetSoulWeaponSkillUseOption(battleCreature), null, 0, isActiveSkill: false, 10) == SkillUseResult.Success)
			{
				Owner.SoulWeaponSkillController.IncreaseUseSkillCount();
				CancelReservedSkill();
			}
		}
	}

	public bool ReserveDungeonSkill(SkillData skillData, int skillSlotIndex)
	{
		if (skillData == null)
		{
			return false;
		}
		if (Owner.StateController.RejectReservedSkill())
		{
			return false;
		}
		if (!Owner.DungeonSkillController.IsValidUseCount(skillSlotIndex))
		{
			return false;
		}
		DungeonSkillStateInfo dungeonSkillInfo = Owner.DungeonSkillController.GetDungeonSkillInfo(skillSlotIndex);
		if (dungeonSkillInfo != null && (dungeonSkillInfo.SkillCanUseResult == SkillUseResult.Success || dungeonSkillInfo.SkillCanUseResult == SkillUseResult.EnhancedSuccess))
		{
			CancelReservedSkill();
			_reservedSkillHolder.Set(skillData);
			_reservedSkillSlotIndexHolder.Set(skillSlotIndex);
			ReserveSkill reserveSkill = new ReserveSkill();
			reserveSkill.CreatureUid = Owner.Uid;
			reserveSkill.SkillId = skillData.Index;
			Owner.RaiseBattleEvent(reserveSkill);
			return true;
		}
		return false;
	}
}
