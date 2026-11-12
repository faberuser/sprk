using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AddPurchaseLog;

public static class JM_NShared_AddPurchaseLog_Request
{
	public static NShared.AddPurchaseLog.Request Parse(string s)
	{
		return Parse(s, new NShared.AddPurchaseLog.Request());
	}

	public static NShared.AddPurchaseLog.Request Parse(string s, NShared.AddPurchaseLog.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AddPurchaseLog.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AddPurchaseLog.Request());
	}

	public static NShared.AddPurchaseLog.Request Parse(IDictionary d, NShared.AddPurchaseLog.Request o)
	{
		o.PurchaseStatus = BaseJsonMarshaler.ParseValueString(d, "PurchaseStatus");
		o.StatusDescription = BaseJsonMarshaler.ParseValueString(d, "StatusDescription");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.PurchaseToken = BaseJsonMarshaler.ParseValueString(d, "PurchaseToken");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.CurrencyCode = BaseJsonMarshaler.ParseValueString(d, "CurrencyCode");
		o.Price = BaseJsonMarshaler.ParseValueString(d, "Price");
		o.Platform = BaseJsonMarshaler.ParseValueString(d, "Platform");
		o.Channel = BaseJsonMarshaler.ParseValueString(d, "Channel");
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.Resume = BaseJsonMarshaler.ParseValueByte(d, "Resume");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AddPurchaseLog.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AddPurchaseLog.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.PurchaseStatus != null)
		{
			dictionary.Add("PurchaseStatus", o.PurchaseStatus);
		}
		if (o.StatusDescription != null)
		{
			dictionary.Add("StatusDescription", o.StatusDescription);
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
		if (o.CurrencyCode != null)
		{
			dictionary.Add("CurrencyCode", o.CurrencyCode);
		}
		if (o.Price != null)
		{
			dictionary.Add("Price", o.Price);
		}
		if (o.Platform != null)
		{
			dictionary.Add("Platform", o.Platform);
		}
		if (o.Channel != null)
		{
			dictionary.Add("Channel", o.Channel);
		}
		if (o.DeveloperPayload != null)
		{
			dictionary.Add("DeveloperPayload", o.DeveloperPayload);
		}
		dictionary.Add("Resume", o.Resume.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
