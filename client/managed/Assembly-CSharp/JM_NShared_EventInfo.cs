using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventInfo
{
	public static EventInfo Parse(string s)
	{
		return Parse(s, new EventInfo());
	}

	public static EventInfo Parse(string s, EventInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventInfo Parse(IDictionary d)
	{
		return Parse(d, new EventInfo());
	}

	public static EventInfo Parse(IDictionary d, EventInfo o)
	{
		o.EventIndex = BaseJsonMarshaler.ParseValueInt(d, "EventIndex");
		o.Value = BaseJsonMarshaler.ParseValueInt(d, "Value");
		return o;
	}

	public static string EncodeJsonString(EventInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventIndex", o.EventIndex.ToString());
		dictionary.Add("Value", o.Value.ToString());
		return dictionary;
	}
}
