using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRankingBoardServerTotalRankInfo;

public static class JM_NShared_GetGuildRankingBoardServerTotalRankInfo_Request
{
	public static NShared.GetGuildRankingBoardServerTotalRankInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRankingBoardServerTotalRankInfo.Request());
	}

	public static NShared.GetGuildRankingBoardServerTotalRankInfo.Request Parse(string s, NShared.GetGuildRankingBoardServerTotalRankInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRankingBoardServerTotalRankInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRankingBoardServerTotalRankInfo.Request());
	}

	public static NShared.GetGuildRankingBoardServerTotalRankInfo.Request Parse(IDictionary d, NShared.GetGuildRankingBoardServerTotalRankInfo.Request o)
	{
		o.BoardType = BaseJsonMarshaler.ParseEnum<GuildRankingBoardType>(d, "BoardType");
		o.RankingType = BaseJsonMarshaler.ParseEnum<GuildRankingType>(d, "RankingType");
		o.ViewSeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "ViewSeasonIndex");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRankingBoardServerTotalRankInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRankingBoardServerTotalRankInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BoardType", o.BoardType.ToString());
		dictionary.Add("RankingType", o.RankingType.ToString());
		dictionary.Add("ViewSeasonIndex", o.ViewSeasonIndex.ToString());
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
