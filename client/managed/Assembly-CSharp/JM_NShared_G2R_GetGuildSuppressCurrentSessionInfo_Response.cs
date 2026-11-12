using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressCurrentSessionInfo;

public static class JM_NShared_G2R_GetGuildSuppressCurrentSessionInfo_Response
{
	public static NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response Parse(string s, NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressCurrentSessionInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.GuildSuppressSessionInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressSessionInfo", JM_NShared_GuildSuppressSessionInfo.Parse);
		return o;
	}
}
