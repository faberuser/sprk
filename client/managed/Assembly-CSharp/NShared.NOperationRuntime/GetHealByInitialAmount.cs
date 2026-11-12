namespace NShared.NOperationRuntime;

public class GetHealByInitialAmount : GetHeal
{
	public override void GetInstantHealInfo(out long amount, out HealFlag healFlag, out int overHealShieldRatio, out int overHealShieldMinHpRatio)
	{
		if (base.OperationInfo.TriggerInfo is ITriggerByDamage triggerByDamage)
		{
			long initialAmount = triggerByDamage.GetInitialAmount();
			int valueInt = base.OperationInfo.GetValueInt(0);
			overHealShieldRatio = base.OperationInfo.GetValueInt(1);
			overHealShieldMinHpRatio = base.OperationInfo.GetValueInt(2);
			HealFlag valueFlag = base.OperationInfo.GetValueFlag<HealFlag>(3);
			amount = StatController.MultStat(initialAmount, valueInt);
			healFlag = valueFlag | HealFlag.HpSteal;
		}
		else
		{
			amount = 0L;
			healFlag = HealFlag.HpSteal;
			overHealShieldRatio = 0;
			overHealShieldMinHpRatio = 0;
		}
	}
}
