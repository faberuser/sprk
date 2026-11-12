using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventDungeonPointResultInfo
{
	public static EventDungeonPointResultInfo Parse(string s)
	{
		return Parse(s, new EventDungeonPointResultInfo());
	}

	public static EventDungeonPointResultInfo Parse(string s, EventDungeonPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventDungeonPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new EventDungeonPointResultInfo());
	}

	public static EventDungeonPointResultInfo Parse(IDictionary d, EventDungeonPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		o.DayValue = BaseJsonMarshaler.ParseValueLong(d, "DayValue");
		return o;
	}

	public static string EncodeJsonString(EventDungeonPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventDungeonPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		dictionary.Add("DayValue", o.DayValue.ToString());
		return dictionary;
	}
}
