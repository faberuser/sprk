using System;
using System.Collections.Generic;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class OperationInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<long> _uidHolder;

	private readonly SnapshotObjectMember<BattleCreature> _creatureHolder;

	private readonly SnapshotObjectMember<FiredOperationInfo> _firedInfoHolder;

	private readonly SnapshotObjectMember<IOperationRuntime> _runtimeHolder;

	private readonly SnapshotObjectMember<int> _elapsedTimeMsHolder;

	private readonly SnapshotObjectMember<int> _tickedTimeMsHolder;

	private readonly SnapshotObjectMember<bool> _expiredHolder;

	private readonly SnapshotObjectMember<bool> _missedHolder;

	private readonly SnapshotObjectMember<OperationInfo> _parentHolder;

	private readonly SnapshotObjectMember<int> _durationMsHolder;

	private readonly SnapshotObjectMember<int> _tickTimeMsHolder;

	private readonly SnapshotObjectMember<SnapshotArray<string>> _valuesHolder;

	private readonly SnapshotObjectMember<int> _distanceDelayMsHolder;

	private readonly SnapshotObjectMember<int> _distanceFixedDelayMsHolder;

	private readonly SnapshotObjectMember<int> _stackCountHolder;

	private readonly SnapshotObjectMember<int> _maxRangeMmHolder;

	private readonly SnapshotObjectMember<int> _areaOffsetMmHolder;

	private readonly SnapshotObjectMember<int> _radiusMmHolder;

	private readonly SnapshotObjectMember<int> _angleHolder;

	private readonly SnapshotObjectMember<int> _widthHolder;

	private readonly SnapshotObjectMember<int> _beginTimeDelayMs;

	private readonly SnapshotObjectMember<OperationTargetType> _targetTypeHolder;

	private readonly SnapshotObjectMember<string> _stateHolder;

	private readonly SnapshotObjectMember<int> _addStateCountHolder;

	private readonly SnapshotObjectMember<SnapshotArray<OperationFlag>> _flagsHolder;

	private readonly SnapshotObjectMember<OperationType> _typeHolder;

	private readonly SnapshotObjectMember<bool> _isIgnoreSoloTime;

	private readonly SnapshotObjectMember<bool> _isTeamInverted;

	public long Uid
	{
		get
		{
			return _uidHolder.Get();
		}
		set
		{
			_uidHolder.Set(value);
		}
	}

	public BattleCreature Creature
	{
		get
		{
			return _creatureHolder.Get();
		}
		set
		{
			_creatureHolder.Set(value);
		}
	}

	public FiredOperationInfo FiredInfo
	{
		get
		{
			return _firedInfoHolder.Get();
		}
		set
		{
			_firedInfoHolder.Set(value);
		}
	}

	public IOperationRuntime Runtime
	{
		get
		{
			return _runtimeHolder.Get();
		}
		set
		{
			_runtimeHolder.Set(value);
		}
	}

	public int ElapsedTimeMs
	{
		get
		{
			return _elapsedTimeMsHolder.Get();
		}
		private set
		{
			_elapsedTimeMsHolder.Set(value);
		}
	}

	public int TickedTimeMs
	{
		get
		{
			return _tickedTimeMsHolder.Get();
		}
		set
		{
			_tickedTimeMsHolder.Set(value);
		}
	}

	public bool Expired
	{
		get
		{
			return _expiredHolder.Get();
		}
		set
		{
			_expiredHolder.Set(value);
		}
	}

	public bool Missed
	{
		get
		{
			return _missedHolder.Get();
		}
		set
		{
			_missedHolder.Set(value);
		}
	}

	public OperationInfo Parent
	{
		get
		{
			return _parentHolder.Get();
		}
		set
		{
			_parentHolder.Set(value);
		}
	}

	public int DurationMs
	{
		get
		{
			return _durationMsHolder.Get();
		}
		set
		{
			_durationMsHolder.Set(value);
		}
	}

	public int TickTimeMs
	{
		get
		{
			return _tickTimeMsHolder.Get();
		}
		private set
		{
			_tickTimeMsHolder.Set(value);
		}
	}

	public string[] Values => _valuesHolder.GET();

	public int DistanceDelayMs
	{
		get
		{
			return _distanceDelayMsHolder.Get();
		}
		set
		{
			_distanceDelayMsHolder.Set(value);
		}
	}

	public int DistanceFixedDelayMs
	{
		get
		{
			return _distanceFixedDelayMsHolder.Get();
		}
		set
		{
			_distanceFixedDelayMsHolder.Set(value);
		}
	}

	public int StackCount
	{
		get
		{
			return _stackCountHolder.Get();
		}
		set
		{
			_stackCountHolder.Set(value);
		}
	}

	public int MaxRangeMm
	{
		get
		{
			return _maxRangeMmHolder.Get();
		}
		private set
		{
			_maxRangeMmHolder.Set(value);
		}
	}

	public int AreaOffsetMm
	{
		get
		{
			return _areaOffsetMmHolder.Get();
		}
		private set
		{
			_areaOffsetMmHolder.Set(value);
		}
	}

	public int RadiusMm
	{
		get
		{
			return _radiusMmHolder.Get();
		}
		private set
		{
			_radiusMmHolder.Set(value);
		}
	}

	public int Angle
	{
		get
		{
			return _angleHolder.Get();
		}
		private set
		{
			_angleHolder.Set(value);
		}
	}

	public int Width
	{
		get
		{
			return _widthHolder.Get();
		}
		private set
		{
			_widthHolder.Set(value);
		}
	}

	public int BeginTimeDelayMs
	{
		get
		{
			return _beginTimeDelayMs.Get();
		}
		set
		{
			_beginTimeDelayMs.Set(value);
		}
	}

	public OperationTargetType TargetType
	{
		get
		{
			return _targetTypeHolder.Get();
		}
		private set
		{
			_targetTypeHolder.Set(value);
		}
	}

	public string State
	{
		get
		{
			return _stateHolder.Get();
		}
		private set
		{
			_stateHolder.Set(value);
		}
	}

	public int AddStateCount
	{
		get
		{
			return _addStateCountHolder.Get();
		}
		private set
		{
			_addStateCountHolder.Set(value);
		}
	}

	public OperationFlag[] Flags
	{
		get
		{
			return _flagsHolder.GET();
		}
		private set
		{
			_flagsHolder.Set(value);
		}
	}

	public OperationType Type
	{
		get
		{
			return _typeHolder.Get();
		}
		private set
		{
			_typeHolder.Set(value);
		}
	}

	public BattleInstance BattleInstance
	{
		get
		{
			if (Creature == null)
			{
				return null;
			}
			return Creature.BattleInstance;
		}
	}

	public OperationTargetSelectType TargetSelectType
	{
		get
		{
			if (OperationData == null)
			{
				return OperationTargetSelectType.Same;
			}
			return OperationData.TargetSelectType;
		}
	}

	public int[] AreaDelayMss
	{
		get
		{
			if (OperationData == null)
			{
				return null;
			}
			return OperationData.AreaDelayMss;
		}
	}

	public bool RangeCheck => HasOperationFlag(OperationFlag.RangeCheck);

	public bool WhileSkillUse => HasOperationFlag(OperationFlag.WhileSkillUse);

	public bool ExcludeTarget => HasOperationFlag(OperationFlag.ExcludeTarget);

	public bool OnlyTarget => HasOperationFlag(OperationFlag.OnlyTarget);

	public BattleCreature FiredCreature
	{
		get
		{
			if (FiredInfo == null)
			{
				return null;
			}
			return FiredInfo.FiredCreature;
		}
	}

	public BattleCreature TargetCreature
	{
		get
		{
			if (FiredInfo == null)
			{
				return null;
			}
			return FiredInfo.TargetCreature;
		}
	}

	public BattleCreature[] TargetCreatures
	{
		get
		{
			if (FiredInfo == null)
			{
				return null;
			}
			return FiredInfo.TargetCreatures.Get();
		}
	}

	public Vector3 FiredPosition
	{
		get
		{
			if (FiredInfo == null)
			{
				return Vector3.zero;
			}
			return FiredInfo.FiredPosition;
		}
	}

	public Vector3 FiredTargetPosition
	{
		get
		{
			if (FiredInfo == null)
			{
				return Vector3.zero;
			}
			return FiredInfo.FiredTargetPosition;
		}
	}

	public Vector3 UsedTargetPosition
	{
		get
		{
			if (FiredInfo == null)
			{
				return Vector3.zero;
			}
			return FiredInfo.UsedTargetPosition;
		}
	}

	public ISkillTriggerInfo TriggerInfo
	{
		get
		{
			if (FiredInfo == null)
			{
				return null;
			}
			return FiredInfo.TriggerInfo;
		}
	}

	public SkillData SkillData
	{
		get
		{
			if (FiredInfo == null)
			{
				return null;
			}
			return FiredInfo.SkillData;
		}
	}

	public SkillData FiredSkillData
	{
		get
		{
			if (FiredInfo == null)
			{
				return null;
			}
			return FiredInfo.FiredSkillData;
		}
	}

	public int SkillIndex
	{
		get
		{
			if (SkillData == null)
			{
				return 0;
			}
			return SkillData.Index;
		}
	}

	public int SkillLevel
	{
		get
		{
			if (FiredInfo == null)
			{
				return 1;
			}
			return FiredInfo.SkillLevel;
		}
	}

	public int FiredSkillIndex
	{
		get
		{
			if (FiredSkillData == null)
			{
				return 0;
			}
			return FiredSkillData.Index;
		}
	}

	public int SoloTimeMs
	{
		get
		{
			if (FiredInfo == null)
			{
				return 0;
			}
			return FiredInfo.SoloTimeMs;
		}
	}

	public int OperationNo
	{
		get
		{
			if (OperationData == null)
			{
				return 0;
			}
			return OperationData.OperationNo;
		}
	}

	public StatType[] RecalculateValueByStats
	{
		get
		{
			if (OperationData == null)
			{
				return null;
			}
			return OperationData.RecalculateValueByStats;
		}
	}

	public string[] RecalculateValueByStates
	{
		get
		{
			if (OperationData == null)
			{
				return null;
			}
			return OperationData.RecalculateValueByStates;
		}
	}

	public OperationData OperationData
	{
		get
		{
			if (FiredInfo == null)
			{
				return null;
			}
			return FiredInfo.OperationData;
		}
	}

	public int TickIndex
	{
		get
		{
			if (FiredInfo == null)
			{
				return 0;
			}
			return FiredInfo.TickIndex;
		}
	}

	public bool IsBaseAttack
	{
		get
		{
			if (FiredInfo == null)
			{
				return false;
			}
			return FiredInfo.IsBaseAttack;
		}
	}

	public bool IsIgnoreSoloTime
	{
		get
		{
			return _isIgnoreSoloTime.Get();
		}
		set
		{
			_isIgnoreSoloTime.Set(value);
		}
	}

	public bool IsTeamInverted
	{
		get
		{
			return _isTeamInverted.Get();
		}
		set
		{
			_isTeamInverted.Set(value);
		}
	}

	public OperationInfo()
	{
		_uidHolder = new SnapshotObjectMember<long>(this, 0L);
		_creatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_firedInfoHolder = new SnapshotObjectMember<FiredOperationInfo>(this);
		_runtimeHolder = new SnapshotObjectMember<IOperationRuntime>(this);
		_elapsedTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_tickedTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_expiredHolder = new SnapshotObjectMember<bool>(this, value: false);
		_missedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_parentHolder = new SnapshotObjectMember<OperationInfo>(this);
		_durationMsHolder = new SnapshotObjectMember<int>(this, 0);
		_tickTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_valuesHolder = new SnapshotObjectMember<SnapshotArray<string>>(this);
		_distanceDelayMsHolder = new SnapshotObjectMember<int>(this, 0);
		_distanceFixedDelayMsHolder = new SnapshotObjectMember<int>(this, 0);
		_stackCountHolder = new SnapshotObjectMember<int>(this, 1);
		_maxRangeMmHolder = new SnapshotObjectMember<int>(this, 0);
		_areaOffsetMmHolder = new SnapshotObjectMember<int>(this, 0);
		_radiusMmHolder = new SnapshotObjectMember<int>(this, 0);
		_angleHolder = new SnapshotObjectMember<int>(this, 0);
		_widthHolder = new SnapshotObjectMember<int>(this, 0);
		_beginTimeDelayMs = new SnapshotObjectMember<int>(this, 0);
		_targetTypeHolder = new SnapshotObjectMember<OperationTargetType>(this, OperationTargetType.Myself);
		_stateHolder = new SnapshotObjectMember<string>(this);
		_addStateCountHolder = new SnapshotObjectMember<int>(this, 0);
		_flagsHolder = new SnapshotObjectMember<SnapshotArray<OperationFlag>>(this);
		_typeHolder = new SnapshotObjectMember<OperationType>(this, OperationType.None);
		_isIgnoreSoloTime = new SnapshotObjectMember<bool>(this, value: false);
		_isTeamInverted = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void Close()
	{
		Runtime = null;
		FiredInfo = null;
		Parent = null;
	}

	public void Init()
	{
		InitOperationData();
	}

	private void InitOperationData()
	{
		if (OperationData == null || FiredInfo == null)
		{
			return;
		}
		OperationData operationData = OperationData;
		DurationMs = FiredInfo.GetFinalDurationMs() + operationData.GetOperationLevelDuration(SkillLevel);
		TickTimeMs = FiredInfo.GetFinalTickTimeMs() + operationData.GetOperationLevelTickTime(SkillLevel);
		DistanceDelayMs = operationData.GetDistanceDelayMs();
		DistanceFixedDelayMs = operationData.GetDistanceFixedDelayMs();
		Type = operationData.Type;
		TargetType = operationData.TargetType;
		MaxRangeMm = operationData.MaxRangeMm;
		AreaOffsetMm = operationData.AreaOffsetMm;
		RadiusMm = operationData.RadiusMm;
		Angle = operationData.Angle;
		Width = operationData.Width;
		State = operationData.State;
		AddStateCount = operationData.AddStateCount;
		Flags = operationData.Flags;
		if (FiredCreature == null)
		{
			return;
		}
		int skillIndex = SkillIndex;
		int operationNo = OperationNo;
		SkillModifierContainer modifierContainer = FiredCreature.SkillController.ModifierContainer;
		Type = modifierContainer.GetModifiedT(skillIndex, operationNo, OperationFieldType.Type, 0, Type);
		DurationMs = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.DurationMs, 0, DurationMs);
		TickTimeMs = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.TickTimeMs, 0, TickTimeMs);
		MaxRangeMm = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.MaxRangeMm, 0, MaxRangeMm);
		AreaOffsetMm = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.AreaOffsetMm, 0, AreaOffsetMm);
		RadiusMm = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.RadiusMm, 0, RadiusMm);
		Angle = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.Angle, 0, Angle);
		Width = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.Width, 0, Width);
		TargetType = modifierContainer.GetModifiedT(skillIndex, operationNo, OperationFieldType.TargetType, 0, TargetType);
		DistanceDelayMs = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.DistanceDelayMs, 0, DistanceDelayMs);
		DistanceFixedDelayMs = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.DistanceFixedDelayMs, 0, DistanceFixedDelayMs);
		State = modifierContainer.GetModifiedString(skillIndex, operationNo, OperationFieldType.State, 0, State);
		AddStateCount = modifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.AddStateCount, 0, AddStateCount);
		RadiusMm = modifierContainer.GetFinalModifierInt(OperationFieldType.FinalRadiusMm, RadiusMm);
		bool flag = false;
		OperationFlag[] array = Flags;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				OperationFlag num = array[i];
				OperationFlag modifiedT = modifierContainer.GetModifiedT(skillIndex, operationNo, OperationFieldType.Flags, i, Flags[i]);
				if (num != modifiedT)
				{
					if (!flag)
					{
						flag = true;
						OperationFlag[] array2 = array;
						array = new OperationFlag[array2.Length];
						Array.Copy(array2, array, array2.Length);
					}
					array[i] = modifiedT;
				}
			}
		}
		RecalculateValue();
	}

	public bool RecalculateValue(int forcedFieldIndex = -1)
	{
		int finalValueFieldCount = FiredInfo.GetFinalValueFieldCount();
		if (finalValueFieldCount <= 0)
		{
			_valuesHolder.Set(null);
			return false;
		}
		string[] array = ArrayPool<string>.Alloc(finalValueFieldCount);
		bool flag = false;
		for (int i = 0; i < finalValueFieldCount; i++)
		{
			string finalValue = FiredInfo.GetFinalValue(i);
			if (forcedFieldIndex != -1 && i != forcedFieldIndex)
			{
				array[i] = finalValue;
				continue;
			}
			string modifiedString = FiredCreature.SkillController.ModifierContainer.GetModifiedString(SkillIndex, OperationNo, OperationFieldType.Value, i, finalValue, checkAddable: true);
			if (string.IsNullOrEmpty(modifiedString) && string.IsNullOrEmpty(finalValue))
			{
				array[i] = finalValue;
				continue;
			}
			if (string.Equals(modifiedString, finalValue, StringComparison.OrdinalIgnoreCase))
			{
				array[i] = finalValue;
				continue;
			}
			array[i] = modifiedString;
			flag = true;
		}
		if (BattleInstance.DevParam_LegacyModSkillRecalculate)
		{
			if (flag)
			{
				_valuesHolder.Set(array);
				return true;
			}
		}
		else
		{
			bool flag2 = false;
			string[] values = Values;
			for (int j = 0; j < finalValueFieldCount; j++)
			{
				string text = ((values != null && j < values.Length) ? values[j] : null);
				string text2 = ((array != null && j < array.Length) ? array[j] : null);
				text = ((text == string.Empty) ? null : text);
				text2 = ((text2 == string.Empty) ? null : text2);
				if (!string.Equals(text, text2, StringComparison.OrdinalIgnoreCase))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				_valuesHolder.Set(array);
				return true;
			}
		}
		ArrayPool<string>.Release(array);
		array = null;
		return false;
	}

	public string[] GetValues()
	{
		if (OperationData != null)
		{
			return OperationData.GetOperationValues(Values);
		}
		return null;
	}

	public int GetValueInt(int index, int defaultValue = 0)
	{
		if (OperationData == null)
		{
			return 0;
		}
		return (int)OperationData.GetOperationValueInt(this, index, defaultValue);
	}

	public long GetValueLong(int index, long defaultValue = 0L)
	{
		if (OperationData == null)
		{
			return 0L;
		}
		return OperationData.GetOperationValueInt(this, index, defaultValue);
	}

	public List<int> GetValueIntListFrom(int index, int defaultValue = 0)
	{
		List<int> list = new List<int>();
		if (OperationData != null)
		{
			while (ArgParser.HasValue(OperationData.Values, index))
			{
				int valueInt = GetValueInt(index, defaultValue);
				list.Add(valueInt);
				index++;
			}
		}
		return list;
	}

	public bool GetValueBool(int index, bool defaultValue = false)
	{
		if (OperationData == null)
		{
			return false;
		}
		return OperationData.GetOperationValueBool(index, Values, defaultValue);
	}

	public string GetValueString(int index)
	{
		if (OperationData == null)
		{
			return null;
		}
		return OperationData.GetOperationValueString(index, Values);
	}

	public string[] GetValueStrings(int index, char seperator)
	{
		if (OperationData == null)
		{
			return null;
		}
		string operationValueString = OperationData.GetOperationValueString(index, Values);
		if (string.IsNullOrEmpty(operationValueString))
		{
			return null;
		}
		string[] array = operationValueString.Split(new char[1] { seperator });
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}

	public T GetValueEnum<T>(int index) where T : struct
	{
		if (OperationData == null)
		{
			return default(T);
		}
		return OperationData.GetOperationValueEnum<T>(index, Values);
	}

	public T[] GetValueEnums<T>(int index) where T : struct
	{
		if (OperationData == null)
		{
			return null;
		}
		return OperationData.GetOperationValueEnums<T>(index, Values);
	}

	public T GetValueFlag<T>(int index) where T : struct
	{
		if (OperationData == null)
		{
			return default(T);
		}
		return OperationData.GetOperationValueFlag<T>(index, Values);
	}

	public bool IsInfinitePassiveOperation()
	{
		if (FiredInfo != null && FiredInfo.TriggerInfo != null)
		{
			return FiredInfo.TriggerInfo.TriggerType == SkillTriggerType.OnStart;
		}
		return false;
	}

	public bool IsPositiveOperation()
	{
		return HasOperationFlag(OperationFlag.Positive);
	}

	public bool IsNegativeOperation()
	{
		return HasOperationFlag(OperationFlag.Negative);
	}

	public bool HasOperationFlag(OperationFlag flag)
	{
		if (Flags != null)
		{
			for (int i = 0; i < Flags.Length; i++)
			{
				if (Flags[i] == flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasDelayedConditionCheckFlag()
	{
		if (Flags != null)
		{
			for (int i = 0; i < Flags.Length; i++)
			{
				if (OperationData.IsDelayedConditionCheckFlag(Flags[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Update(int deltaTimeMs)
	{
		ElapsedTimeMs += deltaTimeMs;
	}

	public bool IsInSoloTime()
	{
		if (SkillData == null || SkillData.GetSoloTimeMs(IsIgnoreSoloTime) <= 0)
		{
			if (FiredSkillData != null)
			{
				return FiredSkillData.GetSoloTimeMs(IsIgnoreSoloTime) > 0;
			}
			return false;
		}
		return true;
	}

	public bool IsDurationEnded()
	{
		return ElapsedTimeMs >= DurationMs;
	}

	public void InitFinalOperationDurationMs()
	{
		int durationMs = DurationMs;
		if (durationMs == 0)
		{
			return;
		}
		BattleCreature creature = Creature;
		if (creature == null)
		{
			VespaDebug.LogError("GetFinalOperationDurationMs: Creature is null");
			return;
		}
		int num = 0;
		int num2 = 0;
		if (Runtime is IStateModifier stateModifier)
		{
			StateData stateData = stateModifier.GetStateData();
			if (stateData != null)
			{
				string stateType = stateData.StateType;
				if (StateController.IsCcState(stateModifier.GetFiredCreature(), stateData))
				{
					num += creature.StatController.GetAddedStat(StatType.CcTime);
					num2 += creature.StatController.GetMultedStat(StatType.CcTime);
				}
				num += creature.StateController.GetAddedStateDurationMs(stateType);
				num2 += creature.StateController.GetMultedStateDurationMs(stateType);
			}
		}
		if (IsNegativeOperation())
		{
			num += creature.StatController.GetAddedStat(StatType.DebuffTime);
			num2 += creature.StatController.GetMultedStat(StatType.DebuffTime);
		}
		int num3 = (int)StatController.MultAndAddStat(durationMs, num2, num);
		if (num3 != DurationMs)
		{
			DurationMs = num3;
		}
	}

	public bool CanChildBeginOperation()
	{
		if (Missed)
		{
			return false;
		}
		if (DurationMs > 0 && Expired)
		{
			return false;
		}
		return true;
	}

	public void Mod(OperationFieldType fieldType, string modifiedValue, int fieldIndex = 0)
	{
		switch (fieldType)
		{
		case OperationFieldType.Value:
			Set(fieldType, modifiedValue, fieldIndex);
			break;
		case OperationFieldType.DurationMs:
			Set(fieldType, DurationMs + BaseJsonMarshaler.ToInt(modifiedValue));
			break;
		case OperationFieldType.TickTimeMs:
			Set(fieldType, TickTimeMs + BaseJsonMarshaler.ToInt(modifiedValue));
			break;
		}
	}

	public void Set(OperationFieldType fieldType, string modifiedValue, int fieldIndex = 0)
	{
		if (fieldType == OperationFieldType.Value)
		{
			if (fieldIndex < Values.Length)
			{
				Values[fieldIndex] = modifiedValue;
			}
		}
		else
		{
			Set(fieldType, BaseJsonMarshaler.ToInt(modifiedValue));
		}
	}

	public void Set(OperationFieldType fieldType, int value, int fieldIndex = 0)
	{
		switch (fieldType)
		{
		case OperationFieldType.DurationMs:
			DurationMs = value;
			break;
		case OperationFieldType.TickTimeMs:
			TickTimeMs = value;
			break;
		case OperationFieldType.RadiusMm:
			RadiusMm = ((RadiusMm < value) ? RadiusMm : value);
			break;
		}
	}

	public bool CheckValueHasExpression()
	{
		string[] array = _valuesHolder.GET() ?? OperationData.Values;
		if (array == null || array.Length == 0)
		{
			return false;
		}
		foreach (string text in array)
		{
			if (text != null && text.StartsWith("$"))
			{
				return true;
			}
		}
		return false;
	}
}
