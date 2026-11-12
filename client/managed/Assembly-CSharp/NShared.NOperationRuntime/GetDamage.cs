namespace NShared.NOperationRuntime;

public class GetDamage : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		long amount = 0L;
		DamageFlag damageFlag = DamageFlag.None;
		DamageResultType damageResultType = DamageResultType.Success;
		if (base.OperationInfo.BattleInstance.IsBattleClosingOrDisabled())
		{
			damageResultType = DamageResultType.BattleClosing;
		}
		else if (base.OperationInfo.Creature.StatController.IsDead())
		{
			damageResultType = DamageResultType.AlreadyDied;
		}
		else
		{
			using (new StatModifierBlock(base.OperationInfo.Creature, SkillTriggerType.OnDamage, base.OperationInfo.SkillIndex))
			{
				using (new StatModifierBlock(base.OperationInfo.FiredCreature, SkillTriggerType.OnAttack, base.OperationInfo.SkillIndex))
				{
					GetInstantDamageInfo(out amount, out damageFlag);
					if (amount <= 0)
					{
						return;
					}
					long finalDamage = 0L;
					damageResultType = base.OperationInfo.Creature.StatController.GiveDamage(amount, base.OperationInfo.FiredCreature, base.OperationInfo, base.OperationInfo.Uid, base.OperationInfo.OperationNo, base.OperationInfo.SkillData, base.OperationInfo.FiredSkillData, base.OperationInfo.SkillLevel, 1, string.Empty, ref damageFlag, out finalDamage);
				}
			}
		}
		if (damageResultType != DamageResultType.Success)
		{
			base.OperationInfo.Missed = true;
		}
	}

	public virtual void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		amount = base.OperationInfo.GetValueInt(0);
		damageFlag = StatController.GetDefaultDamageFlag(base.OperationInfo.SkillData);
	}
}
