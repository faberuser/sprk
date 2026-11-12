namespace NShared.NOperationRuntime;

public class ImmuneState : SnapshotObject, IOperationRuntime, ISnapshotObject, IStateDurationModifier
{
	protected readonly SnapshotObjectMember<OperationInfo> _operationHolder;

	protected readonly SnapshotObjectMember<bool> _cancelHolder;

	public OperationInfo OperationInfo => _operationHolder.Get();

	public ImmuneState()
	{
		_operationHolder = new SnapshotObjectMember<OperationInfo>(this);
		_cancelHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	void IOperationRuntime.Init(OperationInfo operationInfo)
	{
		_operationHolder.Set(operationInfo);
		_cancelHolder.Set(operationInfo.GetValueBool(2));
	}

	void IOperationRuntime.OnBegin()
	{
		OperationInfo.Creature.StateController.AddDurationModifier(this);
	}

	void IOperationRuntime.OnEnd()
	{
		OperationInfo.Creature.StateController.RemoveDurationModifier(this);
	}

	void IOperationRuntime.OnTick()
	{
	}

	string[] IStateDurationModifier.GetStateType()
	{
		return OperationInfo.GetValueStrings(0, '|');
	}

	StateFlag IStateDurationModifier.GetStateFlag()
	{
		return OperationInfo.GetValueFlag<StateFlag>(1);
	}

	int IStateDurationModifier.GetAddedDurationMs()
	{
		return 0;
	}

	int IStateDurationModifier.GetMultedDurationMs()
	{
		return 0;
	}

	ImmuneResult IStateDurationModifier.IsImmune()
	{
		if (!_cancelHolder.Get())
		{
			return ImmuneResult.Immune;
		}
		return ImmuneResult.ImmuneAndCancel;
	}
}
