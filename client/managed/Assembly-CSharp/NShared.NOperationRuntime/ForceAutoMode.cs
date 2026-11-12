namespace NShared.NOperationRuntime;

public class ForceAutoMode : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		if (!base.OperationInfo.Creature.CreateInfo.IsAi)
		{
			base.OnBegin();
			base.OperationInfo.Creature.SkillController.CancelReservedSkill();
			base.OperationInfo.Creature.BehaviorController.StartForcedAutoMode(isSmartAi: true);
		}
	}

	protected override void OnEnd()
	{
		if (!base.OperationInfo.Creature.CreateInfo.IsAi)
		{
			base.OnEnd();
			base.OperationInfo.Creature.BehaviorController.StopForcedAutoMode();
		}
	}
}
