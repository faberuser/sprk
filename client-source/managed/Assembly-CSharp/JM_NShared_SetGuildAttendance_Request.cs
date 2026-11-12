using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetGuildAttendance;

public static class JM_NShared_SetGuildAttendance_Request
{
	public static NShared.SetGuildAttendance.Request Parse(string s)
	{
		return Parse(s, new NShared.SetGuildAttendance.Request());
	}

	public static NShared.SetGuildAttendance.Request Parse(string s, NShared.SetGuildAttendance.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetGuildAttendance.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetGuildAttendance.Request());
	}

	public static NShared.SetGuildAttendance.Request Parse(IDictionary d, NShared.SetGuildAttendance.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetGuildAttendance.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetGuildAttendance.Request o)
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
