using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerPurchaseInfo
{
	public static PlayerPurchaseInfo Parse(string s)
	{
		return Parse(s, new PlayerPurchaseInfo());
	}

	public static PlayerPurchaseInfo Parse(string s, PlayerPurchaseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerPurchaseInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerPurchaseInfo());
	}

	public static PlayerPurchaseInfo Parse(IDictionary d, PlayerPurchaseInfo o)
	{
		o.Platform = BaseJsonMarshaler.ParseValueString(d, "Platform");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.PurchaseToken = BaseJsonMarshaler.ParseValueString(d, "PurchaseToken");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.PurchaseStatus = BaseJsonMarshaler.ParseValueString(d, "PurchaseStatus");
		o.PurchaseTime = BaseJsonMarshaler.ParseValueString(d, "PurchaseTime");
		return o;
	}

	public static string EncodeJsonString(PlayerPurchaseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerPurchaseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Platform != null)
		{
			dictionary.Add("Platform", o.Platform);
		}
		if (o.ProductId != null)
		{
			dictionary.Add("ProductId", o.ProductId);
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		if (o.PurchaseToken != null)
		{
			dictionary.Add("PurchaseToken", o.PurchaseToken);
		}
		if (o.OrderId != null)
		{
			dictionary.Add("OrderId", o.OrderId);
		}
		if (o.PurchaseStatus != null)
		{
			dictionary.Add("PurchaseStatus", o.PurchaseStatus);
		}
		if (o.PurchaseTime != null)
		{
			dictionary.Add("PurchaseTime", o.PurchaseTime);
		}
		return dictionary;
	}
}
