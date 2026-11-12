using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AdminCheckConsume;

public static class JM_NShared_AdminCheckConsume_Request
{
	public static NShared.AdminCheckConsume.Request Parse(string s)
	{
		return Parse(s, new NShared.AdminCheckConsume.Request());
	}

	public static NShared.AdminCheckConsume.Request Parse(string s, NShared.AdminCheckConsume.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AdminCheckConsume.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AdminCheckConsume.Request());
	}

	public static NShared.AdminCheckConsume.Request Parse(IDictionary d, NShared.AdminCheckConsume.Request o)
	{
		o.DeveloperPayload = BaseJsonMarshaler.ParseValueString(d, "DeveloperPayload");
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.OrderId = BaseJsonMarshaler.ParseValueString(d, "OrderId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AdminCheckConsume.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AdminCheckConsume.Request o)
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
		if (o.OrderId != null)
		{
			dictionary.Add("OrderId", o.OrderId);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
