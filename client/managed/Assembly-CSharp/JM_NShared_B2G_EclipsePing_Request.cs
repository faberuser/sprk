using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_EclipsePing;

public static class JM_NShared_B2G_EclipsePing_Request
{
	public static NShared.B2G_EclipsePing.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_EclipsePing.Request());
	}

	public static NShared.B2G_EclipsePing.Request Parse(string s, NShared.B2G_EclipsePing.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_EclipsePing.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_EclipsePing.Request());
	}

	public static NShared.B2G_EclipsePing.Request Parse(IDictionary d, NShared.B2G_EclipsePing.Request o)
	{
		o.SessionKey = BaseJsonMarshaler.ParseValueString(d, "SessionKey");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_EclipsePing.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_EclipsePing.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionKey != null)
		{
			dictionary.Add("SessionKey", o.SessionKey);
		}
		return dictionary;
	}
}
