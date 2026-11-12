using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SweepDungeon;

public static class JM_NShared_SweepDungeon_Request
{
	public static NShared.SweepDungeon.Request Parse(string s)
	{
		return Parse(s, new NShared.SweepDungeon.Request());
	}

	public static NShared.SweepDungeon.Request Parse(string s, NShared.SweepDungeon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SweepDungeon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SweepDungeon.Request());
	}

	public static NShared.SweepDungeon.Request Parse(IDictionary d, NShared.SweepDungeon.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.SelectedRewardItemCodes = BaseJsonMarshaler.ParseArrayCast(d, "SelectedRewardItemCodes", BaseJsonMarshaler.CastToString);
		o.SweepCount = BaseJsonMarshaler.ParseValueLong(d, "SweepCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SweepDungeon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SweepDungeon.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		int chapterIndex = o.ChapterIndex;
		dictionary.Add("ChapterIndex", ((long)chapterIndex).ToString());
		int dungeonIndex = o.DungeonIndex;
		dictionary.Add("DungeonIndex", ((long)dungeonIndex).ToString());
		int dungeonDifficulty = o.DungeonDifficulty;
		dictionary.Add("DungeonDifficulty", ((long)dungeonDifficulty).ToString());
		dictionary.Add("SelectedRewardItemCodes", BaseJsonMarshaler.EncodeArray(o.SelectedRewardItemCodes));
		dictionary.Add("SweepCount", o.SweepCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
