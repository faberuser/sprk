using System.Collections.Generic;
using NShared;

namespace NGame2.NUtil.NCreatureListHelper;

public class HeroAttrTypeComparer : IComparer<CreatureData>
{
	private SkillAttrType _skillType;

	private static HeroAttrTypeComparer _default;

	public static HeroAttrTypeComparer Default
	{
		get
		{
			if (_default == null)
			{
				_default = new HeroAttrTypeComparer(SkillAttrType.None);
			}
			return _default;
		}
	}

	public HeroAttrTypeComparer(SkillAttrType skillType)
	{
		_skillType = skillType;
	}

	private int ValidCompare(CreatureData lhs, CreatureData rhs)
	{
		bool flag = lhs == null;
		bool flag2 = rhs == null;
		if (flag && flag2)
		{
			return -1;
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

	public int Compare(CreatureData lhs, CreatureData rhs)
	{
		int num = ValidCompare(lhs, rhs);
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

	public static HeroAttrTypeComparer GetComparer(SkillAttrType skilltype)
	{
		Default._skillType = skilltype;
		return Default;
	}
}
