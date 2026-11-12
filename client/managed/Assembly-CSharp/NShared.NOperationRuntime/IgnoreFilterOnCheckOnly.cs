namespace NShared.NOperationRuntime;

public class IgnoreFilterOnCheckOnly : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.SkillController.SkillTargetController.SetIgnoreFilterOnCheckOnly(ignoreFilterOnCheckOnly: true);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.SkillController.SkillTargetController.SetIgnoreFilterOnCheckOnly(ignoreFilterOnCheckOnly: false);
	}
}
