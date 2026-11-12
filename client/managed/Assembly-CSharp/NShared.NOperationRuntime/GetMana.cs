namespace NShared.NOperationRuntime;

public class GetMana : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int amount = 0;
		GetInstantManaInfo(out amount);
		if (amount > 0)
		{
			long finalStat = base.OperationInfo.Creature.StatController.GetFinalStat(StatType.MpRegenRatioM);
			amount = (int)(amount * finalStat / 1000);
			base.OperationInfo.Creature.StatController.AddMp(amount, base.OperationInfo.Uid, ChangeMp.ReasonType.Skill, base.OperationInfo.FiredCreature, 0L);
		}
		else if (amount < 0)
		{
			base.OperationInfo.Creature.StatController.ConsumeMp(-amount, base.OperationInfo.Uid, ChangeMp.ReasonType.Skill);
		}
	}

	public virtual void GetInstantManaInfo(out int amount)
	{
		amount = base.OperationInfo.GetValueInt(0);
	}
}
