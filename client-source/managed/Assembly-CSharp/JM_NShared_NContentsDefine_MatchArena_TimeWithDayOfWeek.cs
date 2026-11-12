using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_MatchArena_TimeWithDayOfWeek
{
	public static MatchArena.TimeWithDayOfWeek Parse(string s)
	{
		return Parse(s, new MatchArena.TimeWithDayOfWeek());
	}

	public static MatchArena.TimeWithDayOfWeek Parse(string s, MatchArena.TimeWithDayOfWeek o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchArena.TimeWithDayOfWeek Parse(IDictionary d)
	{
		return Parse(d, new MatchArena.TimeWithDayOfWeek());
	}

	public static MatchArena.TimeWithDayOfWeek Parse(IDictionary d, MatchArena.TimeWithDayOfWeek o)
	{
		o.DayOfWeek = BaseJsonMarshaler.ParseEnum<DayOfWeek>(d, "DayOfWeek");
		o.TimeValue = BaseJsonMarshaler.ParseValueString(d, "TimeValue");
		o.IsUtcTime = BaseJsonMarshaler.ParseValueBool(d, "IsUtcTime");
		return o;
	}

	public static string EncodeJsonString(MatchArena.TimeWithDayOfWeek o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchArena.TimeWithDayOfWeek o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DayOfWeek", o.DayOfWeek.ToString());
		if (o.TimeValue != null)
		{
			dictionary.Add("TimeValue", o.TimeValue);
		}
		dictionary.Add("IsUtcTime", o.IsUtcTime.ToString());
		return dictionary;
	}
}
