using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class CreateProjectile : BaseOperationRuntime
{
	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		SkillProjectileData data = GameRoot.Instance.SkillProjectileDataContainer.GetData(valueInt);
		if (data != null)
		{
			BattleCreature firedCreature = base.OperationInfo.FiredCreature;
			int skillLevel = base.OperationInfo.SkillLevel;
			Vector3 direction = base.OperationInfo.FiredCreature.TransformController.Direction;
			BattleCreature followTarget = (data.FollowTarget ? base.OperationInfo.TargetCreature : null);
			Vector3 originPosition = ((data.SpeedMms != 0) ? firedCreature.TransformController.Position : base.OperationInfo.Creature.TransformController.Position);
			int radiusMm = GetRadiusMm();
			base.OperationInfo.BattleInstance.Snapshot.ProjectileContainer.AddProjectile(data, firedCreature, skillLevel, originPosition, direction, valueInt2, followTarget, radiusMm);
		}
	}

	private int GetRadiusMm()
	{
		_ = base.OperationInfo.SkillIndex;
		_ = base.OperationInfo.OperationNo;
		BattleCreature firedCreature = base.OperationInfo.FiredCreature;
		if (firedCreature == null)
		{
			return 0;
		}
		SkillModifierContainer modifierContainer = firedCreature.SkillController.ModifierContainer;
		if (modifierContainer == null)
		{
			return 0;
		}
		int finalModifierInt = modifierContainer.GetFinalModifierInt(OperationFieldType.FinalRadiusMm, int.MaxValue);
		if (finalModifierInt == int.MaxValue)
		{
			return 0;
		}
		return finalModifierInt;
	}
}
