namespace NShared.NOperationRuntime;

public class EnablePassiveSkill : BaseOperationRuntime, IPassiveToggler, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _skillIndexHolder;

	protected new readonly SnapshotObjectMember<bool> _enabledHolder;

	public EnablePassiveSkill()
	{
		_skillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_enabledHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		bool valueBool = base.OperationInfo.GetValueBool(1);
		_skillIndexHolder.Set(valueInt);
		_enabledHolder.Set(valueBool);
		if (valueBool && base.OperationInfo.Creature.SkillController.PassiveSkillContainer.GetPassiveSkill(valueInt) == null)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(valueInt);
			int skillLevel = ((base.OperationInfo.SkillLevel <= 0) ? 1 : base.OperationInfo.SkillLevel);
			base.OperationInfo.Creature.SkillController.PassiveSkillContainer.Register(data, skillLevel);
		}
		base.OperationInfo.Creature.SkillController.PassiveSkillContainer.AddPassiveToggler(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.SkillController.PassiveSkillContainer.RemovePassiveToggler(this);
	}

	int IPassiveToggler.GetSkillIndex()
	{
		return _skillIndexHolder.Get();
	}

	bool IPassiveToggler.GetEnabled()
	{
		return _enabledHolder.Get();
	}
}
