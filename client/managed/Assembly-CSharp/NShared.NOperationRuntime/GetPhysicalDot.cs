namespace NShared.NOperationRuntime;

public class GetPhysicalDot : GetDot
{
	public override long GetDamageAmount()
	{
		return base.OperationInfo.GetValueInt(0);
	}

	public override DamageFlag GetDamageFlag()
	{
		return DamageFlag.Physical;
	}
}
