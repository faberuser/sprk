using System.Collections.Generic;

namespace NShared.NOperationRuntime;

public class SuppressSkillCheck : BaseOperationRuntime, ISkillCheckSuppresser, ISnapshotObject
{
	protected readonly SnapshotObjectMember<SnapshotList<int>> _skillIndexHolder;

	protected readonly SnapshotObjectMember<SkillCheckSuppressType> _suppressTypeHolder;

	public SuppressSkillCheck()
	{
		_skillIndexHolder = new SnapshotObjectMember<SnapshotList<int>>(this);
		_suppressTypeHolder = new SnapshotObjectMember<SkillCheckSuppressType>(this, SkillCheckSuppressType.None);
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		string[] valueStrings = base.OperationInfo.GetValueStrings(0, '|');
		_skillIndexHolder.Alloc();
		int num = 0;
		while (valueStrings != null && num < valueStrings.Length)
		{
			string s = valueStrings[num];
			int result = 0;
			if (int.TryParse(s, out result))
			{
				_skillIndexHolder.Get().Add(result);
			}
			num++;
		}
		SkillCheckSuppressType valueEnum = base.OperationInfo.GetValueEnum<SkillCheckSuppressType>(1);
		_suppressTypeHolder.Set(valueEnum);
		base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Add(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Remove(this);
	}

	IEnumerable<int> ISkillCheckSuppresser.GetSkillIndexList()
	{
		return _skillIndexHolder.Get();
	}

	SkillCheckSuppressType ISkillCheckSuppresser.GetSuppressType()
	{
		return _suppressTypeHolder.Get();
	}
}
