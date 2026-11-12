using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PingIdleRes
{
	public static PingIdleRes Parse(string s)
	{
		return Parse(s, new PingIdleRes());
	}

	public static PingIdleRes Parse(string s, PingIdleRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PingIdleRes Parse(IDictionary d)
	{
		return Parse(d, new PingIdleRes());
	}

	public static PingIdleRes Parse(IDictionary d, PingIdleRes o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(PingIdleRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PingIdleRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
