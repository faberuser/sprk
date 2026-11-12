using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AttendanceRewards
{
	public static AttendanceRewards Parse(string s)
	{
		return Parse(s, new AttendanceRewards());
	}

	public static AttendanceRewards Parse(string s, AttendanceRewards o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AttendanceRewards Parse(IDictionary d)
	{
		return Parse(d, new AttendanceRewards());
	}

	public static AttendanceRewards Parse(IDictionary d, AttendanceRewards o)
	{
		o.Reward = BaseJsonMarshaler.ParseArray(d, "Reward", JM_NShared_AttendanceReward.Parse);
		return o;
	}

	public static string EncodeJsonString(AttendanceRewards o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AttendanceRewards o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Reward", BaseJsonMarshaler.EncodeArray(o.Reward, JM_NShared_AttendanceReward.EncodeJsonObject));
		return dictionary;
	}
}
