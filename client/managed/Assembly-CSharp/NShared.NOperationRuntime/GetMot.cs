namespace NShared.NOperationRuntime;

public class GetMot : BaseOperationRuntime
{
	protected override void OnTick()
	{
		base.OnTick();
		int manaAmount = GetManaAmount();
		int num = ((base.OperationInfo.StackCount <= 1) ? 1 : base.OperationInfo.StackCount);
		manaAmount *= num;
		if (manaAmount > 0)
		{
			long finalStat = base.OperationInfo.Creature.StatController.GetFinalStat(StatType.MpRegenRatioM);
			manaAmount = (int)(manaAmount * finalStat / 1000);
			base.OperationInfo.Creature.StatController.AddMp(manaAmount, base.OperationInfo.Uid, ChangeMp.ReasonType.Skill, base.OperationInfo.FiredCreature, 0L);
		}
		else if (manaAmount < 0)
		{
			base.OperationInfo.Creature.StatController.ConsumeMp(-manaAmount, base.OperationInfo.Uid, ChangeMp.ReasonType.Skill);
		}
	}

	public virtual int GetManaAmount()
	{
		return base.OperationInfo.GetValueInt(0);
	}

	public virtual string GetManaStateType()
	{
		return base.OperationInfo.GetValueString(1);
	}
}
