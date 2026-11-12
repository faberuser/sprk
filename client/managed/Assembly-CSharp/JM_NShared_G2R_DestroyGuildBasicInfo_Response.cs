using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_DestroyGuildBasicInfo;

public static class JM_NShared_G2R_DestroyGuildBasicInfo_Response
{
	public static NShared.G2R_DestroyGuildBasicInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_DestroyGuildBasicInfo.Response());
	}

	public static NShared.G2R_DestroyGuildBasicInfo.Response Parse(string s, NShared.G2R_DestroyGuildBasicInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_DestroyGuildBasicInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_DestroyGuildBasicInfo.Response());
	}

	public static NShared.G2R_DestroyGuildBasicInfo.Response Parse(IDictionary d, NShared.G2R_DestroyGuildBasicInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
