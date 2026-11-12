namespace NShared.NOperationRuntime;

public class ClearSkillOperation : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		OperationType[] valueEnums = base.OperationInfo.GetValueEnums<OperationType>(2);
		base.OperationInfo.Creature.OperationController.RemoveOperationOfFiredCreature(0, passiveOnly: false, valueInt, valueInt2, valueEnums);
	}
}
