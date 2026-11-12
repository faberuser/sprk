using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PayShopDiscountInfo
{
	public static PayShopDiscountInfo Parse(string s)
	{
		return Parse(s, new PayShopDiscountInfo());
	}

	public static PayShopDiscountInfo Parse(string s, PayShopDiscountInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopDiscountInfo Parse(IDictionary d)
	{
		return Parse(d, new PayShopDiscountInfo());
	}

	public static PayShopDiscountInfo Parse(IDictionary d, PayShopDiscountInfo o)
	{
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.PurchasedCount = BaseJsonMarshaler.ParseValueInt(d, "PurchasedCount");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.DiscountPrice = BaseJsonMarshaler.ParseValueInt(d, "DiscountPrice");
		o.DefaultPrice = BaseJsonMarshaler.ParseValueInt(d, "DefaultPrice");
		o.ServerLimitedCount = BaseJsonMarshaler.ParseValueInt(d, "ServerLimitedCount");
		return o;
	}

	public static string EncodeJsonString(PayShopDiscountInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopDiscountInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		dictionary.Add("PurchasedCount", o.PurchasedCount.ToString());
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("DiscountPrice", o.DiscountPrice.ToString());
		dictionary.Add("DefaultPrice", o.DefaultPrice.ToString());
		dictionary.Add("ServerLimitedCount", o.ServerLimitedCount.ToString());
		return dictionary;
	}
}
