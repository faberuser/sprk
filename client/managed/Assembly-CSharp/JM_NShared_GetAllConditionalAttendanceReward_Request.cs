using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetAllConditionalAttendanceReward;

public static class JM_NShared_GetAllConditionalAttendanceReward_Request
{
	public static NShared.GetAllConditionalAttendanceReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetAllConditionalAttendanceReward.Request());
	}

	public static NShared.GetAllConditionalAttendanceReward.Request Parse(string s, NShared.GetAllConditionalAttendanceReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetAllConditionalAttendanceReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetAllConditionalAttendanceReward.Request());
	}

	public static NShared.GetAllConditionalAttendanceReward.Request Parse(IDictionary d, NShared.GetAllConditionalAttendanceReward.Request o)
	{
		o.AttendanceIndex = BaseJsonMarshaler.ParseArrayCast(d, "AttendanceIndex", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetAllConditionalAttendanceReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetAllConditionalAttendanceReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AttendanceIndex", BaseJsonMarshaler.EncodeArray(o.AttendanceIndex));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
