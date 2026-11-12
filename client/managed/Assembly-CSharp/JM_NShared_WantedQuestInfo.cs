using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WantedQuestInfo
{
	public static WantedQuestInfo Parse(string s)
	{
		return Parse(s, new WantedQuestInfo());
	}

	public static WantedQuestInfo Parse(string s, WantedQuestInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WantedQuestInfo Parse(IDictionary d)
	{
		return Parse(d, new WantedQuestInfo());
	}

	public static WantedQuestInfo Parse(IDictionary d, WantedQuestInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Cleared = BaseJsonMarshaler.ParseValueSByte(d, "Cleared");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		return o;
	}

	public static string EncodeJsonString(WantedQuestInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WantedQuestInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Cleared", o.Cleared.ToString());
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		return dictionary;
	}
}
