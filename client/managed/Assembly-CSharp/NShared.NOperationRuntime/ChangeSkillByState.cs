namespace NShared.NOperationRuntime;

public class ChangeSkillByState : BaseOperationRuntime, ISkillChanger, ISnapshotObject
{
	private readonly SnapshotObjectMember<int> _fromSkillIndexHolder;

	private readonly SnapshotObjectMember<int> _toSkillIndexHolder;

	private readonly SnapshotObjectMember<bool> _isReplacedHolder;

	public ChangeSkillByState()
	{
		_fromSkillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_toSkillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_isReplacedHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		_fromSkillIndexHolder.Set(base.OperationInfo.GetValueInt(0));
		_toSkillIndexHolder.Set(base.OperationInfo.GetValueInt(1));
		_isReplacedHolder.Set(base.OperationInfo.GetValueBool(2));
		if (_isReplacedHolder.Get())
		{
			base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.CheckDisabledSkillSlot(this);
		}
		base.OperationInfo.Creature.SkillController.SkillChangerContainer.Register(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.SkillController.SkillChangerContainer.Unregister(this);
	}

	int ISkillChanger.FromSkillIndex()
	{
		return _fromSkillIndexHolder.Get();
	}

	int ISkillChanger.ToSkillIndex()
	{
		return _toSkillIndexHolder.Get();
	}

	bool ISkillChanger.IsReplaced()
	{
		return _isReplacedHolder.Get();
	}
}
