using NVespa;

namespace NShared.NOperationRuntime;

public class SetTriggerTimer : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		PassiveSkillInfo passiveSkill = base.OperationInfo.Creature.SkillController.PassiveSkillContainer.GetPassiveSkill(valueInt);
		if (passiveSkill == null)
		{
			VespaDebug.LogError("No passive skill: " + valueInt);
		}
		else
		{
			passiveSkill.InitTriggerTime(valueInt2);
		}
	}
}
