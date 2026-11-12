using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGlobalWorldBossRankers;

public static class JM_NShared_G2R_GetGlobalWorldBossRankers_Response
{
	public static NShared.G2R_GetGlobalWorldBossRankers.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGlobalWorldBossRankers.Response());
	}

	public static NShared.G2R_GetGlobalWorldBossRankers.Response Parse(string s, NShared.G2R_GetGlobalWorldBossRankers.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGlobalWorldBossRankers.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGlobalWorldBossRankers.Response());
	}

	public static NShared.G2R_GetGlobalWorldBossRankers.Response Parse(IDictionary d, NShared.G2R_GetGlobalWorldBossRankers.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.Rankers = BaseJsonMarshaler.ParseArray(d, "Rankers", JM_NShared_NWorldBoss_WorldBossRankerInfo.Parse);
		return o;
	}
}
