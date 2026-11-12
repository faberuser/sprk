using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetRecordsOfHonorSeasonInfo;

public static class JM_NShared_G2R_GetRecordsOfHonorSeasonInfo_Response
{
	public static NShared.G2R_GetRecordsOfHonorSeasonInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetRecordsOfHonorSeasonInfo.Response());
	}

	public static NShared.G2R_GetRecordsOfHonorSeasonInfo.Response Parse(string s, NShared.G2R_GetRecordsOfHonorSeasonInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetRecordsOfHonorSeasonInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetRecordsOfHonorSeasonInfo.Response());
	}

	public static NShared.G2R_GetRecordsOfHonorSeasonInfo.Response Parse(IDictionary d, NShared.G2R_GetRecordsOfHonorSeasonInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.SeasonInfo = BaseJsonMarshaler.ParseArray(d, "SeasonInfo", JM_NShared_RecordsOfHonorSeasonInfo.Parse);
		return o;
	}
}
