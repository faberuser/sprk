using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ShopListItemInfo
{
	public static ShopListItemInfo Parse(string s)
	{
		return Parse(s, new ShopListItemInfo());
	}

	public static ShopListItemInfo Parse(string s, ShopListItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ShopListItemInfo Parse(IDictionary d)
	{
		return Parse(d, new ShopListItemInfo());
	}

	public static ShopListItemInfo Parse(IDictionary d, ShopListItemInfo o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.ListNo = BaseJsonMarshaler.ParseValueInt(d, "ListNo");
		o.ItemCode = BaseJsonMarshaler.ParseValueString(d, "ItemCode");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.Sold = BaseJsonMarshaler.ParseValueByte(d, "Sold");
		o.PurchasableCount = BaseJsonMarshaler.ParseValueInt(d, "PurchasableCount");
		o.Purchased = BaseJsonMarshaler.ParseValueInt(d, "Purchased");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		return o;
	}

	public static string EncodeJsonString(ShopListItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShopListItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("ListNo", o.ListNo.ToString());
		if (o.ItemCode != null)
		{
			dictionary.Add("ItemCode", o.ItemCode);
		}
		dictionary.Add("Count", o.Count.ToString());
		dictionary.Add("Sold", o.Sold.ToString());
		dictionary.Add("PurchasableCount", o.PurchasableCount.ToString());
		dictionary.Add("Purchased", o.Purchased.ToString());
		dictionary.Add("Star", o.Star.ToString());
		return dictionary;
	}
}
