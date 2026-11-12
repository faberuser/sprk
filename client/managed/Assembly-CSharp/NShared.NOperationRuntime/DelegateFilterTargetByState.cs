namespace NShared.NOperationRuntime;

public class DelegateFilterTargetByState : SnapshotObject, ISkillTargetFilter, ISnapshotObject
{
	protected readonly SnapshotObjectMember<int> _filterSkillIndexHolder;

	protected readonly SnapshotObjectMember<string> _filterStateHolder;

	protected readonly SnapshotObjectMember<bool> _allowNoTargetHolder;

	protected readonly SnapshotObjectMember<FilterTargetByState.FilterTargetFlag> _flagHolder;

	public DelegateFilterTargetByState()
	{
		_filterSkillIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_filterStateHolder = new SnapshotObjectMember<string>(this);
		_allowNoTargetHolder = new SnapshotObjectMember<bool>(this, value: false);
		_flagHolder = new SnapshotObjectMember<FilterTargetByState.FilterTargetFlag>(this, FilterTargetByState.FilterTargetFlag.None);
	}

	public void OnInit(string[] values)
	{
		_filterSkillIndexHolder.Set(ArgParser.GetValueInt(values, 0));
		_filterStateHolder.Set(ArgParser.GetValueString(values, 1));
		_allowNoTargetHolder.Set(ArgParser.GetValueBool(values, 2));
		_flagHolder.Set(ArgParser.GetValueFlag<FilterTargetByState.FilterTargetFlag>(values, 3));
	}

	void ISkillTargetFilter.Filter(ref BattleCreatureList creatureList, SkillTargetFilterInfo filterInfo)
	{
		if (creatureList == null)
		{
			return;
		}
		int num = _filterSkillIndexHolder.Get();
		if ((num != 0 && filterInfo.SkillData != null && filterInfo.SkillData.Index != num) || ((_flagHolder.Get() & FilterTargetByState.FilterTargetFlag.BaseAttack) != FilterTargetByState.FilterTargetFlag.None && (filterInfo.SkillData == null || !filterInfo.SkillData.BaseAttack)) || ((_flagHolder.Get() & FilterTargetByState.FilterTargetFlag.Skill) != FilterTargetByState.FilterTargetFlag.None && (filterInfo.SkillData == null || filterInfo.SkillData.BaseAttack)))
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
