namespace NShared.NOperationRuntime;

public class GetHot : BaseOperationRuntime
{
	protected override void OnTick()
	{
		base.OnTick();
		long healAmount = GetHealAmount();
		HealFlag healFlag = GetHealFlag();
		GetStateType();
		int stackCount = base.OperationInfo.StackCount;
		base.OperationInfo.Creature.StatController.GiveHeal(healAmount, base.OperationInfo.FiredCreature, base.OperationInfo.Uid, base.OperationInfo.OperationNo, base.OperationInfo.SkillIndex, stackCount, ref healFlag, 0, 0);
	}

	public virtual long GetHealAmount()
	{
		return base.OperationInfo.GetValueInt(0);
	}

	public virtual HealFlag GetHealFlag()
	{
		return HealFlag.None;
	}
}
