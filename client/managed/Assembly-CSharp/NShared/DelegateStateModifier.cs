namespace NShared;

public class DelegateStateModifier : SnapshotObject, IStateModifier, ISnapshotObject
{
	private readonly SnapshotObjectMember<StateData> _stateDataHolder;

	private readonly SnapshotObjectMember<long> _operationUidHolder;

	private readonly SnapshotObjectMember<int> _skillIndexHolder;

	private readonly SnapshotObjectMember<bool> _validHolder;

	private readonly SnapshotObjectMember<long> _expireTimeMsHolder;

	private readonly SnapshotObjectMember<BattleCreature> _firedCreatureHolder;

	private readonly SnapshotObjectMember<int> _stackCountHolder;

	public StateData StateData
	{
		get
		{
			return _stateDataHolder.Get();
		}
		set
		{
			_stateDataHolder.Set(value);
		}
	}

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

	public int SkillIndex
	{
		get
		{
			return _skillIndexHolder.Get();
		}
		set
		{
			_skillIndexHolder.Set(value);
		}
	}

	public bool Valid
	{
		get
		{
			return _validHolder.Get();
		}
		set
		{
			_validHolder.Set(value);
		}
	}

	public long ExpireTimeMs
	{
		get
		{
			return _expireTimeMsHolder.Get();
		}
		set
		{
			_expireTimeMsHolder.Set(value);
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

	public DelegateStateModifier()
	{
		_stateDataHolder = new SnapshotObjectMember<StateData>(this, null, GameRoot.Instance.StateDataContainer);
		_operationUidHolder = new SnapshotObjectMember<long>(this, 0L);
		_skillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_validHolder = new SnapshotObjectMember<bool>(this, value: true);
		_expireTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_firedCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_stackCountHolder = new SnapshotObjectMember<int>(this, 1);
	}

	long IStateModifier.GetOperationUid()
	{
		return OperationUid;
	}

	int IStateModifier.GetOperationNo()
	{
		return 0;
	}

	int IStateModifier.GetSkillIndex()
	{
		return SkillIndex;
	}

	StateData IStateModifier.GetStateData()
	{
		return _stateDataHolder.Get();
	}

	int IStateModifier.GetTickedTimeMs()
	{
		return 0;
	}

	void IStateModifier.SetTickedTimeMs(int tickedTimeMs)
	{
	}

	int IStateModifier.GetStackCount()
	{
		return StackCount;
	}

	void IStateModifier.SetStackCount(int stackCount)
	{
		StackCount = stackCount;
	}

	BattleCreature IStateModifier.GetFiredCreature()
	{
		return FiredCreature;
	}

	bool IStateModifier.IsValid(long mainTimeMs)
	{
		if (!Valid)
		{
			return false;
		}
		if (ExpireTimeMs > 0 && mainTimeMs >= ExpireTimeMs)
		{
			return false;
		}
		return true;
	}

	OperationInfo IStateModifier.GetOperationInfo()
	{
		return null;
	}

	int IStateModifier.GetDurationMs()
	{
		return 0;
	}

	int IStateModifier.GetRemainDurationMs()
	{
		return 0;
	}

	string IStateModifier.Dump()
	{
		return $"DelegateStateModifier(StateType={StateData.StateType}, ExpireTimeMs={ExpireTimeMs}, Valid={Valid})";
	}
}
