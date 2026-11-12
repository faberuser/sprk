using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_GlobalTimeResume
{
	public static GlobalTimeResume Parse(string s)
	{
		return Parse(s, new GlobalTimeResume());
	}

	public static GlobalTimeResume Parse(string s, GlobalTimeResume o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GlobalTimeResume Parse(IDictionary d)
	{
		return Parse(d, new GlobalTimeResume());
	}

	public static GlobalTimeResume Parse(IDictionary d, GlobalTimeResume o)
	{
		o.GlobalEndTime = BaseJsonMarshaler.ParseValueFloat(d, "GlobalEndTime");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		return o;
	}

	public static string EncodeJsonString(GlobalTimeResume o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GlobalTimeResume o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GlobalEndTime", o.GlobalEndTime.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("WaveIndex", o.WaveIndex.ToString());
		return dictionary;
	}
}
