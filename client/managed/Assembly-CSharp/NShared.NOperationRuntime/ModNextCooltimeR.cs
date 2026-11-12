namespace NShared.NOperationRuntime;

public class ModNextCooltimeR : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		int valueInt3 = base.OperationInfo.GetValueInt(2);
		bool valueBool = base.OperationInfo.GetValueBool(3);
		base.OperationInfo.Creature.SkillController.CooltimeManager.ModNextCooltimeRatio(valueInt, valueInt2, valueInt3, valueBool);
	}
}
