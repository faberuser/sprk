using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class AnyHero : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 6)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result) || !int.TryParse(values[2], out var result2) || !int.TryParse(values[3], out var result3) || !int.TryParse(values[4], out var result4) || !bool.TryParse(values[5], out var result5))
		{
			return false;
		}
		bool flag = false;
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
		{
			CachedHeroInfo cachedHeroInfo = heroInfoList[i];
			if (cachedHeroInfo.Level >= result && cachedHeroInfo.Star >= result2 && cachedHeroInfo.Transcended >= result3 && cachedHeroInfo.LimitBreakLevel >= result4)
			{
				flag = true;
				break;
			}
		}
		if (flag == result5)
		{
			return true;
		}
		return false;
	}
}
