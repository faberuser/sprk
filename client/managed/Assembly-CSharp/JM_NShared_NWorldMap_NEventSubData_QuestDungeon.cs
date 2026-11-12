using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_QuestDungeon
{
	public static QuestDungeon Parse(string s)
	{
		return Parse(s, new QuestDungeon());
	}

	public static QuestDungeon Parse(string s, QuestDungeon o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static QuestDungeon Parse(IDictionary d)
	{
		return Parse(d, new QuestDungeon());
	}

	public static QuestDungeon Parse(IDictionary d, QuestDungeon o)
	{
		o.MonsterIndex = BaseJsonMarshaler.ParseValueInt(d, "MonsterIndex");
		return o;
	}

	public static string EncodeJsonString(QuestDungeon o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(QuestDungeon o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MonsterIndex", o.MonsterIndex.ToString());
		return dictionary;
	}
}
