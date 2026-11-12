namespace NShared.NOperationRuntime;

public class BeginResting : BaseOperationRuntime
{
	protected override void OnEnd()
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		bool valueBool = base.OperationInfo.GetValueBool(2);
		WakeUpPivotType valueEnum = base.OperationInfo.GetValueEnum<WakeUpPivotType>(3);
		int valueInt3 = base.OperationInfo.GetValueInt(4);
		base.OperationInfo.Creature.BeginResting(valueInt, valueInt2, valueBool, valueEnum, valueInt3);
	}
}
