namespace NShared;

public class OnFireSlotSkill : BaseSkillTrigger, ITriggerBySlotSkill
{
	private readonly SnapshotObjectMember<int> _slotIndex;

	public override SkillTriggerType TriggerType => SkillTriggerType.OnFireSlotSkill;

	public int SlotIndex
	{
		get
		{
			return _slotIndex.Get();
		}
		set
		{
			_slotIndex.Set(value);
		}
	}

	public OnFireSlotSkill()
	{
		_slotIndex = new SnapshotObjectMember<int>(this, 0);
	}

	int ITriggerBySlotSkill.GetSlotIndex()
	{
		return SlotIndex;
	}
}
