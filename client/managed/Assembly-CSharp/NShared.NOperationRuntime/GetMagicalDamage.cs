namespace NShared.NOperationRuntime;

public class GetMagicalDamage : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		amount = base.OperationInfo.GetValueInt(0);
		damageFlag = DamageFlag.Magical;
	}
}
