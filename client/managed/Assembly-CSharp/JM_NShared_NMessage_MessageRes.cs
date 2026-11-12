using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_MessageRes
{
	public static MessageRes Parse(string s)
	{
		return Parse(s, new MessageRes());
	}

	public static MessageRes Parse(string s, MessageRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MessageRes Parse(IDictionary d)
	{
		return Parse(d, new MessageRes());
	}

	public static MessageRes Parse(IDictionary d, MessageRes o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<MessageResType>(d, "Result");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(MessageRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MessageRes o)
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
