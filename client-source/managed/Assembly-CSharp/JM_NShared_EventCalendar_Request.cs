using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EventCalendar;

public static class JM_NShared_EventCalendar_Request
{
	public static NShared.EventCalendar.Request Parse(string s)
	{
		return Parse(s, new NShared.EventCalendar.Request());
	}

	public static NShared.EventCalendar.Request Parse(string s, NShared.EventCalendar.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EventCalendar.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EventCalendar.Request());
	}

	public static NShared.EventCalendar.Request Parse(IDictionary d, NShared.EventCalendar.Request o)
	{
		o.EventCalendarVersion = BaseJsonMarshaler.ParseValueInt(d, "EventCalendarVersion");
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EventCalendar.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EventCalendar.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventCalendarVersion", o.EventCalendarVersion.ToString());
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
