namespace NShared;

public class OnActiveSkillTrigger : BaseSkillTrigger
{
	private SnapshotObjectMember<SkillTargetType> _targetType;

	private SnapshotObjectMember<int> _targetSkillSlotIndex;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnActiveSkill;

	public void SetSkillTargetType(BattleCreature owner, BattleCreature other)
	{
		if (owner == other)
		{
			int data = ((base.SkillIndex > 0) ? owner.CreatureData.GetSkillSlotByIndex(base.SkillIndex, includeLinkedSkill: true) : 0);
			_targetSkillSlotIndex.Set(data);
			_targetType.Set(SkillTargetType.Myself);
		}
		else if (owner.TeamController.IsFriendly(other.TeamController))
		{
			_targetType.Set(SkillTargetType.Ally);
		}
		else
		{
			_targetType.Set(SkillTargetType.Enemy);
		}
	}

	public OnActiveSkillTrigger()
	{
		_targetType = new SnapshotObjectMember<SkillTargetType>(this, SkillTargetType.Myself);
		_targetSkillSlotIndex = new SnapshotObjectMember<int>(this, 0);
	}

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		if (valueGetter.GetTriggerValueT<SkillTargetType>(firedCreature, 1, out var value) && _targetType.Get() != value)
		{
			return false;
		}
		if (valueGetter.GetTriggerValue(firedCreature, 2, out int value2) && value2 > 0 && _targetSkillSlotIndex.Get() != value2)
		{
			return false;
		}
		return true;
	}
}
