namespace NShared.NOperationRuntime;

public class GetHealByMaxHpR : GetHeal
{
	public override void GetInstantHealInfo(out long amount, out HealFlag healFlag, out int overHealShieldRatio, out int overHealShieldMinHpRatio)
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		HealFlag valueFlag = base.OperationInfo.GetValueFlag<HealFlag>(1);
		overHealShieldRatio = base.OperationInfo.GetValueInt(2);
		overHealShieldMinHpRatio = base.OperationInfo.GetValueInt(3);
		amount = base.OperationInfo.Creature.StatController.GetHpAmountByMaxHp(valueInt);
		healFlag = valueFlag | HealFlag.HpSteal;
	}
}
