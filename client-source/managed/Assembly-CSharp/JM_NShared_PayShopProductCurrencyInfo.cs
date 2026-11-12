using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PayShopProductCurrencyInfo
{
	public static PayShopProductCurrencyInfo Parse(string s)
	{
		return Parse(s, new PayShopProductCurrencyInfo());
	}

	public static PayShopProductCurrencyInfo Parse(string s, PayShopProductCurrencyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopProductCurrencyInfo Parse(IDictionary d)
	{
		return Parse(d, new PayShopProductCurrencyInfo());
	}

	public static PayShopProductCurrencyInfo Parse(IDictionary d, PayShopProductCurrencyInfo o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.Amount = BaseJsonMarshaler.ParseValueInt(d, "Amount");
		return o;
	}

	public static string EncodeJsonString(PayShopProductCurrencyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopProductCurrencyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		dictionary.Add("Amount", o.Amount.ToString());
		return dictionary;
	}
}
