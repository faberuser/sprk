using System.Collections;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenPurchaseMarketingGroupPopup : IActivity
{
	private MarketingType _type;

	public static IActivity Create(string[] values)
	{
		GlobalAutoBehaviour<PurchaseMarketingManager>.CreateInstance();
		return new OpenPurchaseMarketingGroupPopup(Vespa_String_Extension_Method.TOENUM((__Extension_Method_ICollection__.COUNT(values) > 0) ? values[0].ToString() : null, MarketingType.None));
	}

	public OpenPurchaseMarketingGroupPopup(MarketingType type)
	{
		_type = type;
	}

	IEnumerator IActivity.coStart()
	{
		GlobalAutoBehaviour<PurchaseMarketingManager>.instance.OpenPurchaseMarketingGroupPopup(_type);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
