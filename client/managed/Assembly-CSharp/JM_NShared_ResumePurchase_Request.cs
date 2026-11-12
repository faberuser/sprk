using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResumePurchase;

public static class JM_NShared_ResumePurchase_Request
{
	public static NShared.ResumePurchase.Request Parse(string s)
	{
		return Parse(s, new NShared.ResumePurchase.Request());
	}

	public static NShared.ResumePurchase.Request Parse(string s, NShared.ResumePurchase.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResumePurchase.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResumePurchase.Request());
	}

	public static NShared.ResumePurchase.Request Parse(IDictionary d, NShared.ResumePurchase.Request o)
	{
		o.Platform = BaseJsonMarshaler.ParseEnum<EPurchasePlatform>(d, "Platform");
		o.Channel = BaseJsonMarshaler.ParseValueString(d, "Channel");
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.Token = BaseJsonMarshaler.ParseValueString(d, "Token");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.ExtraInfo = BaseJsonMarshaler.ParseValueString(d, "ExtraInfo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResumePurchase.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResumePurchase.Request o)
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
		if (o.DeveloperPayload != null)
		{
			dictionary.Add("DeveloperPayload", o.DeveloperPayload);
		}
		if (o.ProductId != null)
		{
			dictionary.Add("ProductId", o.ProductId);
		}
		if (o.Token != null)
		{
			dictionary.Add("Token", o.Token);
		}
		if (o.OrderId != null)
		{
			dictionary.Add("OrderId", o.OrderId);
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
