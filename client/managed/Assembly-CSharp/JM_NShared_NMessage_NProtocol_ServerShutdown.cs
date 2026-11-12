using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_ServerShutdown
{
	public static ServerShutdown Parse(string s)
	{
		return Parse(s, new ServerShutdown());
	}

	public static ServerShutdown Parse(string s, ServerShutdown o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ServerShutdown Parse(IDictionary d)
	{
		return Parse(d, new ServerShutdown());
	}

	public static ServerShutdown Parse(IDictionary d, ServerShutdown o)
	{
		o.Second = BaseJsonMarshaler.ParseValueInt(d, "Second");
		return o;
	}

	public static string EncodeJsonString(ServerShutdown o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ServerShutdown o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Second", o.Second.ToString());
		return dictionary;
	}
}
