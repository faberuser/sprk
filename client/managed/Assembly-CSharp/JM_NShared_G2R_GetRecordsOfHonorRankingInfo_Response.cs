using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetRecordsOfHonorRankingInfo;

public static class JM_NShared_G2R_GetRecordsOfHonorRankingInfo_Response
{
	public static NShared.G2R_GetRecordsOfHonorRankingInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetRecordsOfHonorRankingInfo.Response());
	}

	public static NShared.G2R_GetRecordsOfHonorRankingInfo.Response Parse(string s, NShared.G2R_GetRecordsOfHonorRankingInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetRecordsOfHonorRankingInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetRecordsOfHonorRankingInfo.Response());
	}

	public static NShared.G2R_GetRecordsOfHonorRankingInfo.Response Parse(IDictionary d, NShared.G2R_GetRecordsOfHonorRankingInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.RankingInfo = BaseJsonMarshaler.ParseArray(d, "RankingInfo", JM_NShared_RecordsOfHonorRankingInfo.Parse);
		return o;
	}
}
