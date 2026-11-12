using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class DashToTarget : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		if (!base.OperationInfo.Creature.StateController.CannotMoveState())
		{
			Vector3 position = base.OperationInfo.TargetCreature.TransformController.Position;
			int sizeMm = base.OperationInfo.TargetCreature.CreatureData.SizeMm;
			bool ignoreBlock = base.OperationInfo.SkillData.IgnoreBlock;
			int valueInt = base.OperationInfo.GetValueInt(0);
			int valueInt2 = base.OperationInfo.GetValueInt(1);
			bool valueBool = base.OperationInfo.GetValueBool(2);
			int valueInt3 = base.OperationInfo.GetValueInt(3);
			if (base.OperationInfo.GetValueBool(4))
			{
				ignoreBlock = false;
			}
			base.OperationInfo.Creature.TransformController.Dash(base.OperationInfo.Uid, position, sizeMm, sizeMm, valueInt, valueInt2, valueBool, valueInt3, ignoreBlock);
		}
	}
}
