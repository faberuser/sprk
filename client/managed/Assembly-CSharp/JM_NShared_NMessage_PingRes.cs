using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_PingRes
{
	public static NShared.NMessage.PingRes Parse(string s)
	{
		return Parse(s, new NShared.NMessage.PingRes());
	}

	public static NShared.NMessage.PingRes Parse(string s, NShared.NMessage.PingRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.NMessage.PingRes Parse(IDictionary d)
	{
		return Parse(d, new NShared.NMessage.PingRes());
	}

	public static NShared.NMessage.PingRes Parse(IDictionary d, NShared.NMessage.PingRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<PingResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(NShared.NMessage.PingRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.NMessage.PingRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Result", o.Result.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
