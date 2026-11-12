using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckPurchase;

public static class JM_NShared_CheckPurchase_Request
{
	public static NShared.CheckPurchase.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckPurchase.Request());
	}

	public static NShared.CheckPurchase.Request Parse(string s, NShared.CheckPurchase.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckPurchase.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckPurchase.Request());
	}

	public static NShared.CheckPurchase.Request Parse(IDictionary d, NShared.CheckPurchase.Request o)
	{
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.PurchaseToken = BaseJsonMarshaler.ParseValueString(d, "PurchaseToken");
		o.CurrencyCode = BaseJsonMarshaler.ParseValueString(d, "CurrencyCode");
		o.Price = BaseJsonMarshaler.ParseValueFloat(d, "Price");
		o.Resume = BaseJsonMarshaler.ParseValueByte(d, "Resume");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckPurchase.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckPurchase.Request o)
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
		dictionary.Add("Resume", o.Resume.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
