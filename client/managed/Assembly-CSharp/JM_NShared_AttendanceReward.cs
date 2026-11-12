using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AttendanceReward
{
	public static AttendanceReward Parse(string s)
	{
		return Parse(s, new AttendanceReward());
	}

	public static AttendanceReward Parse(string s, AttendanceReward o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AttendanceReward Parse(IDictionary d)
	{
		return Parse(d, new AttendanceReward());
	}

	public static AttendanceReward Parse(IDictionary d, AttendanceReward o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Day = BaseJsonMarshaler.ParseValueInt(d, "Day");
		o.No = BaseJsonMarshaler.ParseValueInt(d, "No");
		o.Invisible = BaseJsonMarshaler.ParseValueBool(d, "Invisible");
		o.SendMail = BaseJsonMarshaler.ParseValueBool(d, "SendMail");
		o.ShowType = BaseJsonMarshaler.ParseEnum<EAttendanceRewardShowType>(d, "ShowType");
		o.Type = BaseJsonMarshaler.ParseEnum<RewardType>(d, "Type");
		o.Value = BaseJsonMarshaler.ParseValueInt(d, "Value");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		return o;
	}

	public static string EncodeJsonString(AttendanceReward o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AttendanceReward o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Day", o.Day.ToString());
		dictionary.Add("No", o.No.ToString());
		dictionary.Add("Invisible", o.Invisible.ToString());
		dictionary.Add("SendMail", o.SendMail.ToString());
		dictionary.Add("ShowType", o.ShowType.ToString());
		dictionary.Add("Type", o.Type.ToString());
		dictionary.Add("Value", o.Value.ToString());
		dictionary.Add("Count", o.Count.ToString());
		dictionary.Add("Star", o.Star.ToString());
		return dictionary;
	}
}
