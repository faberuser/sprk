namespace NShared.NOperationRuntime;

public class GetThreat : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		long amount = 0L;
		GetInstantThreatInfo(out amount);
		base.OperationInfo.Creature?.StatController.GainThreat(amount, base.OperationInfo.SkillIndex);
	}

	public virtual void GetInstantThreatInfo(out long amount)
	{
		amount = base.OperationInfo.GetValueInt(0);
	}
}
