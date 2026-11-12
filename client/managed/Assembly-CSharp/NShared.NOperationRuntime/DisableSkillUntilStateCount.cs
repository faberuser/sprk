namespace NShared.NOperationRuntime;

public class DisableSkillUntilStateCount : DisableSkillUse
{
	protected readonly SnapshotObjectMember<int> _requireStateCountHolder;

	protected readonly SnapshotObjectMember<string> _checkStateHolder;

	public DisableSkillUntilStateCount()
	{
		_requireStateCountHolder = new SnapshotObjectMember<int>(this, 0);
		_checkStateHolder = new SnapshotObjectMember<string>(this);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		string valueString = base.OperationInfo.GetValueString(1);
		int valueInt = base.OperationInfo.GetValueInt(2);
		_checkStateHolder.Set(valueString);
		_requireStateCountHolder.Set(valueInt);
		UpdateSkillState();
	}

	public override void RecalculateOperationValue()
	{
		UpdateSkillState();
	}

	private void UpdateSkillState()
	{
		if (base.OperationInfo.Creature.StateController.GetStateStackCount(_checkStateHolder.Get()) >= _requireStateCountHolder.Get())
		{
			base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Remove(this);
		}
		else
		{
			base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Add(this);
		}
	}

	public override void SetDisablerType()
	{
		_disablerTypeHolder.Set(SkillDisablerType.StateCount);
	}
}
