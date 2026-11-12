using System;
using System.Collections.Generic;

namespace NShared.NOperationRuntime;

public class ModTriggerStat : BaseOperationRuntime, ITriggerStatModifier, IStatModifier, ISnapshotObject
{
	protected readonly SnapshotObjectMember<SkillTriggerType> _triggerTypeHolder;

	protected readonly SnapshotObjectMember<SnapshotArray<StatType>> _statTypeHolder;

	protected readonly SnapshotObjectMember<int> _multedStatHolder;

	protected readonly SnapshotObjectMember<int> _addedStatHolder;

	protected readonly SnapshotObjectMember<int> _modCountHolder;

	protected readonly SnapshotObjectMember<SnapshotArray<int>> _skillIndicesHolder;

	public ModTriggerStat()
	{
		_triggerTypeHolder = new SnapshotObjectMember<SkillTriggerType>(this, SkillTriggerType.None);
		_statTypeHolder = new SnapshotObjectMember<SnapshotArray<StatType>>(this);
		_multedStatHolder = new SnapshotObjectMember<int>(this, 0);
		_addedStatHolder = new SnapshotObjectMember<int>(this, 0);
		_modCountHolder = new SnapshotObjectMember<int>(this, 0);
		_skillIndicesHolder = new SnapshotObjectMember<SnapshotArray<int>>(this);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		SkillTriggerType valueEnum = base.OperationInfo.GetValueEnum<SkillTriggerType>(0);
		if (valueEnum != SkillTriggerType.None)
		{
			StatType valueEnum2 = base.OperationInfo.GetValueEnum<StatType>(1);
			if (valueEnum2 != StatType.None)
			{
				int valueInt = base.OperationInfo.GetValueInt(2);
				int valueInt2 = base.OperationInfo.GetValueInt(3);
				int valueInt3 = base.OperationInfo.GetValueInt(4);
				List<int> valueIntListFrom = base.OperationInfo.GetValueIntListFrom(5);
				_triggerTypeHolder.Set(valueEnum);
				_statTypeHolder.Set(new StatType[1] { valueEnum2 });
				_multedStatHolder.Set(valueInt);
				_addedStatHolder.Set(valueInt2);
				_modCountHolder.Set(valueInt3);
				_skillIndicesHolder.Alloc().Set(valueIntListFrom?.ToArray());
				base.OperationInfo.Creature.StatController.TriggerStatModifierList.AddLast(this);
			}
		}
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.TriggerStatModifierList.Remove(this);
	}

	protected override bool IsValidState(long mainTimeMs)
	{
		if (!base.IsValidState(mainTimeMs))
		{
			return false;
		}
		return !base.OperationInfo.Expired;
	}

	int[] ITriggerStatModifier.GetSkillIndex()
	{
		return _skillIndicesHolder.Get()?.Get();
	}

	SkillTriggerType ITriggerStatModifier.GetTriggerType()
	{
		return _triggerTypeHolder.Get();
	}

	void ITriggerStatModifier.DecCount()
	{
		int num = Math.Max(_modCountHolder.Get() - 1, 0);
		_modCountHolder.Set(num);
		if (num <= 0)
		{
			base.OperationInfo.Expired = true;
		}
	}

	int IStatModifier.GetMultedStat()
	{
		return _multedStatHolder.Get();
	}

	int IStatModifier.GetAddedStat()
	{
		return _addedStatHolder.Get();
	}

	StatType[] IStatModifier.GetStatTypes()
	{
		return _statTypeHolder.Get().Get();
	}

	long IStatModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}
}
