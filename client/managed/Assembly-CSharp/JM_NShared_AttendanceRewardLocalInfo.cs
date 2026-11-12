using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AttendanceRewardLocalInfo
{
	public static AttendanceRewardLocalInfo Parse(string s)
	{
		return Parse(s, new AttendanceRewardLocalInfo());
	}

	public static AttendanceRewardLocalInfo Parse(string s, AttendanceRewardLocalInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AttendanceRewardLocalInfo Parse(IDictionary d)
	{
		return Parse(d, new AttendanceRewardLocalInfo());
	}

	public static AttendanceRewardLocalInfo Parse(IDictionary d, AttendanceRewardLocalInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Day = BaseJsonMarshaler.ParseValueInt(d, "Day");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.Description = BaseJsonMarshaler.ParseValueString(d, "Description");
		o.ImageUrl = BaseJsonMarshaler.ParseValueString(d, "ImageUrl");
		o.DetailTextKey = BaseJsonMarshaler.ParseValueString(d, "DetailTextKey");
		return o;
	}

	public static string EncodeJsonString(AttendanceRewardLocalInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AttendanceRewardLocalInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Day", o.Day.ToString());
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.Description != null)
		{
			dictionary.Add("Description", o.Description);
		}
		if (o.ImageUrl != null)
		{
			dictionary.Add("ImageUrl", o.ImageUrl);
		}
		if (o.DetailTextKey != null)
		{
			dictionary.Add("DetailTextKey", o.DetailTextKey);
		}
		return dictionary;
	}
}
