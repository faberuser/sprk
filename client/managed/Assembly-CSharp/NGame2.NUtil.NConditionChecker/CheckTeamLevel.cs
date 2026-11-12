using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CheckTeamLevel : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(values, 1);
		if (GlobalAutoBehaviour<AccountManager>.instance.UserInfo.TeamLevel >= valueInt)
		{
			return true;
		}
		return false;
	}
}
