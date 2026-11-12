namespace NShared.NOperationRuntime;

public class Cleanse : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int num = 0;
		num += base.OperationInfo.Creature.StateController.RemoveNegativeState();
		num += base.OperationInfo.Creature.OperationController.RemoveNegativeOperation();
		base.OperationInfo.Missed = num == 0;
	}
}
