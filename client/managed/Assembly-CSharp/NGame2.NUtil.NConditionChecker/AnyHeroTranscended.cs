using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class AnyHeroTranscended : ConditionChecker.ConditionBase
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
		using PoolingList<CachedHeroInfo> poolingList = default(PoolingList<CachedHeroInfo>);
		poolingList.PushList(GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false));
		for (int i = 0; i < poolingList.COUNT(); i++)
		{
			if (poolingList[i].Transcended >= result)
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
