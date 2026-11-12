using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.Logout;

public static class JM_NShared_Logout_Request
{
	public static NShared.Logout.Request Parse(string s)
	{
		return Parse(s, new NShared.Logout.Request());
	}

	public static NShared.Logout.Request Parse(string s, NShared.Logout.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.Logout.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.Logout.Request());
	}

	public static NShared.Logout.Request Parse(IDictionary d, NShared.Logout.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.Logout.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.Logout.Request o)
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
