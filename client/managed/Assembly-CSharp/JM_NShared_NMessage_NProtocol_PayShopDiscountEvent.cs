using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PayShopDiscountEvent
{
	public static PayShopDiscountEvent Parse(string s)
	{
		return Parse(s, new PayShopDiscountEvent());
	}

	public static PayShopDiscountEvent Parse(string s, PayShopDiscountEvent o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopDiscountEvent Parse(IDictionary d)
	{
		return Parse(d, new PayShopDiscountEvent());
	}

	public static PayShopDiscountEvent Parse(IDictionary d, PayShopDiscountEvent o)
	{
		o.PayShopDiscountInfos = BaseJsonMarshaler.ParseArray(d, "PayShopDiscountInfos", JM_NShared_PayShopDiscountInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PayShopDiscountEvent o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopDiscountEvent o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PayShopDiscountInfos", BaseJsonMarshaler.EncodeArray(o.PayShopDiscountInfos, JM_NShared_PayShopDiscountInfo.EncodeJsonObject));
		return dictionary;
	}
}
