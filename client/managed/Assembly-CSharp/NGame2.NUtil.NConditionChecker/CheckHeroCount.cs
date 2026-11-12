using NGame2.NAccount;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CheckHeroCount : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) > 2)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result))
		{
			return false;
		}
		int num = __Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false));
		return result <= num;
	}
}
