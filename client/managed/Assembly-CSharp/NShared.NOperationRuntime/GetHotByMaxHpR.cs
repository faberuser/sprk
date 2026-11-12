namespace NShared.NOperationRuntime;

public class GetHotByMaxHpR : GetHot
{
	public override long GetHealAmount()
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		long hpAmountByMaxHp = base.OperationInfo.Creature.StatController.GetHpAmountByMaxHp(valueInt);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		return hpAmountByMaxHp + valueInt2;
	}

	public override HealFlag GetHealFlag()
	{
		HealFlag valueFlag = base.OperationInfo.GetValueFlag<HealFlag>(2);
		return HealFlag.None | valueFlag;
	}
}
