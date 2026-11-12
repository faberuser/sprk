using System;

namespace NShared.NOperationRuntime;

public abstract class ModSkillBase : BaseOperationRuntime, ISkillModifyListener, ISnapshotObject
{
	protected readonly SnapshotObjectMember<SkillModifier> _modifierHolder;

	public ModSkillBase()
	{
		_modifierHolder = new SnapshotObjectMember<SkillModifier>(this);
	}

	protected abstract SkillModifier.ModifyType GetModifyType();

	protected abstract int GetModifySkillIndex();

	protected abstract int GetModifyOperationNo();

	protected abstract OperationFieldType GetModifyFieldType();

	protected abstract int GetModifyFieldIndex();

	protected abstract string GetModifyValue();

	protected override void OnBegin()
	{
		base.OnBegin();
		SkillModifier skillModifier = SnapshotObjectPool<SkillModifier>.Alloc();
		if (skillModifier != null)
		{
			base.OperationInfo.FiredCreature?.SkillController.ModifierContainer.AddModifyListener(this);
			skillModifier.Type = GetModifyType();
			skillModifier.SkillIndex = GetModifySkillIndex();
			skillModifier.OperationNo = GetModifyOperationNo();
			skillModifier.FieldType = GetModifyFieldType();
			skillModifier.FieldIndex = GetModifyFieldIndex();
			skillModifier.Value = GetModifyValue();
			skillModifier.Init();
			_modifierHolder.Set(skillModifier);
			base.OperationInfo.Creature.SkillController.ModifierContainer.AddModifier(skillModifier);
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		if (_modifierHolder.Get() != null)
		{
			base.OperationInfo.Creature.SkillController.ModifierContainer.RemoveModifier(_modifierHolder.Get());
			_modifierHolder.Set(null);
		}
		base.OperationInfo.FiredCreature?.SkillController.ModifierContainer.RemoveModifyListener(this);
	}

	protected int MultiplyStackCount(int value)
	{
		int num = ((base.OperationInfo.StackCount <= 0) ? 1 : base.OperationInfo.StackCount);
		if (num >= 2)
		{
			return value * num;
		}
		return value;
	}

	public override void RecalculateOperationValue()
	{
		RecalculateOperationValueInternal();
	}

	private void RecalculateOperationValueInternal(int fieldIndex = -1)
	{
		SkillModifier skillModifier = _modifierHolder.Get();
		if (skillModifier != null && (base.OperationInfo.RecalculateValue(fieldIndex) || base.OperationInfo.CheckValueHasExpression()))
		{
			string modifyValue = GetModifyValue();
			string value = skillModifier.Value;
			if ((!string.IsNullOrEmpty(modifyValue) || !string.IsNullOrEmpty(value)) && !string.Equals(modifyValue, value, StringComparison.OrdinalIgnoreCase))
			{
				skillModifier.Value = modifyValue;
				skillModifier.Init();
				base.OperationInfo.Creature.SkillController.ModifierContainer.AnnounceModified(skillModifier);
			}
		}
	}

	void ISkillModifyListener.OnModified(int skillIndex, int operationNo, OperationFieldType fieldType, int fieldIndex)
	{
		if (skillIndex == base.OperationInfo.SkillIndex && (operationNo == 0 || operationNo == base.OperationInfo.OperationNo))
		{
			RecalculateOperationValueInternal(fieldIndex);
		}
	}
}
