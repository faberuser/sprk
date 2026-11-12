using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetServerGroupRanker;

public static class JM_NShared_G2R_GetServerGroupRanker_Response
{
	public static NShared.G2R_GetServerGroupRanker.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetServerGroupRanker.Response());
	}

	public static NShared.G2R_GetServerGroupRanker.Response Parse(string s, NShared.G2R_GetServerGroupRanker.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetServerGroupRanker.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetServerGroupRanker.Response());
	}

	public static NShared.G2R_GetServerGroupRanker.Response Parse(IDictionary d, NShared.G2R_GetServerGroupRanker.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.Rankers = BaseJsonMarshaler.ParseArray(d, "Rankers", JM_NShared_ServerGroupRankerInfo.Parse);
		return o;
	}
}
