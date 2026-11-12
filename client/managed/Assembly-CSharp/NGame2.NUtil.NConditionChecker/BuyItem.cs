using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class BuyItem : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		int num = 0;
		int result = 0;
		int result2 = 0;
		if (!int.TryParse(values[1], out result2) || !int.TryParse(values[2], out result))
		{
			return false;
		}
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(result2);
		if (playerProductPurchaseInfo != null)
		{
			num = playerProductPurchaseInfo.PurchasedCount;
		}
		if (num >= result)
		{
			return true;
		}
		return false;
	}
}
