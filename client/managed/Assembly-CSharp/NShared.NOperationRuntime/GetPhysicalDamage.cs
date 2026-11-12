namespace NShared.NOperationRuntime;

public class GetPhysicalDamage : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		amount = base.OperationInfo.GetValueInt(0);
		damageFlag = DamageFlag.Physical;
	}
}
