using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MessageServerInfo
{
	public static MessageServerInfo Parse(string s)
	{
		return Parse(s, new MessageServerInfo());
	}

	public static MessageServerInfo Parse(string s, MessageServerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MessageServerInfo Parse(IDictionary d)
	{
		return Parse(d, new MessageServerInfo());
	}

	public static MessageServerInfo Parse(IDictionary d, MessageServerInfo o)
	{
		o.Address = BaseJsonMarshaler.ParseValueString(d, "Address");
		o.Port = BaseJsonMarshaler.ParseValueInt(d, "Port");
		return o;
	}

	public static string EncodeJsonString(MessageServerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MessageServerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Address != null)
		{
			dictionary.Add("Address", o.Address);
		}
		dictionary.Add("Port", o.Port.ToString());
		return dictionary;
	}
}
