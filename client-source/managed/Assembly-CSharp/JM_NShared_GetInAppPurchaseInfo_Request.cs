using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetInAppPurchaseInfo;

public static class JM_NShared_GetInAppPurchaseInfo_Request
{
	public static NShared.GetInAppPurchaseInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetInAppPurchaseInfo.Request());
	}

	public static NShared.GetInAppPurchaseInfo.Request Parse(string s, NShared.GetInAppPurchaseInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetInAppPurchaseInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetInAppPurchaseInfo.Request());
	}

	public static NShared.GetInAppPurchaseInfo.Request Parse(IDictionary d, NShared.GetInAppPurchaseInfo.Request o)
	{
		o.Platform = BaseJsonMarshaler.ParseEnum<EPurchasePlatform>(d, "Platform");
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.TransactionId = BaseJsonMarshaler.ParseValueString(d, "TransactionId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetInAppPurchaseInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetInAppPurchaseInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Platform", o.Platform.ToString());
		if (o.DeveloperPayload != null)
		{
			dictionary.Add("DeveloperPayload", o.DeveloperPayload);
		}
		if (o.ProductId != null)
		{
			dictionary.Add("ProductId", o.ProductId);
		}
		if (o.TransactionId != null)
		{
			dictionary.Add("TransactionId", o.TransactionId);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
