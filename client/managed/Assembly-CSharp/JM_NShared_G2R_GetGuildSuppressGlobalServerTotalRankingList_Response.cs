using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList;

public static class JM_NShared_G2R_GetGuildSuppressGlobalServerTotalRankingList_Response
{
	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response());
	}

	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response Parse(string s, NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response());
	}

	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.Rankers = BaseJsonMarshaler.ParseArray(d, "Rankers", JM_NShared_GuildRankingBoardServerTotalRankingInfo.Parse);
		return o;
	}
}
