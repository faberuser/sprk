using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2R_SetGuildSuppressGroup;

public static class JM_NShared_G2R_SetGuildSuppressGroup_Response
{
	public static NShared.G2R_SetGuildSuppressGroup.Response Parse(string s)
	{
		return Parse(s, new NShared.G2R_SetGuildSuppressGroup.Response());
	}

	public static NShared.G2R_SetGuildSuppressGroup.Response Parse(string s, NShared.G2R_SetGuildSuppressGroup.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_SetGuildSuppressGroup.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_SetGuildSuppressGroup.Response());
	}

	public static NShared.G2R_SetGuildSuppressGroup.Response Parse(IDictionary d, NShared.G2R_SetGuildSuppressGroup.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
