using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.InitPurchase;

public static class JM_NShared_InitPurchase_Request
{
	public static NShared.InitPurchase.Request Parse(string s)
	{
		return Parse(s, new NShared.InitPurchase.Request());
	}

	public static NShared.InitPurchase.Request Parse(string s, NShared.InitPurchase.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.InitPurchase.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.InitPurchase.Request());
	}

	public static NShared.InitPurchase.Request Parse(IDictionary d, NShared.InitPurchase.Request o)
	{
		o.Platform = BaseJsonMarshaler.ParseEnum<EPurchasePlatform>(d, "Platform");
		o.Channel = BaseJsonMarshaler.ParseValueString(d, "Channel");
		o.PurchaseInfos = BaseJsonMarshaler.ParseValueString(d, "PurchaseInfos");
		o.SteamId = BaseJsonMarshaler.ParseValueString(d, "SteamId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.InitPurchase.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.InitPurchase.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Platform", o.Platform.ToString());
		if (o.Channel != null)
		{
			dictionary.Add("Channel", o.Channel);
		}
		if (o.PurchaseInfos != null)
		{
			dictionary.Add("PurchaseInfos", o.PurchaseInfos);
		}
		if (o.SteamId != null)
		{
			dictionary.Add("SteamId", o.SteamId);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
