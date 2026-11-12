using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NHelper.NInfoHelper;

public class CachedHeroInfoHelper
{
	public static CachedHeroInfo GetFirstHeroInfo()
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(heroInfoList, 0))
		{
			return heroInfoList[0];
		}
		return null;
	}
}
