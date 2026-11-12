using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterBattleLoading
{
	public static EnterBattleLoading Parse(string s)
	{
		return Parse(s, new EnterBattleLoading());
	}

	public static EnterBattleLoading Parse(string s, EnterBattleLoading o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterBattleLoading Parse(IDictionary d)
	{
		return Parse(d, new EnterBattleLoading());
	}

	public static EnterBattleLoading Parse(IDictionary d, EnterBattleLoading o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(EnterBattleLoading o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterBattleLoading o)
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
