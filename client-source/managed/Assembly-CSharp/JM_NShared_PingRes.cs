using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PingRes
{
	public static PingRes Parse(string s)
	{
		return Parse(s, new PingRes());
	}

	public static PingRes Parse(string s, PingRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PingRes Parse(IDictionary d)
	{
		return Parse(d, new PingRes());
	}

	public static PingRes Parse(IDictionary d, PingRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<PingRes.ResultType>(d, "Result");
		o.ClientTimeMs = BaseJsonMarshaler.ParseValueLong(d, "ClientTimeMs");
		o.ServerTimeMs = BaseJsonMarshaler.ParseValueLong(d, "ServerTimeMs");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(PingRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PingRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("ClientTimeMs", o.ClientTimeMs.ToString());
		dictionary.Add("ServerTimeMs", o.ServerTimeMs.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
