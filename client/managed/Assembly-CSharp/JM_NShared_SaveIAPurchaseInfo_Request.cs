using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SaveIAPurchaseInfo;

public static class JM_NShared_SaveIAPurchaseInfo_Request
{
	public static NShared.SaveIAPurchaseInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.SaveIAPurchaseInfo.Request());
	}

	public static NShared.SaveIAPurchaseInfo.Request Parse(string s, NShared.SaveIAPurchaseInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SaveIAPurchaseInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SaveIAPurchaseInfo.Request());
	}

	public static NShared.SaveIAPurchaseInfo.Request Parse(IDictionary d, NShared.SaveIAPurchaseInfo.Request o)
	{
		o.Platform = BaseJsonMarshaler.ParseValueString(d, "Platform");
		o.PurchaseStatus = BaseJsonMarshaler.ParseValueString(d, "PurchaseStatus");
		o.StatusDescription = BaseJsonMarshaler.ParseValueString(d, "StatusDescription");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.PurchaseToken = BaseJsonMarshaler.ParseValueString(d, "PurchaseToken");
		o.CurrencyCode = BaseJsonMarshaler.ParseValueString(d, "CurrencyCode");
		o.Price = BaseJsonMarshaler.ParseValueFloat(d, "Price");
		o.Resume = BaseJsonMarshaler.ParseValueByte(d, "Resume");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SaveIAPurchaseInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SaveIAPurchaseInfo.Request o)
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
		if (o.DeveloperPayload != null)
		{
			dictionary.Add("DeveloperPayload", o.DeveloperPayload);
		}
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
