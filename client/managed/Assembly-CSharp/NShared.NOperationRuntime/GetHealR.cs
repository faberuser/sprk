namespace NShared.NOperationRuntime;

public class GetHealR : GetHeal
{
	public override void GetInstantHealInfo(out long amount, out HealFlag healFlag, out int overHealShieldRatio, out int overHealShieldMinHpRatio)
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		HealFlag valueFlag = base.OperationInfo.GetValueFlag<HealFlag>(2);
		overHealShieldRatio = base.OperationInfo.GetValueInt(3);
		overHealShieldMinHpRatio = base.OperationInfo.GetValueInt(4);
		amount = base.OperationInfo.FiredCreature.StatController.GetHealAmountByPower(valueInt);
		amount += valueInt2;
		healFlag = HealFlag.None;
		healFlag |= valueFlag;
	}
}
