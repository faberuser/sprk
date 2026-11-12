using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetAttendanceReward;

public static class JM_NShared_GetAttendanceReward_Request
{
	public static NShared.GetAttendanceReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetAttendanceReward.Request());
	}

	public static NShared.GetAttendanceReward.Request Parse(string s, NShared.GetAttendanceReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetAttendanceReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetAttendanceReward.Request());
	}

	public static NShared.GetAttendanceReward.Request Parse(IDictionary d, NShared.GetAttendanceReward.Request o)
	{
		o.AttendanceIndex = BaseJsonMarshaler.ParseValueInt(d, "AttendanceIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetAttendanceReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetAttendanceReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AttendanceIndex", o.AttendanceIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
