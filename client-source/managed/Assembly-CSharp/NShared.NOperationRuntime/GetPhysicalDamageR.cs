using NVespa;

namespace NShared.NOperationRuntime;

public class GetPhysicalDamageR : GetDamage
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		if (base.OperationInfo == null)
		{
			amount = 0L;
			damageFlag = DamageFlag.Physical;
			VespaDebug.LogError("OperationInfo is null");
			return;
		}
		if (base.OperationInfo.FiredInfo == null)
		{
			amount = 0L;
			damageFlag = DamageFlag.Physical;
			VespaDebug.LogError("OperationInfo.FiredInfo is null. SkillIndex=" + base.OperationInfo.SkillIndex + " OpNo=" + base.OperationInfo.OperationNo);
			return;
		}
		if (base.OperationInfo.FiredCreature == null)
		{
			amount = 0L;
			damageFlag = DamageFlag.Physical;
			VespaDebug.LogError("OperationInfo.FiredCreature is null. SkillIndex=" + base.OperationInfo.SkillIndex + " OpNo=" + base.OperationInfo.OperationNo);
			return;
		}
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
		damageFlag = DamageFlag.Physical;
		amount = base.OperationInfo.FiredCreature.StatController.GetAttackAmountByPower(valueInt, damageFlag);
		amount += valueInt2;
		damageFlag |= valueFlag;
	}
}
