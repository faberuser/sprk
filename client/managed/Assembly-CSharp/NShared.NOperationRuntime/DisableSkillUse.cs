using System.Collections.Generic;

namespace NShared.NOperationRuntime;

public class DisableSkillUse : BaseOperationRuntime, ISkillDisabler, ISnapshotObject
{
	protected readonly SnapshotObjectMember<SnapshotList<int>> _skillIndexHolder;

	protected readonly SnapshotObjectMember<SkillDisablerType> _disablerTypeHolder;

	public DisableSkillUse()
	{
		_skillIndexHolder = new SnapshotObjectMember<SnapshotList<int>>(this);
		_disablerTypeHolder = new SnapshotObjectMember<SkillDisablerType>(this, SkillDisablerType.None);
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
		SetDisablerType();
		base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Add(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.SkillController.SkillCheckModifierContainer.Remove(this);
	}

	public virtual void SetDisablerType()
	{
		SkillDisablerType valueEnum = base.OperationInfo.GetValueEnum<SkillDisablerType>(1);
		_disablerTypeHolder.Set(valueEnum);
	}

	IEnumerable<int> ISkillDisabler.GetSkillIndexList()
	{
		return _skillIndexHolder.Get();
	}

	SkillDisablerType ISkillDisabler.GetDisablerType()
	{
		return _disablerTypeHolder.Get();
	}

	void ISkillDisabler.AddSkillIndex(int toSkillIndex)
	{
		if (!_skillIndexHolder.Get().Contains(toSkillIndex))
		{
			_skillIndexHolder.Get().Add(toSkillIndex);
		}
	}

	bool ISkillDisabler.IsDisableAutoUse()
	{
		return SkillDisablerTypeAttribute.IsDisableAutoUse(_disablerTypeHolder.Get());
	}
}
