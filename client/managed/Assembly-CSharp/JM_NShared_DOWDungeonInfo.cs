using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DOWDungeonInfo
{
	public static DOWDungeonInfo Parse(string s)
	{
		return Parse(s, new DOWDungeonInfo());
	}

	public static DOWDungeonInfo Parse(string s, DOWDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DOWDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new DOWDungeonInfo());
	}

	public static DOWDungeonInfo Parse(IDictionary d, DOWDungeonInfo o)
	{
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Diff = BaseJsonMarshaler.ParseValueInt(d, "Diff");
		o.StartTime = BaseJsonMarshaler.ParseValueString(d, "StartTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		return o;
	}

	public static string EncodeJsonString(DOWDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DOWDungeonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Diff", o.Diff.ToString());
		if (o.StartTime != null)
		{
			dictionary.Add("StartTime", o.StartTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		return dictionary;
	}
}
