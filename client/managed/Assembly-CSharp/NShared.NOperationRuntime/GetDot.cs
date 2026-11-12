namespace NShared.NOperationRuntime;

public class GetDot : BaseOperationRuntime
{
	protected override void OnTick()
	{
		base.OnTick();
		long damageAmount = GetDamageAmount();
		DamageFlag damageFlag = GetDamageFlag();
		string stateType = GetStateType();
		int stackCount = base.OperationInfo.StackCount;
		damageFlag |= DamageFlag.Dot;
		long finalDamage = 0L;
		base.OperationInfo.Creature.StatController.GiveDamage(damageAmount, base.OperationInfo.FiredCreature, base.OperationInfo, base.OperationInfo.Uid, base.OperationInfo.OperationNo, base.OperationInfo.SkillData, base.OperationInfo.FiredSkillData, base.OperationInfo.SkillLevel, stackCount, stateType, ref damageFlag, out finalDamage);
		int hpStealRatio = GetHpStealRatio();
		if (hpStealRatio > 0 && finalDamage > 0 && base.OperationInfo.FiredCreature != null)
		{
			long amount = finalDamage * hpStealRatio / 1000;
			HealFlag healFlag = HealFlag.HpSteal | HealFlag.Supplemently;
			base.OperationInfo.FiredCreature.StatController.GiveHeal(amount, base.OperationInfo.FiredCreature, 0L, 0, 0, 1, ref healFlag, 0, 0);
		}
	}

	public virtual long GetDamageAmount()
	{
		return base.OperationInfo.GetValueInt(0);
	}

	public virtual DamageFlag GetDamageFlag()
	{
		return StatController.GetDefaultDamageFlag(base.OperationInfo.SkillData);
	}

	public virtual int GetHpStealRatio()
	{
		return 0;
	}
}
