namespace NShared.NOperationRuntime;

public class GetHotByDamageR : GetHot
{
	public override string GetStateType()
	{
		if (base.OperationInfo.TriggerInfo is ITriggerByDamage)
		{
			return base.GetStateType();
		}
		return string.Empty;
	}

	public override long GetHealAmount()
	{
		if (base.OperationInfo.TriggerInfo is ITriggerByDamage triggerByDamage)
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			long num = StatController.MultStat(triggerByDamage.GetAmount(), valueInt);
			int valueInt2 = base.OperationInfo.GetValueInt(1);
			return num + valueInt2;
		}
		return 0L;
	}

	public override HealFlag GetHealFlag()
	{
		if (base.OperationInfo.TriggerInfo is ITriggerByDamage)
		{
			HealFlag valueFlag = base.OperationInfo.GetValueFlag<HealFlag>(2);
			return HealFlag.HpSteal | valueFlag;
		}
		return HealFlag.HpSteal;
	}
}
