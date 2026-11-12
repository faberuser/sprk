using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressBoardRankerInfo;

public static class JM_NShared_G2R_GetGuildSuppressBoardRankerInfo_Response
{
	public static NShared.G2R_GetGuildSuppressBoardRankerInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressBoardRankerInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressBoardRankerInfo.Response Parse(string s, NShared.G2R_GetGuildSuppressBoardRankerInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressBoardRankerInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressBoardRankerInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressBoardRankerInfo.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressBoardRankerInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.RankerInfo = BaseJsonMarshaler.ParseArray(d, "RankerInfo", JM_NShared_GuildSuppressGroupRankerInfo.Parse);
		return o;
	}
}
