using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class DashFromMyself : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		if (!base.OperationInfo.Creature.StateController.CannotMoveState())
		{
			int valueInt = base.OperationInfo.GetValueInt(0);
			int valueInt2 = base.OperationInfo.GetValueInt(1);
			bool valueBool = base.OperationInfo.GetValueBool(2);
			int valueInt3 = base.OperationInfo.GetValueInt(3);
			bool valueBool2 = base.OperationInfo.GetValueBool(4);
			Vector3 targetPosition = base.OperationInfo.Creature.TransformController.Position + valueInt * base.OperationInfo.Creature.TransformController.Direction / 1000L;
			bool ignoreBlock = base.OperationInfo.SkillData.IgnoreBlock;
			int sizeMm = base.OperationInfo.TargetCreature.CreatureData.SizeMm;
			if (valueBool2)
			{
				ignoreBlock = false;
			}
			base.OperationInfo.Creature.TransformController.Dash(base.OperationInfo.Uid, targetPosition, sizeMm, 0, 0, valueInt2, valueBool, valueInt3, ignoreBlock);
		}
	}
}
