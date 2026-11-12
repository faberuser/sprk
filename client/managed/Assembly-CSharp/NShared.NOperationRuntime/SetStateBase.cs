namespace NShared.NOperationRuntime;

public abstract class SetStateBase : BaseOperationRuntime
{
	public override string GetStateType()
	{
		return GetType().Name;
	}
}
