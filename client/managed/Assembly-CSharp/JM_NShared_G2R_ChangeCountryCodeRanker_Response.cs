using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_ChangeCountryCodeRanker;

public static class JM_NShared_G2R_ChangeCountryCodeRanker_Response
{
	public static NShared.G2R_ChangeCountryCodeRanker.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_ChangeCountryCodeRanker.Response());
	}

	public static NShared.G2R_ChangeCountryCodeRanker.Response Parse(string s, NShared.G2R_ChangeCountryCodeRanker.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_ChangeCountryCodeRanker.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_ChangeCountryCodeRanker.Response());
	}

	public static NShared.G2R_ChangeCountryCodeRanker.Response Parse(IDictionary d, NShared.G2R_ChangeCountryCodeRanker.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
