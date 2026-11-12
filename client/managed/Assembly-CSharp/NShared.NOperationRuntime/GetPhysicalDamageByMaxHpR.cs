namespace NShared.NOperationRuntime;

public class GetPhysicalDamageByMaxHpR : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
		amount = base.OperationInfo.Creature.StatController.GetHpAmountByMaxHp(valueInt);
		amount += valueInt2;
		damageFlag = DamageFlag.Physical;
		damageFlag |= valueFlag;
	}
}
