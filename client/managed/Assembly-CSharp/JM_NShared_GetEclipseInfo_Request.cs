using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEclipseInfo;

public static class JM_NShared_GetEclipseInfo_Request
{
	public static NShared.GetEclipseInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEclipseInfo.Request());
	}

	public static NShared.GetEclipseInfo.Request Parse(string s, NShared.GetEclipseInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEclipseInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEclipseInfo.Request());
	}

	public static NShared.GetEclipseInfo.Request Parse(IDictionary d, NShared.GetEclipseInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEclipseInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEclipseInfo.Request o)
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
