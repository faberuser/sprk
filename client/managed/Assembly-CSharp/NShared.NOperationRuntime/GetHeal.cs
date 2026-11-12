namespace NShared.NOperationRuntime;

public class GetHeal : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		long amount = 0L;
		HealFlag healFlag = HealFlag.None;
		int overHealShieldRatio = 0;
		int overHealShieldMinHpRatio = 0;
		GetInstantHealInfo(out amount, out healFlag, out overHealShieldRatio, out overHealShieldMinHpRatio);
		base.OperationInfo.Creature.StatController.GiveHeal(amount, base.OperationInfo.FiredCreature, base.OperationInfo.Uid, base.OperationInfo.OperationNo, base.OperationInfo.SkillIndex, 1, ref healFlag, overHealShieldRatio, overHealShieldMinHpRatio);
	}

	public virtual void GetInstantHealInfo(out long amount, out HealFlag healFlag, out int overHealShieldRatio, out int overHealShieldMinHpRatio)
	{
		amount = base.OperationInfo.GetValueInt(0);
		healFlag = HealFlag.None;
		overHealShieldRatio = 0;
		overHealShieldMinHpRatio = 0;
	}
}
