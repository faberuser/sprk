namespace NShared.NOperationRuntime;

public class GetChainHealR : GetHeal
{
	public override void GetInstantHealInfo(out long amount, out HealFlag healFlag, out int overHealShieldRatio, out int overHealShieldMinHpRatio)
	{
		if (base.OperationInfo.TriggerInfo is ITriggerByHeal triggerByHeal)
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			overHealShieldRatio = base.OperationInfo.GetValueInt(1);
			overHealShieldMinHpRatio = base.OperationInfo.GetValueInt(2);
			amount = StatController.MultStat(triggerByHeal.GetAmount(), valueInt);
			healFlag = HealFlag.None;
		}
		else
		{
			amount = 0L;
			healFlag = HealFlag.None;
			overHealShieldRatio = 0;
			overHealShieldMinHpRatio = 0;
		}
	}
}
