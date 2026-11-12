namespace NShared.NOperationRuntime;

public class GetDamageByDamageR : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		amount = 0L;
		damageFlag = DamageFlag.None;
		if (base.OperationInfo.TriggerInfo is ITriggerByDamage triggerByDamage)
		{
			amount = triggerByDamage.GetInitialAmount();
			damageFlag = triggerByDamage.GetFlag();
			if (amount > 0)
			{
				int valueInt = base.OperationInfo.GetValueInt(0);
				int valueInt2 = base.OperationInfo.GetValueInt(1);
				DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
				amount = StatController.MultStat(amount, valueInt);
				amount += valueInt2;
				damageFlag |= valueFlag;
			}
		}
	}
}
