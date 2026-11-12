using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRaidGuildRanking;

public static class JM_NShared_GetGuildRaidGuildRanking_Request
{
	public static NShared.GetGuildRaidGuildRanking.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRaidGuildRanking.Request());
	}

	public static NShared.GetGuildRaidGuildRanking.Request Parse(string s, NShared.GetGuildRaidGuildRanking.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRaidGuildRanking.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRaidGuildRanking.Request());
	}

	public static NShared.GetGuildRaidGuildRanking.Request Parse(IDictionary d, NShared.GetGuildRaidGuildRanking.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRaidGuildRanking.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRaidGuildRanking.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
