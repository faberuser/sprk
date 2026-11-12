using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BeginPurchase;

public static class JM_NShared_BeginPurchase_Request
{
	public static NShared.BeginPurchase.Request Parse(string s)
	{
		return Parse(s, new NShared.BeginPurchase.Request());
	}

	public static NShared.BeginPurchase.Request Parse(string s, NShared.BeginPurchase.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BeginPurchase.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BeginPurchase.Request());
	}

	public static NShared.BeginPurchase.Request Parse(IDictionary d, NShared.BeginPurchase.Request o)
	{
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.Platform = BaseJsonMarshaler.ParseEnum<EPurchasePlatform>(d, "Platform");
		o.Channel = BaseJsonMarshaler.ParseValueString(d, "Channel");
		o.ProductName = BaseJsonMarshaler.ParseValueString(d, "ProductName");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BeginPurchase.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BeginPurchase.Request o)
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
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		dictionary.Add("Platform", o.Platform.ToString());
		if (o.Channel != null)
		{
			dictionary.Add("Channel", o.Channel);
		}
		if (o.ProductName != null)
		{
			dictionary.Add("ProductName", o.ProductName);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
