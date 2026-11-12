using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_PingReq
{
	public static NShared.NMessage.PingReq Parse(string s)
	{
		return Parse(s, new NShared.NMessage.PingReq());
	}

	public static NShared.NMessage.PingReq Parse(string s, NShared.NMessage.PingReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.NMessage.PingReq Parse(IDictionary d)
	{
		return Parse(d, new NShared.NMessage.PingReq());
	}

	public static NShared.NMessage.PingReq Parse(IDictionary d, NShared.NMessage.PingReq o)
	{
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(NShared.NMessage.PingReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.NMessage.PingReq o)
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
