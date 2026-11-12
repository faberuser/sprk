using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PayShopProductItemInfo
{
	public static PayShopProductItemInfo Parse(string s)
	{
		return Parse(s, new PayShopProductItemInfo());
	}

	public static PayShopProductItemInfo Parse(string s, PayShopProductItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopProductItemInfo Parse(IDictionary d)
	{
		return Parse(d, new PayShopProductItemInfo());
	}

	public static PayShopProductItemInfo Parse(IDictionary d, PayShopProductItemInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		return o;
	}

	public static string EncodeJsonString(PayShopProductItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopProductItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		return dictionary;
	}
}
