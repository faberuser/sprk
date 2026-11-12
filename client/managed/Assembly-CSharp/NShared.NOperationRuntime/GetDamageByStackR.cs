namespace NShared.NOperationRuntime;

public class GetDamageByStackR : GetDamageNoTrigger
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		string valueString = base.OperationInfo.GetValueString(0);
		int num = (base.OperationInfo.GetValueBool(1, defaultValue: true) ? base.OperationInfo.FiredCreature : base.OperationInfo.TargetCreature)?.StateController.GetStateStackCount(valueString) ?? 0;
		int valueInt = base.OperationInfo.GetValueInt(2);
		long valueLong = base.OperationInfo.GetValueLong(3, 0L);
		long num2 = num * valueInt / 1000 + valueLong;
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(4);
		int valueInt2 = base.OperationInfo.GetValueInt(5);
		if (valueInt2 > 0)
		{
			amount = num2 * valueInt2;
		}
		else if (valueInt2 < 0)
		{
			amount = num2 / -valueInt2;
		}
		else
		{
			amount = num2;
		}
		damageFlag = valueFlag;
	}
}
