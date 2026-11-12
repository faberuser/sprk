namespace NShared.NOperationRuntime;

public class StartCooltime : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		bool valueBool = base.OperationInfo.GetValueBool(2);
		base.OperationInfo.Creature.SkillController.CooltimeManager.BeginCooltime(valueInt, valueInt2, valueBool);
	}
}
