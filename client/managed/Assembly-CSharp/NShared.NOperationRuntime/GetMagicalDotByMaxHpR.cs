namespace NShared.NOperationRuntime;

public class GetMagicalDotByMaxHpR : GetMagicalDotR
{
	public override long GetDamageAmount()
	{
		int damageRatio = GetDamageRatio();
		long hpAmountByMaxHp = base.OperationInfo.Creature.StatController.GetHpAmountByMaxHp(damageRatio);
		int addedDamage = GetAddedDamage();
		return hpAmountByMaxHp + addedDamage;
	}
}
