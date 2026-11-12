using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGlobalRankerInfo;

public static class JM_NShared_G2R_GetGlobalRankerInfo_Response
{
	public static NShared.G2R_GetGlobalRankerInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGlobalRankerInfo.Response());
	}

	public static NShared.G2R_GetGlobalRankerInfo.Response Parse(string s, NShared.G2R_GetGlobalRankerInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGlobalRankerInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGlobalRankerInfo.Response());
	}

	public static NShared.G2R_GetGlobalRankerInfo.Response Parse(IDictionary d, NShared.G2R_GetGlobalRankerInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.Rankers = BaseJsonMarshaler.ParseArray(d, "Rankers", JM_NShared_NWorldBoss_WorldBossRankerInfo.Parse);
		return o;
	}
}
