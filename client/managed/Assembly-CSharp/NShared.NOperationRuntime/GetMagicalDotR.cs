namespace NShared.NOperationRuntime;

public class GetMagicalDotR : GetDotR
{
	public override DamageFlag GetDamageFlag()
	{
		DamageFlag addedFlag = GetAddedFlag();
		return DamageFlag.Magical | addedFlag;
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
