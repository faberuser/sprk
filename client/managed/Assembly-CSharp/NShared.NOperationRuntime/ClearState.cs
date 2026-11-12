namespace NShared.NOperationRuntime;

public class ClearState : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		string valueString = base.OperationInfo.GetValueString(0);
		int valueInt = base.OperationInfo.GetValueInt(1);
		base.OperationInfo.Creature.StateController.RemoveState(valueString, valueInt);
	}
}
