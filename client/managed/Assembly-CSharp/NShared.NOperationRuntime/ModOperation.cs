namespace NShared.NOperationRuntime;

public class ModOperation : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		OperationFieldType valueEnum = base.OperationInfo.GetValueEnum<OperationFieldType>(2);
		string valueString = base.OperationInfo.GetValueString(3);
		int valueInt3 = base.OperationInfo.GetValueInt(4);
		base.OperationInfo.Creature?.OperationController.FindOperationInfo(valueInt, valueInt2)?.Mod(valueEnum, valueString, valueInt3);
	}
}
