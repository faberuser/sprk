using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CompleteScenarioDungeon;

public static class JM_NShared_CompleteScenarioDungeon_Request
{
	public static NShared.CompleteScenarioDungeon.Request Parse(string s)
	{
		return Parse(s, new NShared.CompleteScenarioDungeon.Request());
	}

	public static NShared.CompleteScenarioDungeon.Request Parse(string s, NShared.CompleteScenarioDungeon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CompleteScenarioDungeon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CompleteScenarioDungeon.Request());
	}

	public static NShared.CompleteScenarioDungeon.Request Parse(IDictionary d, NShared.CompleteScenarioDungeon.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CompleteScenarioDungeon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CompleteScenarioDungeon.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
