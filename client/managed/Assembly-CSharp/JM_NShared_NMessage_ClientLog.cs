using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage;

public static class JM_NShared_NMessage_ClientLog
{
	public static ClientLog Parse(string s)
	{
		return Parse(s, new ClientLog());
	}

	public static ClientLog Parse(string s, ClientLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClientLog Parse(IDictionary d)
	{
		return Parse(d, new ClientLog());
	}

	public static ClientLog Parse(IDictionary d, ClientLog o)
	{
		o.Log = BaseJsonMarshaler.ParseValueString(d, "Log");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ClientLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClientLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Log != null)
		{
			dictionary.Add("Log", o.Log);
		}
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
