namespace NShared.NOperationRuntime;

public class GetDamageR : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
		base.OperationInfo.FiredCreature.StatController.GetAttackAmountAndFlagByPower(base.OperationInfo.SkillData, valueInt, out damageFlag, out amount);
		amount += valueInt2;
		damageFlag |= valueFlag;
	}
}
