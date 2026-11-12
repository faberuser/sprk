using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.WaitMatch;

public static class JM_NShared_WaitMatch_RequestPause
{
	public static RequestPause Parse(string s)
	{
		return Parse(s, new RequestPause());
	}

	public static RequestPause Parse(string s, RequestPause o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RequestPause Parse(IDictionary d)
	{
		return Parse(d, new RequestPause());
	}

	public static RequestPause Parse(IDictionary d, RequestPause o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(RequestPause o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RequestPause o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
