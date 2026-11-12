using NShared.NUtil;

namespace NShared;

public class FiredOperationInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<OperationData> _operationDataHolder;

	private readonly SnapshotObjectMember<BattleCreature> _firedCreatureHolder;

	private readonly SnapshotObjectMember<BattleCreature> _targetCreatureHolder;

	private readonly SnapshotObjectMember<SnapshotArray<BattleCreature>> _targetCreaturesHolder;

	private readonly SnapshotObjectMember<Vector3> _firedPositionHolder;

	private readonly SnapshotObjectMember<Vector3> _firedTargetPositionHolder;

	private readonly SnapshotObjectMember<Vector3> _usedTargetPositionHolder;

	private readonly SnapshotObjectMember<ISkillTriggerInfo> _triggerInfoHolder;

	private readonly SnapshotObjectMember<SkillData> _skillDataHolder;

	private readonly SnapshotObjectMember<SkillData> _firedSkillDataHolder;

	private readonly SnapshotObjectMember<int> _skillLevelHolder;

	private readonly SnapshotObjectMember<int> _soloTimeMsHolder;

	private readonly SnapshotObjectMember<int> _cancellableTimeMsHolder;

	private readonly SnapshotObjectMember<int> _tickIndexHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, string>> _modValueDictionaryHolder;

	private readonly SnapshotObjectMember<SnapshotDictionary<int, string>> _setValueDictionaryHolder;

	private readonly SnapshotObjectMember<int> _modDurationMsHolder;

	private readonly SnapshotObjectMember<int> _setDurationMsHolder;

	private readonly SnapshotObjectMember<int> _modTickTimeMsHolder;

	private readonly SnapshotObjectMember<int> _setTickTimeMsHolder;

	private readonly SnapshotObjectMember<bool> _isBaseAttackHolder;

	public OperationData OperationData
	{
		get
		{
			return _operationDataHolder.Get();
		}
		set
		{
			_operationDataHolder.Set(value);
		}
	}

	public BattleCreature FiredCreature
	{
		get
		{
			return _firedCreatureHolder.Get();
		}
		set
		{
			_firedCreatureHolder.Set(value);
		}
	}

	public BattleCreature TargetCreature
	{
		get
		{
			return _targetCreatureHolder.Get();
		}
		set
		{
			_targetCreatureHolder.Set(value);
		}
	}

	public SnapshotArray<BattleCreature> TargetCreatures
	{
		get
		{
			return _targetCreaturesHolder.Get();
		}
		set
		{
			_targetCreaturesHolder.Set(value);
		}
	}

	public Vector3 FiredPosition
	{
		get
		{
			return _firedPositionHolder.Get();
		}
		set
		{
			_firedPositionHolder.Set(value);
		}
	}

	public Vector3 FiredTargetPosition
	{
		get
		{
			return _firedTargetPositionHolder.Get();
		}
		set
		{
			_firedTargetPositionHolder.Set(value);
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

	public int SoloTimeMs
	{
		get
		{
			return _soloTimeMsHolder.Get();
		}
		set
		{
			_soloTimeMsHolder.Set(value);
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

	public bool IsBaseAttack
	{
		get
		{
			return _isBaseAttackHolder.Get();
		}
		set
		{
			_isBaseAttackHolder.Set(value);
		}
	}

	public int OperationNo => OperationData.OperationNo;

	public FiredOperationInfo()
	{
		_operationDataHolder = new SnapshotObjectMember<OperationData>(this, null, OperationData.SnapshotSerializer);
		_firedCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_targetCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_targetCreaturesHolder = new SnapshotObjectMember<SnapshotArray<BattleCreature>>(this);
		_firedPositionHolder = new SnapshotObjectMember<Vector3>(this);
		_firedTargetPositionHolder = new SnapshotObjectMember<Vector3>(this);
		_usedTargetPositionHolder = new SnapshotObjectMember<Vector3>(this);
		_triggerInfoHolder = new SnapshotObjectMember<ISkillTriggerInfo>(this);
		_skillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_firedSkillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_skillLevelHolder = new SnapshotObjectMember<int>(this, 0);
		_soloTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_cancellableTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_tickIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_modValueDictionaryHolder = new SnapshotObjectMember<SnapshotDictionary<int, string>>(this);
		_setValueDictionaryHolder = new SnapshotObjectMember<SnapshotDictionary<int, string>>(this);
		_modDurationMsHolder = new SnapshotObjectMember<int>(this, 0);
		_setDurationMsHolder = new SnapshotObjectMember<int>(this, 0);
		_modTickTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_setTickTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_isBaseAttackHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void Init()
	{
		_setDurationMsHolder.Set(-1);
		_setTickTimeMsHolder.Set(-1);
	}

	public void ModValue(int fieldIndex, string value)
	{
		SnapshotDictionary<int, string> snapshotDictionary = _modValueDictionaryHolder.Get();
		if (snapshotDictionary == null)
		{
			snapshotDictionary = _modValueDictionaryHolder.Alloc();
		}
		snapshotDictionary[fieldIndex] = value;
	}

	public void SetValue(int fieldIndex, string value)
	{
		SnapshotDictionary<int, string> snapshotDictionary = _setValueDictionaryHolder.Get();
		if (snapshotDictionary == null)
		{
			snapshotDictionary = _setValueDictionaryHolder.Alloc();
		}
		snapshotDictionary[fieldIndex] = value;
	}

	public int GetFinalValueFieldCount()
	{
		if (OperationData == null)
		{
			return 0;
		}
		if (OperationData.Values == null)
		{
			return 0;
		}
		return OperationData.Values.Length;
	}

	public string GetFinalValue(int fieldIndex)
	{
		if (OperationData == null)
		{
			return null;
		}
		string text = null;
		string[] values = OperationData.Values;
		if (values != null && fieldIndex < values.Length)
		{
			text = values[fieldIndex];
		}
		SnapshotDictionary<int, string> snapshotDictionary = _setValueDictionaryHolder.Get();
		if (snapshotDictionary != null && snapshotDictionary.ContainsKey(fieldIndex))
		{
			text = snapshotDictionary[fieldIndex];
		}
		SnapshotDictionary<int, string> snapshotDictionary2 = _modValueDictionaryHolder.Get();
		if (snapshotDictionary2 != null && snapshotDictionary2.ContainsKey(fieldIndex))
		{
			string s = ((text != null) ? text : "0");
			string s2 = snapshotDictionary2[fieldIndex];
			long result = 0L;
			long result2 = 0L;
			if (long.TryParse(s, out result) && long.TryParse(s2, out result2))
			{
				text = (result + result2).ToString();
			}
		}
		return text;
	}

	public void ModDurationMs(int value)
	{
		_modDurationMsHolder.Set(value);
	}

	public void SetDurationMs(int value)
	{
		_setDurationMsHolder.Set(value);
	}

	public int GetFinalDurationMs()
	{
		if (OperationData == null)
		{
			return 0;
		}
		int num = OperationData.DurationMs;
		if (_setDurationMsHolder.Get() >= 0)
		{
			num = _setDurationMsHolder.Get();
		}
		if (_modDurationMsHolder.Get() != 0)
		{
			num += _modDurationMsHolder.Get();
		}
		return num;
	}

	public void ModTickTimeMs(int value)
	{
		_modTickTimeMsHolder.Set(value);
	}

	public void SetTickTimeMs(int value)
	{
		_setTickTimeMsHolder.Set(value);
	}

	public int GetFinalTickTimeMs()
	{
		if (OperationData == null)
		{
			return 0;
		}
		int num = OperationData.TickTimeMs;
		if (_setTickTimeMsHolder.Get() >= 0)
		{
			num = _setTickTimeMsHolder.Get();
		}
		if (_modTickTimeMsHolder.Get() != 0)
		{
			num += _modTickTimeMsHolder.Get();
		}
		return num;
	}
}
