using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressPlayInfos;

public static class JM_NShared_G2R_GetGuildSuppressPlayInfos_Response
{
	public static NShared.G2R_GetGuildSuppressPlayInfos.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressPlayInfos.Response());
	}

	public static NShared.G2R_GetGuildSuppressPlayInfos.Response Parse(string s, NShared.G2R_GetGuildSuppressPlayInfos.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressPlayInfos.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressPlayInfos.Response());
	}

	public static NShared.G2R_GetGuildSuppressPlayInfos.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressPlayInfos.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.GuildSuppressPlayInfos = BaseJsonMarshaler.ParseArray(d, "GuildSuppressPlayInfos", JM_NShared_GuildSuppressPlayInfo.Parse);
		return o;
	}
}
