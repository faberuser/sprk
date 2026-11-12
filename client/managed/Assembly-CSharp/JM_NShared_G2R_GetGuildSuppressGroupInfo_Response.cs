using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressGroupInfo;

public static class JM_NShared_G2R_GetGuildSuppressGroupInfo_Response
{
	public static NShared.G2R_GetGuildSuppressGroupInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressGroupInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressGroupInfo.Response Parse(string s, NShared.G2R_GetGuildSuppressGroupInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressGroupInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressGroupInfo.Response());
	}

	public static NShared.G2R_GetGuildSuppressGroupInfo.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressGroupInfo.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.GuildSuppressGroupInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressGroupInfo", JM_NShared_GuildSuppressInfo.Parse);
		o.GuildSuppressGroupRank = BaseJsonMarshaler.ParseValueInt(d, "GuildSuppressGroupRank");
		return o;
	}
}
