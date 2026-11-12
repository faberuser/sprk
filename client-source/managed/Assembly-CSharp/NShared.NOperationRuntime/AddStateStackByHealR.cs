namespace NShared.NOperationRuntime;

public class AddStateStackByHealR : BaseOperationRuntime
{
	public override int GetAddStateCount()
	{
		if (!(base.OperationInfo.TriggerInfo is ITriggerByHeal triggerByHeal))
		{
			return 0;
		}
		long num = triggerByHeal.GetAmount() - triggerByHeal.GetOverHeal();
		long overHeal = triggerByHeal.GetOverHeal();
		int valueInt = base.OperationInfo.GetValueInt(1);
		int valueInt2 = base.OperationInfo.GetValueInt(2);
		int valueInt3 = base.OperationInfo.GetValueInt(3);
		long num2 = num * valueInt / 1000;
		long num3 = overHeal * valueInt * valueInt2 / 1000000;
		long num4 = (num2 + num3) / valueInt3;
		if (num4 <= int.MaxValue)
		{
			return (int)num4;
		}
		return int.MaxValue;
	}

	public override string GetStateType()
	{
		return base.OperationInfo.GetValueString(0);
	}
}
