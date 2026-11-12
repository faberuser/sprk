using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NUtil;

namespace NShared;

public class UsingSkillInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<SkillData> _skillDataHolder;

	private readonly SnapshotObjectMember<SkillData> _firedSkillDataHolder;

	private readonly SnapshotObjectMember<int> _skillLevelHolder;

	private readonly SnapshotObjectMember<ISkillTriggerInfo> _triggerInfoHolder;

	private readonly SnapshotObjectMember<SnapshotArray<BattleCreature>> _usedTargetCreaturesHolder;

	private readonly SnapshotObjectMember<SnapshotArray<BattleCreature>> _firedTargetCreaturesHolder;

	private readonly SnapshotObjectMember<Vector3> _usedTargetPositionHolder;

	private readonly SnapshotObjectMember<long> _startedTimeMsHolder;

	private readonly SnapshotObjectMember<int> _tickIndexHolder;

	private readonly SnapshotObjectMember<int> _operationCreateHolder;

	private readonly SnapshotObjectMember<int> _durationTimeMsHolder;

	private readonly SnapshotObjectMember<int> _elapsedTimeMsHolder;

	private readonly SnapshotObjectMember<bool> _onAttackEventAppliedHolder;

	private readonly SnapshotObjectMember<SnapshotArray<DelegateStateModifier>> _stateModifiersHolder;

	private readonly SnapshotObjectMember<SnapshotList<OperationInfo>> _operationInfosHolder;

	private readonly SnapshotObjectMember<bool> _nextActivatedHolder;

	private readonly SnapshotObjectMember<BattleCreature> _manualTargetHolder;

	private readonly SnapshotObjectMember<SnapshotList<SkillTickTime>> _skillTickTimeListHolder;

	private readonly SnapshotObjectMember<SnapshotArray<string>> _ignoreStateTypesHolder;

	private readonly SnapshotObjectMember<bool> _isIgnoreCoolTime;

	private readonly SnapshotObjectMember<bool> _isIgnoreSoloTime;

	private readonly SnapshotObjectMember<bool> _isCanBeCancel;

	private readonly SnapshotObjectMember<bool> _isBaseAttack;

	private readonly SnapshotObjectMember<int> _skillSlotIndexHolder;

	private readonly SnapshotObjectMember<bool> _isSkillLeft;

	public SkillData SkillData
	{
		get
		{
			return _skillDataHolder.Get();
		}
		set
		{
			_skillDataHolder.Set(value);
		}
	}

	public SkillData FiredSkillData
	{
		get
		{
			return _firedSkillDataHolder.Get();
		}
		set
		{
			_firedSkillDataHolder.Set(value);
		}
	}

	public int SkillLevel
	{
		get
		{
			return _skillLevelHolder.Get();
		}
		set
		{
			_skillLevelHolder.Set(value);
		}
	}

	public ISkillTriggerInfo TriggerInfo
	{
		get
		{
			return _triggerInfoHolder.Get();
		}
		set
		{
			_triggerInfoHolder.Set(value);
		}
	}

	public BattleCreature[] UsedTargetCreatures
	{
		get
		{
			return _usedTargetCreaturesHolder.GET();
		}
		set
		{
			_usedTargetCreaturesHolder.Set(value);
		}
	}

	public BattleCreature[] FiredTargetCreatures
	{
		get
		{
			return _firedTargetCreaturesHolder.GET();
		}
		set
		{
			_firedTargetCreaturesHolder.Set(value);
		}
	}

	public Vector3 UsedTargetPosition
	{
		get
		{
			return _usedTargetPositionHolder.Get();
		}
		set
		{
			_usedTargetPositionHolder.Set(value);
		}
	}

	public long StartedTimeMs
	{
		get
		{
			return _startedTimeMsHolder.Get();
		}
		set
		{
			_startedTimeMsHolder.Set(value);
		}
	}

	public int TickIndex
	{
		get
		{
			return _tickIndexHolder.Get();
		}
		set
		{
			_tickIndexHolder.Set(value);
		}
	}

	public int OperationCreate
	{
		get
		{
			return _operationCreateHolder.Get();
		}
		set
		{
			_operationCreateHolder.Set(value);
		}
	}

	public int DurationTimeMs
	{
		get
		{
			return _durationTimeMsHolder.Get();
		}
		set
		{
			_durationTimeMsHolder.Set(value);
		}
	}

	public int ElapsedTimeMs
	{
		get
		{
			return _elapsedTimeMsHolder.Get();
		}
		set
		{
			_elapsedTimeMsHolder.Set(value);
		}
	}

	public bool OnAttackEventApplied
	{
		get
		{
			return _onAttackEventAppliedHolder.Get();
		}
		set
		{
			_onAttackEventAppliedHolder.Set(value);
		}
	}

	public DelegateStateModifier[] StateModifiers
	{
		get
		{
			return _stateModifiersHolder.GET();
		}
		set
		{
			_stateModifiersHolder.Set(value);
		}
	}

	public SnapshotList<OperationInfo> OperationInfos => _operationInfosHolder.Get();

	public bool NextActivated
	{
		get
		{
			return _nextActivatedHolder.Get();
		}
		set
		{
			_nextActivatedHolder.Set(value);
		}
	}

	public BattleCreature ManualTarget
	{
		get
		{
			return _manualTargetHolder.Get();
		}
		set
		{
			_manualTargetHolder.Set(value);
		}
	}

	public SnapshotList<SkillTickTime> SkillTickTimeList => _skillTickTimeListHolder.Get();

	public string[] IgnoreStateTypes => _ignoreStateTypesHolder.GET();

	public bool IsIgnoreCoolTime
	{
		get
		{
			return _isIgnoreCoolTime.Get();
		}
		set
		{
			_isIgnoreCoolTime.Set(value);
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

	public bool IsCanBeCancel
	{
		get
		{
			return _isCanBeCancel.Get();
		}
		set
		{
			_isCanBeCancel.Set(value);
		}
	}

	public bool IsBaseAttack
	{
		get
		{
			return _isBaseAttack.Get();
		}
		set
		{
			_isBaseAttack.Set(value);
		}
	}

	public int SkillIndex
	{
		get
		{
			if (_skillDataHolder.Get() == null)
			{
				return 0;
			}
			return _skillDataHolder.Get().Index;
		}
	}

	public int SkillSlotIndex
	{
		get
		{
			return _skillSlotIndexHolder.Get();
		}
		set
		{
			_skillSlotIndexHolder.Set(value);
		}
	}

	public bool IsSkillLeft
	{
		get
		{
			return _isSkillLeft.Get();
		}
		set
		{
			_isSkillLeft.Set(value);
		}
	}

	public UsingSkillInfo()
	{
		_skillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_firedSkillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_skillLevelHolder = new SnapshotObjectMember<int>(this, 0);
		_triggerInfoHolder = new SnapshotObjectMember<ISkillTriggerInfo>(this);
		_usedTargetCreaturesHolder = new SnapshotObjectMember<SnapshotArray<BattleCreature>>(this);
		_firedTargetCreaturesHolder = new SnapshotObjectMember<SnapshotArray<BattleCreature>>(this);
		_usedTargetPositionHolder = new SnapshotObjectMember<Vector3>(this);
		_startedTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_tickIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_operationCreateHolder = new SnapshotObjectMember<int>(this, 0);
		_durationTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_elapsedTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_onAttackEventAppliedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_stateModifiersHolder = new SnapshotObjectMember<SnapshotArray<DelegateStateModifier>>(this);
		_operationInfosHolder = new SnapshotObjectMember<SnapshotList<OperationInfo>>(this);
		_nextActivatedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_manualTargetHolder = new SnapshotObjectMember<BattleCreature>(this);
		_skillTickTimeListHolder = new SnapshotObjectMember<SnapshotList<SkillTickTime>>(this);
		_ignoreStateTypesHolder = new SnapshotObjectMember<SnapshotArray<string>>(this);
		_isIgnoreCoolTime = new SnapshotObjectMember<bool>(this, value: false);
		_isIgnoreSoloTime = new SnapshotObjectMember<bool>(this, value: false);
		_isCanBeCancel = new SnapshotObjectMember<bool>(this, value: false);
		_isBaseAttack = new SnapshotObjectMember<bool>(this, value: false);
		_skillSlotIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_isSkillLeft = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void Init()
	{
		_operationInfosHolder.Alloc();
	}

	public void InitModified(BattleCreature firedCreature)
	{
		InitSkillTickTime(firedCreature);
		InitIgnoreStateType(firedCreature);
	}

	private void InitSkillTickTime(BattleCreature firedCreature)
	{
		SkillData.TickTimeData[] tickTime = SkillData.TickTime;
		if (tickTime == null || tickTime.Length == 0)
		{
			return;
		}
		SkillData.TickTimeData[] array = new SkillData.TickTimeData[tickTime.Length];
		firedCreature.SkillController.ModifierContainer.FindModifier(SkillData.Index, 0, OperationFieldType.SkillTickTime, 0, out var modifiers, SkillModifier.ModifyType.Set);
		int num = 0;
		while (modifiers != null && num < modifiers.Count)
		{
			string value = modifiers[num].Value;
			SkillData.TickTimeData tickTimeData = SkillData.CreateTickTimeData(0, value);
			for (int i = 0; i < tickTime.Length; i++)
			{
				if (tickTime[i].TickMs == tickTimeData.TickMs)
				{
					tickTimeData.TickIndex = tickTime[i].TickIndex;
					array[tickTimeData.TickIndex] = tickTimeData;
					break;
				}
			}
			num++;
		}
		_skillTickTimeListHolder.Alloc();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] != null)
			{
				SkillTickTime skillTickTime = SnapshotObjectPool<SkillTickTime>.Alloc();
				skillTickTime.Init(array[j]);
				_skillTickTimeListHolder.Get().Add(skillTickTime);
			}
			else
			{
				SkillTickTime skillTickTime2 = SnapshotObjectPool<SkillTickTime>.Alloc();
				skillTickTime2.Init(tickTime[j]);
				_skillTickTimeListHolder.Get().Add(skillTickTime2);
			}
		}
	}

	private void InitIgnoreStateType(BattleCreature firedCreature)
	{
		HashSet<string> hashSet = ((FiredSkillData != null) ? FiredSkillData.GetIgnoreStateTypeList(firedCreature) : null);
		if (hashSet != null)
		{
			_ignoreStateTypesHolder.Alloc().Set(hashSet.ToArray());
		}
	}

	public void Update(int deltaTimeMs)
	{
		ElapsedTimeMs += deltaTimeMs;
	}

	public bool IsUsing()
	{
		if (SkillData == null)
		{
			return false;
		}
		return ElapsedTimeMs < DurationTimeMs;
	}

	public bool IsSoloTime()
	{
		if (SkillData == null)
		{
			return false;
		}
		return ElapsedTimeMs < SkillData.GetSoloTimeMs(IsIgnoreSoloTime);
	}

	public int GetRemainSoloTimeMs()
	{
		return Math.Max(SkillData.GetSoloTimeMs(IsIgnoreSoloTime) - ElapsedTimeMs, 0);
	}

	public bool IsActivated()
	{
		if (SkillData == null)
		{
			return false;
		}
		return ElapsedTimeMs >= SkillData.ActingTimeMs;
	}

	public bool IsCanBeCanceled()
	{
		return IsCanBeCancel;
	}
}
