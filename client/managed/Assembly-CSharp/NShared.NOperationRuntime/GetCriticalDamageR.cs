namespace NShared.NOperationRuntime;

public class GetCriticalDamageR : GetDamageR
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		base.GetInstantDamageInfo(out amount, out damageFlag);
		damageFlag |= DamageFlag.Critical;
	}
}
