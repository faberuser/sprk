namespace NShared.NOperationRuntime;

public class GetHealByDamageR : GetHeal
{
	public override void GetInstantHealInfo(out long amount, out HealFlag healFlag, out int overHealShieldRatio, out int overHealShieldMinHpRatio)
	{
		if (base.OperationInfo.TriggerInfo is ITriggerByDamage triggerByDamage)
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			HealFlag valueFlag = base.OperationInfo.GetValueFlag<HealFlag>(1);
			overHealShieldRatio = base.OperationInfo.GetValueInt(2);
			overHealShieldMinHpRatio = base.OperationInfo.GetValueInt(3);
			amount = StatController.MultStat(triggerByDamage.GetAmount(), valueInt);
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
