using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_GetGuildSuppressAutoApply;

public static class JM_NShared_G2R_GetGuildSuppressAutoApply_Response
{
	public static NShared.G2R_GetGuildSuppressAutoApply.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetGuildSuppressAutoApply.Response());
	}

	public static NShared.G2R_GetGuildSuppressAutoApply.Response Parse(string s, NShared.G2R_GetGuildSuppressAutoApply.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetGuildSuppressAutoApply.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetGuildSuppressAutoApply.Response());
	}

	public static NShared.G2R_GetGuildSuppressAutoApply.Response Parse(IDictionary d, NShared.G2R_GetGuildSuppressAutoApply.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.GuildSuppressAutoApplyInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressAutoApplyInfo", JM_NShared_GuildSuppressAutoApplyInfo.Parse);
		return o;
	}
}
