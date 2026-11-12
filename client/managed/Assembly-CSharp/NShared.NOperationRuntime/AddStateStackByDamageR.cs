namespace NShared.NOperationRuntime;

public class AddStateStackByDamageR : BaseOperationRuntime
{
	public override int GetAddStateCount()
	{
		if (!(base.OperationInfo.TriggerInfo is ITriggerByDamage triggerByDamage))
		{
			return 0;
		}
		int valueInt = base.OperationInfo.GetValueInt(1);
		int valueInt2 = base.OperationInfo.GetValueInt(2);
		long num = ((valueInt2 > 0) ? (triggerByDamage.GetAmount() * valueInt / 1000 / valueInt2) : ((valueInt2 >= 0) ? (triggerByDamage.GetAmount() * valueInt / 1000) : (triggerByDamage.GetAmount() * valueInt * -valueInt2 / 1000)));
		if (num <= int.MaxValue)
		{
			return (int)num;
		}
		return int.MaxValue;
	}

	public override string GetStateType()
	{
		return base.OperationInfo.GetValueString(0);
	}
}
