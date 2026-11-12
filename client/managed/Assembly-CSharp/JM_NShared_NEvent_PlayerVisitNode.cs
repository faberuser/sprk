using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_PlayerVisitNode
{
	public static PlayerVisitNode Parse(string s)
	{
		return Parse(s, new PlayerVisitNode());
	}

	public static PlayerVisitNode Parse(string s, PlayerVisitNode o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerVisitNode Parse(IDictionary d)
	{
		return Parse(d, new PlayerVisitNode());
	}

	public static PlayerVisitNode Parse(IDictionary d, PlayerVisitNode o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(PlayerVisitNode o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerVisitNode o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
