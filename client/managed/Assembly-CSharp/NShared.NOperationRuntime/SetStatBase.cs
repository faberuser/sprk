namespace NShared.NOperationRuntime;

public abstract class SetStatBase : BaseOperationRuntime, IStatModifier, ISnapshotObject, ISkillModifyListener
{
	protected readonly SnapshotObjectMember<int> _statHolder;

	public SetStatBase()
	{
		_statHolder = new SnapshotObjectMember<int>(this, 0);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		UpdateStat();
		base.OperationInfo.FiredCreature?.SkillController.ModifierContainer.AddModifyListener(this);
		if (base.OperationInfo.Creature != null)
		{
			base.OperationInfo.Creature.StatController.AddModifier(this);
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveModifier(this);
		base.OperationInfo.FiredCreature?.SkillController.ModifierContainer.RemoveModifyListener(this);
	}

	protected override void OnStackCountChanged()
	{
		base.OnStackCountChanged();
		if (_enabledHolder.Get())
		{
			base.OperationInfo.Creature.StatController.MakeDirtyModifier(this);
		}
	}

	public override void RecalculateOperationValue()
	{
		UpdateStat();
	}

	private void UpdateStat()
	{
		int num = _statHolder.Get();
		int num2 = 0;
		if (_enabledHolder.Get())
		{
			num2 = base.OperationInfo.GetValueInt(0);
			BattleCreature firedCreature = base.OperationInfo.FiredCreature;
			if (firedCreature != null)
			{
				int skillIndex = base.OperationInfo.SkillIndex;
				int operationNo = base.OperationInfo.OperationNo;
				int modifiedInt = firedCreature.SkillController.ModifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.SkillPowerR, 0, 0);
				int modifiedInt2 = firedCreature.SkillController.ModifierContainer.GetModifiedInt(skillIndex, operationNo, OperationFieldType.SkillPower, 0, 0);
				num2 = (int)StatController.MultAndAddStat(num2, modifiedInt, modifiedInt2, allowNegative: true);
			}
		}
		if (num2 != num)
		{
			_statHolder.Set(num2);
			OnStatUpdatedAndMakeDirty();
		}
	}

	public abstract StatType[] GetStatTypes();

	public virtual int GetAddedStat()
	{
		return 0;
	}

	public virtual int GetMultedStat()
	{
		return 0;
	}

	protected virtual void OnStatUpdatedAndMakeDirty()
	{
		MakeCreatureStatDirty();
	}

	protected void MakeCreatureStatDirty()
	{
		if (base.OperationInfo != null)
		{
			base.OperationInfo.Creature?.StatController?.MakeDirtyModifier(this);
		}
	}

	public bool HasStatType(StatType statType)
	{
		StatType[] statTypes = GetStatTypes();
		if (statTypes == null)
		{
			return false;
		}
		for (int i = 0; i < statTypes.Length; i++)
		{
			if (statTypes[i] == statType)
			{
				return true;
			}
		}
		return false;
	}

	long IStatModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	void ISkillModifyListener.OnModified(int skillIndex, int operationNo, OperationFieldType fieldType, int fieldIndex)
	{
		if (skillIndex == base.OperationInfo.SkillIndex && (base.OperationInfo.BattleInstance.DevParam_LegacySkillModifierCheck || operationNo == 0 || operationNo == base.OperationInfo.OperationNo) && (fieldType == OperationFieldType.SkillPower || fieldType == OperationFieldType.SkillPowerR || fieldType == OperationFieldType.Value))
		{
			UpdateStat();
		}
	}
}
