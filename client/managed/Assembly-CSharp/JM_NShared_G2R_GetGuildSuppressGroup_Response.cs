using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressGroup;

public static class JM_NShared_G2R_GetGuildSuppressGroup_Response
{
	public static NShared.G2R_GetGuildSuppressGroup.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressGroup.Response());
	}

	public static NShared.G2R_GetGuildSuppressGroup.Response Parse(string s, NShared.G2R_GetGuildSuppressGroup.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressGroup.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressGroup.Response());
	}

	public static NShared.G2R_GetGuildSuppressGroup.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressGroup.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.GuildSuppressGroupInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressGroupInfo", JM_NShared_GuildSuppressInfo.Parse);
		return o;
	}
}
