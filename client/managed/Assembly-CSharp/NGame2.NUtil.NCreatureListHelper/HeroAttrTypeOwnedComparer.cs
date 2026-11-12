using System.Collections.Generic;
using NShared;

namespace NGame2.NUtil.NCreatureListHelper;

public class HeroAttrTypeOwnedComparer : IComparer<CreatureData>
{
	private SkillAttrType _skillType;

	private static HeroAttrTypeOwnedComparer _default;

	public static HeroAttrTypeOwnedComparer Default
	{
		get
		{
			if (_default == null)
			{
				_default = new HeroAttrTypeOwnedComparer(SkillAttrType.None);
			}
			return _default;
		}
	}

	public HeroAttrTypeOwnedComparer(SkillAttrType skillType)
	{
		_skillType = skillType;
	}

	public int Compare(CreatureData lhs, CreatureData rhs)
	{
		int num = HeroOwnedComparer.Default.Compare(lhs, rhs);
		if (num != 0)
		{
			return num;
		}
		return IsSameType(lhs, rhs);
	}

	private int IsSameType(CreatureData lhs, CreatureData rhs)
	{
		bool flag = lhs.HeroAttrType == _skillType;
		bool flag2 = rhs.HeroAttrType == _skillType;
		if (flag && flag2)
		{
			return 0;
		}
		if (flag && !flag2)
		{
			return -1;
		}
		if (!flag && flag2)
		{
			return 1;
		}
		return 0;
	}

	public static HeroAttrTypeOwnedComparer GetComparer(SkillAttrType skilltype)
	{
		Default._skillType = skilltype;
		return Default;
	}
}
