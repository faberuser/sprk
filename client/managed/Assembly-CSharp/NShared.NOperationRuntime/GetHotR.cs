namespace NShared.NOperationRuntime;

public class GetHotR : GetHot
{
	public override long GetHealAmount()
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		long healAmountByPower = base.OperationInfo.FiredCreature.StatController.GetHealAmountByPower(valueInt);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		return healAmountByPower + valueInt2;
	}

	public override HealFlag GetHealFlag()
	{
		HealFlag valueFlag = base.OperationInfo.GetValueFlag<HealFlag>(2);
		return HealFlag.None | valueFlag;
	}
}
