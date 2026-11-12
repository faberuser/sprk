using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRankingBoardRankInfo;

public static class JM_NShared_GetGuildRankingBoardRankInfo_Request
{
	public static NShared.GetGuildRankingBoardRankInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRankingBoardRankInfo.Request());
	}

	public static NShared.GetGuildRankingBoardRankInfo.Request Parse(string s, NShared.GetGuildRankingBoardRankInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRankingBoardRankInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRankingBoardRankInfo.Request());
	}

	public static NShared.GetGuildRankingBoardRankInfo.Request Parse(IDictionary d, NShared.GetGuildRankingBoardRankInfo.Request o)
	{
		o.BoardType = BaseJsonMarshaler.ParseEnum<GuildRankingBoardType>(d, "BoardType");
		o.RankingType = BaseJsonMarshaler.ParseEnum<GuildRankingType>(d, "RankingType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRankingBoardRankInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRankingBoardRankInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BoardType", o.BoardType.ToString());
		dictionary.Add("RankingType", o.RankingType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
