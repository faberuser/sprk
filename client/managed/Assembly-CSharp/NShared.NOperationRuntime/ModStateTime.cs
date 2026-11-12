namespace NShared.NOperationRuntime;

public class ModStateTime : BaseOperationRuntime, IStateDurationModifier, ISnapshotObject
{
	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StateController.AddDurationModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StateController.RemoveDurationModifier(this);
	}

	string[] IStateDurationModifier.GetStateType()
	{
		return base.OperationInfo.GetValueStrings(0, '|');
	}

	StateFlag IStateDurationModifier.GetStateFlag()
	{
		return base.OperationInfo.GetValueFlag<StateFlag>(2);
	}

	int IStateDurationModifier.GetAddedDurationMs()
	{
		return base.OperationInfo.GetValueInt(1);
	}

	int IStateDurationModifier.GetMultedDurationMs()
	{
		return 0;
	}

	ImmuneResult IStateDurationModifier.IsImmune()
	{
		return ImmuneResult.None;
	}
}
