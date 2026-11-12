using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_GlobalTimePause
{
	public static GlobalTimePause Parse(string s)
	{
		return Parse(s, new GlobalTimePause());
	}

	public static GlobalTimePause Parse(string s, GlobalTimePause o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GlobalTimePause Parse(IDictionary d)
	{
		return Parse(d, new GlobalTimePause());
	}

	public static GlobalTimePause Parse(IDictionary d, GlobalTimePause o)
	{
		o.BattleStartTime = BaseJsonMarshaler.ParseValueFloat(d, "BattleStartTime");
		o.GolbalStartTime = BaseJsonMarshaler.ParseValueFloat(d, "GolbalStartTime");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		return o;
	}

	public static string EncodeJsonString(GlobalTimePause o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GlobalTimePause o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleStartTime", o.BattleStartTime.ToString());
		dictionary.Add("GolbalStartTime", o.GolbalStartTime.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("WaveIndex", o.WaveIndex.ToString());
		return dictionary;
	}
}
