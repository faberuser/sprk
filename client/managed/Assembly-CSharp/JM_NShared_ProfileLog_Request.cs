using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ProfileLog;

public static class JM_NShared_ProfileLog_Request
{
	public static NShared.ProfileLog.Request Parse(string s)
	{
		return Parse(s, new NShared.ProfileLog.Request());
	}

	public static NShared.ProfileLog.Request Parse(string s, NShared.ProfileLog.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ProfileLog.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ProfileLog.Request());
	}

	public static NShared.ProfileLog.Request Parse(IDictionary d, NShared.ProfileLog.Request o)
	{
		o.LogItems = BaseJsonMarshaler.ParseArrayCast(d, "LogItems", BaseJsonMarshaler.CastToString);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ProfileLog.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ProfileLog.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LogItems", BaseJsonMarshaler.EncodeArray(o.LogItems));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
