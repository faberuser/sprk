using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetRecordsOfHonorTopInfo;

public static class JM_NShared_G2R_GetRecordsOfHonorTopInfo_Response
{
	public static NShared.G2R_GetRecordsOfHonorTopInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetRecordsOfHonorTopInfo.Response());
	}

	public static NShared.G2R_GetRecordsOfHonorTopInfo.Response Parse(string s, NShared.G2R_GetRecordsOfHonorTopInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetRecordsOfHonorTopInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetRecordsOfHonorTopInfo.Response());
	}

	public static NShared.G2R_GetRecordsOfHonorTopInfo.Response Parse(IDictionary d, NShared.G2R_GetRecordsOfHonorTopInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.TopInfo = BaseJsonMarshaler.ParseArray(d, "TopInfo", JM_NShared_RecordsOfHonorTopInfo.Parse);
		return o;
	}
}
