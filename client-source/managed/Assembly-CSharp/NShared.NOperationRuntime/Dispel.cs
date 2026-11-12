namespace NShared.NOperationRuntime;

public class Dispel : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StateController.RemovePositiveState();
		base.OperationInfo.Creature.OperationController.RemovePositiveOperation();
	}
}
