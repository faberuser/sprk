using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckConsume;

public static class JM_NShared_CheckConsume_Request
{
	public static NShared.CheckConsume.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckConsume.Request());
	}

	public static NShared.CheckConsume.Request Parse(string s, NShared.CheckConsume.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckConsume.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckConsume.Request());
	}

	public static NShared.CheckConsume.Request Parse(IDictionary d, NShared.CheckConsume.Request o)
	{
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.PurchaseToken = BaseJsonMarshaler.ParseValueString(d, "PurchaseToken");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.CurrencyCode = BaseJsonMarshaler.ParseValueString(d, "CurrencyCode");
		o.Price = BaseJsonMarshaler.ParseValueFloat(d, "Price");
		o.Resume = BaseJsonMarshaler.ParseValueByte(d, "Resume");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckConsume.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckConsume.Request o)
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
		if (o.PurchaseToken != null)
		{
			dictionary.Add("PurchaseToken", o.PurchaseToken);
		}
		if (o.OrderId != null)
		{
			dictionary.Add("OrderId", o.OrderId);
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
