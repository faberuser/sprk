using NGame2.NAccount;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class IsBuyableProduct : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		int result = 0;
		if (!int.TryParse(values[1], out result))
		{
			return false;
		}
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(result);
		if (newPayShopProductInfo == null)
		{
			return false;
		}
		if (PayShopHelper.IsBuyable(newPayShopProductInfo))
		{
			return !PayShopHelper.IsNewbieTabProductPurchaseable(newPayShopProductInfo);
		}
		return false;
	}
}
