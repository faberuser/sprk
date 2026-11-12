using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressPlayInfo;

public static class JM_NShared_G2R_GetGuildSuppressPlayInfo_Response
{
	public static NShared.G2R_GetGuildSuppressPlayInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressPlayInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressPlayInfo.Response Parse(string s, NShared.G2R_GetGuildSuppressPlayInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressPlayInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressPlayInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressPlayInfo.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressPlayInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.GuildSuppressPlayInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressPlayInfo", JM_NShared_GuildSuppressPlayInfo.Parse);
		return o;
	}
}
