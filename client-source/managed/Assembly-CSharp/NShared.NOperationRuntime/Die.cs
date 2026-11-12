namespace NShared.NOperationRuntime;

public class Die : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.Die(base.OperationInfo.SkillIndex, base.OperationInfo.Uid);
	}
}
