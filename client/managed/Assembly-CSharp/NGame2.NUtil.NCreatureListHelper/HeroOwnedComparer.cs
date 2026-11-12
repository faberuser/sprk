using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NCreatureListHelper;

public class HeroOwnedComparer : IComparer<CreatureData>
{
	private static HeroOwnedComparer _default;

	public static HeroOwnedComparer Default
	{
		get
		{
			if (_default == null)
			{
				_default = new HeroOwnedComparer();
			}
			return _default;
		}
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
		return HasHero(lhs, rhs);
	}

	private int HasHero(CreatureData lhs, CreatureData rhs)
	{
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(lhs.Index);
		CachedHeroInfo cachedHeroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(rhs.Index);
		bool flag = cachedHeroInfo == null;
		bool flag2 = cachedHeroInfo2 == null;
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
}
