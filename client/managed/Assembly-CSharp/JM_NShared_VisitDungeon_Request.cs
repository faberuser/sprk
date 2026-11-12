using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.VisitDungeon;

public static class JM_NShared_VisitDungeon_Request
{
	public static NShared.VisitDungeon.Request Parse(string s)
	{
		return Parse(s, new NShared.VisitDungeon.Request());
	}

	public static NShared.VisitDungeon.Request Parse(string s, NShared.VisitDungeon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.VisitDungeon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.VisitDungeon.Request());
	}

	public static NShared.VisitDungeon.Request Parse(IDictionary d, NShared.VisitDungeon.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.VisitDungeon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.VisitDungeon.Request o)
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
