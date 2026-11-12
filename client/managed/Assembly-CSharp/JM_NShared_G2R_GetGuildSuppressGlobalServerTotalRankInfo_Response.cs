using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo;

public static class JM_NShared_G2R_GetGuildSuppressGlobalServerTotalRankInfo_Response
{
	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response Parse(string s, NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.Info = BaseJsonMarshaler.ParseObject(d, "Info", JM_NShared_GuildRankingBoardServerTotalRankingInfo.Parse);
		return o;
	}
}
