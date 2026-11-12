using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PingReq
{
	public static PingReq Parse(string s)
	{
		return Parse(s, new PingReq());
	}

	public static PingReq Parse(string s, PingReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PingReq Parse(IDictionary d)
	{
		return Parse(d, new PingReq());
	}

	public static PingReq Parse(IDictionary d, PingReq o)
	{
		o.ClientTimeMs = BaseJsonMarshaler.ParseValueLong(d, "ClientTimeMs");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(PingReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PingReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return SprkConquest.ReplayStatus(dictionary);
		}
		dictionary.Add("ClientTimeMs", o.ClientTimeMs.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return SprkConquest.ReplayStatus(dictionary);
	}
}
