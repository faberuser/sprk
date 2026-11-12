using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BossHpPercentage
{
	public static BossHpPercentage Parse(string s)
	{
		return Parse(s, new BossHpPercentage());
	}

	public static BossHpPercentage Parse(string s, BossHpPercentage o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BossHpPercentage Parse(IDictionary d)
	{
		return Parse(d, new BossHpPercentage());
	}

	public static BossHpPercentage Parse(IDictionary d, BossHpPercentage o)
	{
		o.Percentage = BaseJsonMarshaler.ParseValueInt(d, "Percentage");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(BossHpPercentage o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BossHpPercentage o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Percentage", o.Percentage.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
