using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.WaitMatch;

public static class JM_NShared_WaitMatch_RequestResume
{
	public static RequestResume Parse(string s)
	{
		return Parse(s, new RequestResume());
	}

	public static RequestResume Parse(string s, RequestResume o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RequestResume Parse(IDictionary d)
	{
		return Parse(d, new RequestResume());
	}

	public static RequestResume Parse(IDictionary d, RequestResume o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(RequestResume o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RequestResume o)
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
