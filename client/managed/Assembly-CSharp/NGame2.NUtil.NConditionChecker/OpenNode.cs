using NGame2.NAccount;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class OpenNode : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		if (int.TryParse(values[1], out var result) && int.TryParse(values[2], out var result2))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(result, result2);
		}
		return false;
	}
}
