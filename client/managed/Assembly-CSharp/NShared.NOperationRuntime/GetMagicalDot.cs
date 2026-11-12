namespace NShared.NOperationRuntime;

public class GetMagicalDot : GetDot
{
	public override long GetDamageAmount()
	{
		return base.OperationInfo.GetValueInt(0);
	}

	public override DamageFlag GetDamageFlag()
	{
		return DamageFlag.Magical;
	}
}
