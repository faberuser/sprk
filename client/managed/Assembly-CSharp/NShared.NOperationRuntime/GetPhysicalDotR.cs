namespace NShared.NOperationRuntime;

public class GetPhysicalDotR : GetDotR
{
	public override DamageFlag GetDamageFlag()
	{
		DamageFlag addedFlag = GetAddedFlag();
		return DamageFlag.Physical | addedFlag;
	}

	public override long GetDamageAmount()
	{
		int damageRatio = GetDamageRatio();
		DamageFlag damageFlag = GetDamageFlag();
		long attackAmountByPower = base.OperationInfo.FiredCreature.StatController.GetAttackAmountByPower(damageRatio, damageFlag);
		int addedDamage = GetAddedDamage();
		return attackAmountByPower + addedDamage;
	}
}
