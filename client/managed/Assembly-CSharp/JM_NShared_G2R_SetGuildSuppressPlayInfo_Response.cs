using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_SetGuildSuppressPlayInfo;

public static class JM_NShared_G2R_SetGuildSuppressPlayInfo_Response
{
	public static NShared.G2R_SetGuildSuppressPlayInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_SetGuildSuppressPlayInfo.Response());
	}

	public static NShared.G2R_SetGuildSuppressPlayInfo.Response Parse(string s, NShared.G2R_SetGuildSuppressPlayInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_SetGuildSuppressPlayInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_SetGuildSuppressPlayInfo.Response());
	}

	public static NShared.G2R_SetGuildSuppressPlayInfo.Response Parse(IDictionary d, NShared.G2R_SetGuildSuppressPlayInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
