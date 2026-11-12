using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PingIdleReq
{
	public static PingIdleReq Parse(string s)
	{
		return Parse(s, new PingIdleReq());
	}

	public static PingIdleReq Parse(string s, PingIdleReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PingIdleReq Parse(IDictionary d)
	{
		return Parse(d, new PingIdleReq());
	}

	public static PingIdleReq Parse(IDictionary d, PingIdleReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(PingIdleReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PingIdleReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return SprkConquest.ReplayStatus(dictionary);
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return SprkConquest.ReplayStatus(dictionary);
	}
}
