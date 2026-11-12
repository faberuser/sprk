using NGame2.NAccount;
using NVespa.NSingleton;

namespace NGame2.NUtil.NModifyChecker;

public class FirstInAppPurchases : ModifyChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Count() == 1)
		{
			return true;
		}
		return false;
	}
}
