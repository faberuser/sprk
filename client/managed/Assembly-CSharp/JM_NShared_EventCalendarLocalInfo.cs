using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventCalendarLocalInfo
{
	public static EventCalendarLocalInfo Parse(string s)
	{
		return Parse(s, new EventCalendarLocalInfo());
	}

	public static EventCalendarLocalInfo Parse(string s, EventCalendarLocalInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventCalendarLocalInfo Parse(IDictionary d)
	{
		return Parse(d, new EventCalendarLocalInfo());
	}

	public static EventCalendarLocalInfo Parse(IDictionary d, EventCalendarLocalInfo o)
	{
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.SimpleDescription = BaseJsonMarshaler.ParseValueString(d, "SimpleDescription");
		o.Description = BaseJsonMarshaler.ParseValueString(d, "Description");
		o.Nametag = BaseJsonMarshaler.ParseValueString(d, "Nametag");
		o.NoticeUrl = BaseJsonMarshaler.ParseValueString(d, "NoticeUrl");
		return o;
	}

	public static string EncodeJsonString(EventCalendarLocalInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventCalendarLocalInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.SimpleDescription != null)
		{
			dictionary.Add("SimpleDescription", o.SimpleDescription);
		}
		if (o.Description != null)
		{
			dictionary.Add("Description", o.Description);
		}
		if (o.Nametag != null)
		{
			dictionary.Add("Nametag", o.Nametag);
		}
		if (o.NoticeUrl != null)
		{
			dictionary.Add("NoticeUrl", o.NoticeUrl);
		}
		return dictionary;
	}
}
