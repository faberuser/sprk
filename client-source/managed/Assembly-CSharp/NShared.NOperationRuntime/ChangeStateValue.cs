namespace NShared.NOperationRuntime;

public class ChangeStateValue : BaseOperationRuntime
{
	protected readonly SnapshotObjectMember<StateFlagModifier> _modifierHolder;

	public ChangeStateValue()
	{
		_modifierHolder = new SnapshotObjectMember<StateFlagModifier>(this);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		StateFlagModifier stateFlagModifier = SnapshotObjectPool<StateFlagModifier>.Alloc();
		stateFlagModifier.State = base.OperationInfo.GetValueString(0);
		stateFlagModifier.Flag = base.OperationInfo.GetValueFlag<StateFlag>(1);
		stateFlagModifier.Value = base.OperationInfo.GetValueString(2);
		base.OperationInfo.Creature.StateController.AddModifier(stateFlagModifier);
		_modifierHolder.Set(stateFlagModifier);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if (_modifierHolder.Get() != null)
		{
			base.OperationInfo.Creature.StateController.RemoveModifier(_modifierHolder.Get());
			_modifierHolder.Set(null);
		}
	}
}
