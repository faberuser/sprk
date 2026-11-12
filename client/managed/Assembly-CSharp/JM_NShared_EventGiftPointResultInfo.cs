using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EventGiftPointResultInfo
{
	public static EventGiftPointResultInfo Parse(string s)
	{
		return Parse(s, new EventGiftPointResultInfo());
	}

	public static EventGiftPointResultInfo Parse(string s, EventGiftPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventGiftPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new EventGiftPointResultInfo());
	}

	public static EventGiftPointResultInfo Parse(IDictionary d, EventGiftPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(EventGiftPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventGiftPointResultInfo o)
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
