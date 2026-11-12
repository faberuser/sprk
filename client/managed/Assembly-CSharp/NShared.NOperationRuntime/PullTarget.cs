using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class PullTarget : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		Vector3 position = base.OperationInfo.Creature.TransformController.Position;
		int sizeMm = base.OperationInfo.Creature.CreatureData.SizeMm;
		bool ignoreBlock = base.OperationInfo.SkillData.IgnoreBlock;
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		bool valueBool = base.OperationInfo.GetValueBool(2);
		int valueInt3 = base.OperationInfo.GetValueInt(3);
		base.OperationInfo.TargetCreature.TransformController.Dash(base.OperationInfo.Uid, position, sizeMm, sizeMm, valueInt, valueInt2, valueBool, valueInt3, ignoreBlock);
	}
}
