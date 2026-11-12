using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_AddRanker;

public static class JM_NShared_G2R_AddRanker_Response
{
	public static NShared.G2R_AddRanker.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_AddRanker.Response());
	}

	public static NShared.G2R_AddRanker.Response Parse(string s, NShared.G2R_AddRanker.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_AddRanker.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_AddRanker.Response());
	}

	public static NShared.G2R_AddRanker.Response Parse(IDictionary d, NShared.G2R_AddRanker.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
