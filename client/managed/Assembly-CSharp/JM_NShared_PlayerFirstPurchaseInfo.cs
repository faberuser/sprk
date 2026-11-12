using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerFirstPurchaseInfo
{
	public static PlayerFirstPurchaseInfo Parse(string s)
	{
		return Parse(s, new PlayerFirstPurchaseInfo());
	}

	public static PlayerFirstPurchaseInfo Parse(string s, PlayerFirstPurchaseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerFirstPurchaseInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerFirstPurchaseInfo());
	}

	public static PlayerFirstPurchaseInfo Parse(IDictionary d, PlayerFirstPurchaseInfo o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.PurchasedTime = BaseJsonMarshaler.ParseValueString(d, "PurchasedTime");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.PurchaseCountResetTime = BaseJsonMarshaler.ParseValueString(d, "PurchaseCountResetTime");
		return o;
	}

	public static string EncodeJsonString(PlayerFirstPurchaseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerFirstPurchaseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("Index", o.Index.ToString());
		if (o.PurchasedTime != null)
		{
			dictionary.Add("PurchasedTime", o.PurchasedTime);
		}
		dictionary.Add("Count", o.Count.ToString());
		if (o.PurchaseCountResetTime != null)
		{
			dictionary.Add("PurchaseCountResetTime", o.PurchaseCountResetTime);
		}
		return dictionary;
	}
}
