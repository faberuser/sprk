using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventCalendarData
{
	public static EventCalendarData Parse(string s)
	{
		return Parse(s, new EventCalendarData());
	}

	public static EventCalendarData Parse(string s, EventCalendarData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventCalendarData Parse(IDictionary d)
	{
		return Parse(d, new EventCalendarData());
	}

	public static EventCalendarData Parse(IDictionary d, EventCalendarData o)
	{
		o.Id = BaseJsonMarshaler.ParseValueInt(d, "Id");
		o.BonusType = BaseJsonMarshaler.ParseEnum<EventBonusType>(d, "BonusType");
		o.Regular = BaseJsonMarshaler.ParseValueInt(d, "Regular");
		o.Color = BaseJsonMarshaler.ParseValueString(d, "Color");
		o.BeginYear = BaseJsonMarshaler.ParseValueInt(d, "BeginYear");
		o.BeginMonth = BaseJsonMarshaler.ParseValueInt(d, "BeginMonth");
		o.BeginDay = BaseJsonMarshaler.ParseValueInt(d, "BeginDay");
		o.EndYear = BaseJsonMarshaler.ParseValueInt(d, "EndYear");
		o.EndMonth = BaseJsonMarshaler.ParseValueInt(d, "EndMonth");
		o.EndDay = BaseJsonMarshaler.ParseValueInt(d, "EndDay");
		o.DayOfWeek = BaseJsonMarshaler.ParseValueString(d, "DayOfWeek");
		o.Show = BaseJsonMarshaler.ParseValueInt(d, "Show");
		o.LocalInfo = BaseJsonMarshaler.ParseObject(d, "LocalInfo", JM_NShared_EventCalendarLocalInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(EventCalendarData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventCalendarData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		dictionary.Add("BonusType", o.BonusType.ToString());
		dictionary.Add("Regular", o.Regular.ToString());
		if (o.Color != null)
		{
			dictionary.Add("Color", o.Color);
		}
		dictionary.Add("BeginYear", o.BeginYear.ToString());
		dictionary.Add("BeginMonth", o.BeginMonth.ToString());
		dictionary.Add("BeginDay", o.BeginDay.ToString());
		dictionary.Add("EndYear", o.EndYear.ToString());
		dictionary.Add("EndMonth", o.EndMonth.ToString());
		dictionary.Add("EndDay", o.EndDay.ToString());
		if (o.DayOfWeek != null)
		{
			dictionary.Add("DayOfWeek", o.DayOfWeek);
		}
		dictionary.Add("Show", o.Show.ToString());
		dictionary.Add("LocalInfo", JM_NShared_EventCalendarLocalInfo.EncodeJsonObject(o.LocalInfo));
		return dictionary;
	}
}
