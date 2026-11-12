namespace NShared.NOperationRuntime;

public class GetMagicalDamageR : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
		damageFlag = DamageFlag.Magical;
		amount = base.OperationInfo.FiredCreature.StatController.GetAttackAmountByPower(valueInt, damageFlag);
		amount += valueInt2;
		damageFlag |= valueFlag;
	}
}
