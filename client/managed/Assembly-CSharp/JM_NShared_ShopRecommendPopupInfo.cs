using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ShopRecommendPopupInfo
{
	public static ShopRecommendPopupInfo Parse(string s)
	{
		return Parse(s, new ShopRecommendPopupInfo());
	}

	public static ShopRecommendPopupInfo Parse(string s, ShopRecommendPopupInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ShopRecommendPopupInfo Parse(IDictionary d)
	{
		return Parse(d, new ShopRecommendPopupInfo());
	}

	public static ShopRecommendPopupInfo Parse(IDictionary d, ShopRecommendPopupInfo o)
	{
		o.Url = BaseJsonMarshaler.ParseValueString(d, "Url");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		return o;
	}

	public static string EncodeJsonString(ShopRecommendPopupInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShopRecommendPopupInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Url != null)
		{
			dictionary.Add("Url", o.Url);
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		return dictionary;
	}
}
