namespace NShared.NOperationRuntime;

public abstract class ModStatByHpBase : SnapshotObject, IOperationRuntime, ISnapshotObject, IStatModifier, IHpObserver
{
	protected readonly SnapshotObjectMember<OperationInfo> _operationHolder;

	protected readonly SnapshotObjectMember<SnapshotArray<StatType>> _statTypesHolder;

	public OperationInfo OperationInfo => _operationHolder.Get();

	public ModStatByHpBase()
	{
		_operationHolder = new SnapshotObjectMember<OperationInfo>(this);
		_statTypesHolder = new SnapshotObjectMember<SnapshotArray<StatType>>(this);
	}

	void IOperationRuntime.Init(OperationInfo operationInfo)
	{
		_operationHolder.Set(operationInfo);
		_statTypesHolder.Set(new StatType[1] { operationInfo.GetValueEnum<StatType>(0) });
	}

	void IOperationRuntime.OnBegin()
	{
		OperationInfo.Creature.StatController.AddHpObserver(this);
		OperationInfo.Creature.StatController.AddModifier(this);
	}

	void IOperationRuntime.OnEnd()
	{
		OperationInfo.Creature.StatController.RemoveHpObserver(this);
		OperationInfo.Creature.StatController.RemoveModifier(this);
	}

	void IOperationRuntime.OnTick()
	{
	}

	public StatType[] GetStatTypes()
	{
		return _statTypesHolder.Get().Get();
	}

	public StatType GetStatType()
	{
		return _statTypesHolder.Get()[0];
	}

	public int GetModifyValue()
	{
		int valueInt = OperationInfo.GetValueInt(1);
		bool valueBool = OperationInfo.GetValueBool(2);
		int num = OperationInfo.Creature.StatController.HpRatioM;
		if (valueBool)
		{
			num = 1000 - num;
		}
		return num * valueInt / 1000;
	}

	protected virtual int GetAddedStat()
	{
		return 0;
	}

	protected virtual int GetMultedStat()
	{
		return 0;
	}

	void IHpObserver.OnHpChanged(long oldHp, long newHp)
	{
		StatType statType = GetStatType();
		OperationInfo.Creature.StatController.MakeStatDirty(statType);
	}

	int IStatModifier.GetAddedStat()
	{
		return GetAddedStat();
	}

	int IStatModifier.GetMultedStat()
	{
		return GetMultedStat();
	}

	long IStatModifier.GetOperationUid()
	{
		return OperationInfo.Uid;
	}
}
