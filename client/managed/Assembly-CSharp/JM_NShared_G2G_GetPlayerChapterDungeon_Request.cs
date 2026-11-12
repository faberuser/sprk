using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2G_GetPlayerChapterDungeon;

public static class JM_NShared_G2G_GetPlayerChapterDungeon_Request
{
	public static NShared.G2G_GetPlayerChapterDungeon.Request Parse(string s)
	{
		return Parse(s, new NShared.G2G_GetPlayerChapterDungeon.Request());
	}

	public static NShared.G2G_GetPlayerChapterDungeon.Request Parse(string s, NShared.G2G_GetPlayerChapterDungeon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2G_GetPlayerChapterDungeon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2G_GetPlayerChapterDungeon.Request());
	}

	public static NShared.G2G_GetPlayerChapterDungeon.Request Parse(IDictionary d, NShared.G2G_GetPlayerChapterDungeon.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		return o;
	}

	public static string EncodeJsonString(NShared.G2G_GetPlayerChapterDungeon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2G_GetPlayerChapterDungeon.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		return dictionary;
	}
}
