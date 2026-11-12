namespace NShared.NOperationRuntime;

public class GetDamageByHealR : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		amount = 0L;
		damageFlag = DamageFlag.None;
		if (base.OperationInfo.TriggerInfo is ITriggerByHeal triggerByHeal)
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			int valueInt2 = base.OperationInfo.GetValueInt(1);
			int valueInt3 = base.OperationInfo.GetValueInt(2);
			DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(3);
			long amount2 = triggerByHeal.GetAmount();
			long overHeal = triggerByHeal.GetOverHeal();
			amount += StatController.MultStat(amount2, valueInt);
			amount += StatController.MultStat(overHeal, valueInt2);
			amount += valueInt3;
			damageFlag |= valueFlag;
		}
	}
}
