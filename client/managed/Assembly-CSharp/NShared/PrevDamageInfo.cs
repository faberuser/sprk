namespace NShared;

public class PrevDamageInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<long> _operationUidHolder;

	private readonly SnapshotObjectMember<BattleCreature> _attackerHolder;

	private readonly SnapshotObjectMember<BattleCreature> _damagedHolder;

	private readonly SnapshotObjectMember<SkillData> _skillDataHolder;

	private readonly SnapshotObjectMember<SkillData> _firedSkillDataHolder;

	private readonly SnapshotObjectMember<int> _skillLevelHolder;

	private readonly SnapshotObjectMember<int> _stackCountHolder;

	private readonly SnapshotObjectMember<string> _stateTypeHolder;

	private readonly SnapshotObjectMember<DamageFlag> _damageFlagHolder;

	private readonly SnapshotObjectMember<long> _initialAmountHolder;

	private readonly SnapshotObjectMember<long> _finalAmountHolder;

	public long OperationUid
	{
		get
		{
			return _operationUidHolder.Get();
		}
		set
		{
			_operationUidHolder.Set(value);
		}
	}

	public BattleCreature Attacker
	{
		get
		{
			return _attackerHolder.Get();
		}
		set
		{
			_attackerHolder.Set(value);
		}
	}

	public BattleCreature Damaged
	{
		get
		{
			return _damagedHolder.Get();
		}
		set
		{
			_damagedHolder.Set(value);
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

	public string StateType
	{
		get
		{
			return _stateTypeHolder.Get();
		}
		set
		{
			_stateTypeHolder.Set(value);
		}
	}

	public DamageFlag DamageFlag
	{
		get
		{
			return _damageFlagHolder.Get();
		}
		set
		{
			_damageFlagHolder.Set(value);
		}
	}

	public long InitialAmount
	{
		get
		{
			return _initialAmountHolder.Get();
		}
		set
		{
			_initialAmountHolder.Set(value);
		}
	}

	public long FinalAmount
	{
		get
		{
			return _finalAmountHolder.Get();
		}
		set
		{
			_finalAmountHolder.Set(value);
		}
	}

	public PrevDamageInfo()
	{
		_operationUidHolder = new SnapshotObjectMember<long>(this, 0L);
		_attackerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_damagedHolder = new SnapshotObjectMember<BattleCreature>(this);
		_skillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_firedSkillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_skillLevelHolder = new SnapshotObjectMember<int>(this, 0);
		_stackCountHolder = new SnapshotObjectMember<int>(this, 0);
		_stateTypeHolder = new SnapshotObjectMember<string>(this);
		_damageFlagHolder = new SnapshotObjectMember<DamageFlag>(this, DamageFlag.None);
		_initialAmountHolder = new SnapshotObjectMember<long>(this, 0L);
		_finalAmountHolder = new SnapshotObjectMember<long>(this, 0L);
	}
}
