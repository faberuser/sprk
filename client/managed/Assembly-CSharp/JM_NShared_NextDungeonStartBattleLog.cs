using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NextDungeonStartBattleLog
{
	public static NextDungeonStartBattleLog Parse(string s)
	{
		return Parse(s, new NextDungeonStartBattleLog());
	}

	public static NextDungeonStartBattleLog Parse(string s, NextDungeonStartBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NextDungeonStartBattleLog Parse(IDictionary d)
	{
		return Parse(d, new NextDungeonStartBattleLog());
	}

	public static NextDungeonStartBattleLog Parse(IDictionary d, NextDungeonStartBattleLog o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(NextDungeonStartBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NextDungeonStartBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}
