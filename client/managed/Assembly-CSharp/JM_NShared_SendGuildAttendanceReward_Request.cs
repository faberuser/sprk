using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SendGuildAttendanceReward;

public static class JM_NShared_SendGuildAttendanceReward_Request
{
	public static NShared.SendGuildAttendanceReward.Request Parse(string s)
	{
		return Parse(s, new NShared.SendGuildAttendanceReward.Request());
	}

	public static NShared.SendGuildAttendanceReward.Request Parse(string s, NShared.SendGuildAttendanceReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SendGuildAttendanceReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SendGuildAttendanceReward.Request());
	}

	public static NShared.SendGuildAttendanceReward.Request Parse(IDictionary d, NShared.SendGuildAttendanceReward.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SendGuildAttendanceReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SendGuildAttendanceReward.Request o)
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
