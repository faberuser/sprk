namespace NShared.NOperationRuntime;

public class RemovePassiveSkill : BaseOperationRuntime
{
	protected override void OnEnd()
	{
		base.OnEnd();
		int valueInt = base.OperationInfo.GetValueInt(0);
		PassiveSkillInfo passiveSkill = base.OperationInfo.Creature.SkillController.PassiveSkillContainer.GetPassiveSkill(valueInt);
		if (passiveSkill != null)
		{
			base.OperationInfo.Creature.SkillController.PassiveSkillContainer.Unregister(passiveSkill);
		}
	}
}
