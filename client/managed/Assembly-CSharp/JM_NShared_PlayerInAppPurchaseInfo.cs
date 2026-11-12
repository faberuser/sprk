using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerInAppPurchaseInfo
{
	public static PlayerInAppPurchaseInfo Parse(string s)
	{
		return Parse(s, new PlayerInAppPurchaseInfo());
	}

	public static PlayerInAppPurchaseInfo Parse(string s, PlayerInAppPurchaseInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerInAppPurchaseInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerInAppPurchaseInfo());
	}

	public static PlayerInAppPurchaseInfo Parse(IDictionary d, PlayerInAppPurchaseInfo o)
	{
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.Platform = BaseJsonMarshaler.ParseValueString(d, "Platform");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.PurchaseStatus = BaseJsonMarshaler.ParseValueString(d, "PurchaseStatus");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.PurchaseToken = BaseJsonMarshaler.ParseValueString(d, "PurchaseToken");
		o.CurrencyCode = BaseJsonMarshaler.ParseValueString(d, "CurrencyCode");
		o.Price = BaseJsonMarshaler.ParseValueFloat(d, "Price");
		return o;
	}

	public static string EncodeJsonString(PlayerInAppPurchaseInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerInAppPurchaseInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.DeveloperPayload != null)
		{
			dictionary.Add("DeveloperPayload", o.DeveloperPayload);
		}
		if (o.Platform != null)
		{
			dictionary.Add("Platform", o.Platform);
		}
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		if (o.PurchaseStatus != null)
		{
			dictionary.Add("PurchaseStatus", o.PurchaseStatus);
		}
		if (o.ProductId != null)
		{
			dictionary.Add("ProductId", o.ProductId);
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		if (o.OrderId != null)
		{
			dictionary.Add("OrderId", o.OrderId);
		}
		if (o.PurchaseToken != null)
		{
			dictionary.Add("PurchaseToken", o.PurchaseToken);
		}
		if (o.CurrencyCode != null)
		{
			dictionary.Add("CurrencyCode", o.CurrencyCode);
		}
		dictionary.Add("Price", o.Price.ToString());
		return dictionary;
	}
}
