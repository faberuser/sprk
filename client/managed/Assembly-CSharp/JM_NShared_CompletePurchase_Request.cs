using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CompletePurchase;

public static class JM_NShared_CompletePurchase_Request
{
	public static NShared.CompletePurchase.Request Parse(string s)
	{
		return Parse(s, new NShared.CompletePurchase.Request());
	}

	public static NShared.CompletePurchase.Request Parse(string s, NShared.CompletePurchase.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CompletePurchase.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CompletePurchase.Request());
	}

	public static NShared.CompletePurchase.Request Parse(IDictionary d, NShared.CompletePurchase.Request o)
	{
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.PurchaseToken = BaseJsonMarshaler.ParseValueString(d, "PurchaseToken");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.Resume = BaseJsonMarshaler.ParseValueByte(d, "Resume");
		o.Platform = BaseJsonMarshaler.ParseEnum<EPurchasePlatform>(d, "Platform");
		o.Channel = BaseJsonMarshaler.ParseValueString(d, "Channel");
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.CurrencyCode = BaseJsonMarshaler.ParseValueString(d, "CurrencyCode");
		o.Price = BaseJsonMarshaler.ParseValueString(d, "Price");
		o.ExtraInfo = BaseJsonMarshaler.ParseValueString(d, "ExtraInfo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CompletePurchase.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CompletePurchase.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
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
		dictionary.Add("Resume", o.Resume.ToString());
		dictionary.Add("Platform", o.Platform.ToString());
		if (o.Channel != null)
		{
			dictionary.Add("Channel", o.Channel);
		}
		if (o.DeveloperPayload != null)
		{
			dictionary.Add("DeveloperPayload", o.DeveloperPayload);
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		if (o.CurrencyCode != null)
		{
			dictionary.Add("CurrencyCode", o.CurrencyCode);
		}
		if (o.Price != null)
		{
			dictionary.Add("Price", o.Price);
		}
		if (o.ExtraInfo != null)
		{
			dictionary.Add("ExtraInfo", o.ExtraInfo);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
