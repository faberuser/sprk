namespace NShared.NOperationRuntime;

public class ModSetRadiusMm : BaseOperationRuntime
{
	protected readonly SnapshotObjectMember<SkillModifier> _finalModifierHolder;

	public ModSetRadiusMm()
	{
		_finalModifierHolder = new SnapshotObjectMember<SkillModifier>(this);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		int valueInt = base.OperationInfo.GetValueInt(0);
		SetSkillModifierRadiusMm(base.OperationInfo.Creature, valueInt);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		RemoveSkillModifierRadiusMm(base.OperationInfo.Creature);
	}

	private void SetSkillModifierRadiusMm(BattleCreature targetCreature, int radiusMm)
	{
		if (targetCreature != null && !targetCreature.StatController.IsDead())
		{
			SkillModifierContainer modifierContainer = targetCreature.SkillController.ModifierContainer;
			if (modifierContainer != null)
			{
				SkillModifier skillModifier = CreateSkillModifier(radiusMm);
				modifierContainer.AddFinalModifier(skillModifier);
				_finalModifierHolder.Set(skillModifier);
			}
		}
	}

	private void RemoveSkillModifierRadiusMm(BattleCreature targetCreature)
	{
		if (targetCreature != null)
		{
			SkillModifierContainer modifierContainer = targetCreature.SkillController.ModifierContainer;
			if (modifierContainer != null)
			{
				SkillModifier finalModifier = _finalModifierHolder.Get();
				modifierContainer.RemoveFinalModifer(finalModifier);
			}
		}
	}

	private SkillModifier CreateSkillModifier(int radiusMm)
	{
		return new SkillModifier
		{
			FieldType = OperationFieldType.FinalRadiusMm,
			NumberValue = radiusMm
		};
	}
}
