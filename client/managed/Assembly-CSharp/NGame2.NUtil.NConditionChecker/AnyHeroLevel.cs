using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class AnyHeroLevel : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result) || !bool.TryParse(values[2], out var result2))
		{
			return false;
		}
		bool flag = false;
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
		{
			if (heroInfoList[i].Level >= result)
			{
				flag = true;
				break;
			}
		}
		if (flag == result2)
		{
			return true;
		}
		return false;
	}
}
