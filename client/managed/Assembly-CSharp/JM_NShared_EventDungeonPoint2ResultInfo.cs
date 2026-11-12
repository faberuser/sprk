using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventDungeonPoint2ResultInfo
{
	public static EventDungeonPoint2ResultInfo Parse(string s)
	{
		return Parse(s, new EventDungeonPoint2ResultInfo());
	}

	public static EventDungeonPoint2ResultInfo Parse(string s, EventDungeonPoint2ResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventDungeonPoint2ResultInfo Parse(IDictionary d)
	{
		return Parse(d, new EventDungeonPoint2ResultInfo());
	}

	public static EventDungeonPoint2ResultInfo Parse(IDictionary d, EventDungeonPoint2ResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(EventDungeonPoint2ResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventDungeonPoint2ResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
