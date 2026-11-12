using System;

namespace NShared.NOperationRuntime;

public class FilterTargetByState : BaseOperationRuntime, ISkillTargetFilter, ISnapshotObject
{
	[Flags]
	public enum FilterTargetFlag
	{
		None = 0,
		BaseAttack = 1,
		Skill = 2
	}

	protected readonly SnapshotObjectMember<int> _filterSkillIndexHolder;

	protected readonly SnapshotObjectMember<string> _filterStateHolder;

	protected readonly SnapshotObjectMember<bool> _allowNoTargetHolder;

	protected readonly SnapshotObjectMember<FilterTargetFlag> _flagHolder;

	public FilterTargetByState()
	{
		_filterSkillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_filterStateHolder = new SnapshotObjectMember<string>(this);
		_allowNoTargetHolder = new SnapshotObjectMember<bool>(this, value: false);
		_flagHolder = new SnapshotObjectMember<FilterTargetFlag>(this, FilterTargetFlag.None);
	}

	protected override void OnInit()
	{
		_filterSkillIndexHolder.Set(base.OperationInfo.GetValueInt(0));
		_filterStateHolder.Set(base.OperationInfo.GetValueString(1));
		_allowNoTargetHolder.Set(base.OperationInfo.GetValueBool(2));
		_flagHolder.Set(base.OperationInfo.GetValueFlag<FilterTargetFlag>(3));
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.SkillController.SkillTargetController.AddTargetFilter(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.SkillController.SkillTargetController.RemoveTargetFilter(this);
	}

	void ISkillTargetFilter.Filter(ref BattleCreatureList creatureList, SkillTargetFilterInfo filterInfo)
	{
		if (creatureList == null)
		{
			return;
		}
		int num = _filterSkillIndexHolder.Get();
		if ((num != 0 && filterInfo.SkillData != null && filterInfo.SkillData.Index != num) || ((_flagHolder.Get() & FilterTargetFlag.BaseAttack) != FilterTargetFlag.None && (filterInfo.SkillData == null || !filterInfo.SkillData.BaseAttack)) || ((_flagHolder.Get() & FilterTargetFlag.Skill) != FilterTargetFlag.None && (filterInfo.SkillData == null || filterInfo.SkillData.BaseAttack)))
		{
			return;
		}
		BattleCreatureList battleCreatureList = null;
		for (int i = 0; i < creatureList.Count; i++)
		{
			BattleCreature battleCreature = creatureList[i];
			if (battleCreature != null && battleCreature.StateController.IsInState(_filterStateHolder.Get()))
			{
				battleCreatureList = battleCreatureList ?? BattleCreatureList.Alloc();
				battleCreatureList.Add(battleCreature);
			}
		}
		if (battleCreatureList != null || _allowNoTargetHolder.Get())
		{
			creatureList = battleCreatureList;
		}
	}
}
