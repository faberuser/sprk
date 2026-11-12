using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressBoardAllRankers;

public static class JM_NShared_G2R_GetGuildSuppressBoardAllRankers_Response
{
	public static NShared.G2R_GetGuildSuppressBoardAllRankers.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressBoardAllRankers.Response());
	}

	public static NShared.G2R_GetGuildSuppressBoardAllRankers.Response Parse(string s, NShared.G2R_GetGuildSuppressBoardAllRankers.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressBoardAllRankers.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressBoardAllRankers.Response());
	}

	public static NShared.G2R_GetGuildSuppressBoardAllRankers.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressBoardAllRankers.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.RankerInfos = BaseJsonMarshaler.ParseArray(d, "RankerInfos", JM_NShared_GuildSuppressGroupRankerInfo.Parse);
		return o;
	}
}
