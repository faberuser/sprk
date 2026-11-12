namespace NShared.NOperationRuntime;

public class DisableSkillSlotToTarget : DisableSkillUse
{
	protected override void OnBegin()
	{
		base.OnBegin();
		string[] valueStrings = base.OperationInfo.GetValueStrings(0, '|');
		_skillIndexHolder.Alloc();
		int num = 0;
		while (valueStrings != null && num < valueStrings.Length)
		{
			string s = valueStrings[num];
			int result = 0;
			if (int.TryParse(s, out result))
			{
				int skillIndexBySlot = base.OperationInfo.Creature.CreatureData.GetSkillIndexBySlot(result);
				if (skillIndexBySlot != 0)
				{
					_skillIndexHolder.Get().Add(skillIndexBySlot);
					if (base.OperationInfo.Creature.SkillController.SkillChangerContainer.GetChangedSkillIndex(skillIndexBySlot, out var resultSkillIndex, out var _))
					{
						_skillIndexHolder.Get().Add(resultSkillIndex);
					}
				}
				int[] linkedSkillIndexBySlot = base.OperationInfo.Creature.CreatureData.GetLinkedSkillIndexBySlot(result);
				if (linkedSkillIndexBySlot != null && linkedSkillIndexBySlot.Length != 0)
				{
					_skillIndexHolder.Get().AddRange(linkedSkillIndexBySlot);
				}
			}
			num++;
		}
		SetDisablerType();
		base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Add(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Remove(this);
	}

	public override void SetDisablerType()
	{
		_disablerTypeHolder.Set(SkillDisablerType.DisableSkillSlot);
	}
}
