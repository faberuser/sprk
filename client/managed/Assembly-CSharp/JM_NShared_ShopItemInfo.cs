using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ShopItemInfo
{
	public static ShopItemInfo Parse(string s)
	{
		return Parse(s, new ShopItemInfo());
	}

	public static ShopItemInfo Parse(string s, ShopItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ShopItemInfo Parse(IDictionary d)
	{
		return Parse(d, new ShopItemInfo());
	}

	public static ShopItemInfo Parse(IDictionary d, ShopItemInfo o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.ListNo = BaseJsonMarshaler.ParseValueInt(d, "ListNo");
		o.Sold = BaseJsonMarshaler.ParseValueByte(d, "Sold");
		o.Purchased = BaseJsonMarshaler.ParseValueInt(d, "Purchased");
		o.PurchasedTime = BaseJsonMarshaler.ParseValueInt(d, "PurchasedTime");
		return o;
	}

	public static string EncodeJsonString(ShopItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShopItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("ListNo", o.ListNo.ToString());
		dictionary.Add("Sold", o.Sold.ToString());
		dictionary.Add("Purchased", o.Purchased.ToString());
		dictionary.Add("PurchasedTime", o.PurchasedTime.ToString());
		return dictionary;
	}
}
