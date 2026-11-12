using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetRecommendDeckList;

public static class JM_NShared_GetRecommendDeckList_Request
{
	public static NShared.GetRecommendDeckList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetRecommendDeckList.Request());
	}

	public static NShared.GetRecommendDeckList.Request Parse(string s, NShared.GetRecommendDeckList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetRecommendDeckList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetRecommendDeckList.Request());
	}

	public static NShared.GetRecommendDeckList.Request Parse(IDictionary d, NShared.GetRecommendDeckList.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetRecommendDeckList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetRecommendDeckList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
