using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class BillingUser : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		bool result = false;
		if (!bool.TryParse(values[1], out result))
		{
			return false;
		}
		bool flag = false;
		PlayerProductPurchaseInfo[] values2 = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.GetValues();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values2); i++)
		{
			if (values2[i].PurchasedCount > 0 && !String_Extension_Method.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(values2[i].ProductIndex).InAppPurchaseId))
			{
				flag = true;
				break;
			}
		}
		if (flag == result)
		{
			return true;
		}
		return false;
	}
}
