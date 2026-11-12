namespace NShared.NOperationRuntime;

public class SetState : SetStateBase
{
	public override string GetStateType()
	{
		return base.OperationInfo.GetValueString(0);
	}
}
