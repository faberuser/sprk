using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2G_SetGuildId;

public static class JM_NShared_G2G_SetGuildId_Response
{
	public static NShared.G2G_SetGuildId.Response Parse(string s)
	{
		return Parse(s, new NShared.G2G_SetGuildId.Response());
	}

	public static NShared.G2G_SetGuildId.Response Parse(string s, NShared.G2G_SetGuildId.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2G_SetGuildId.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2G_SetGuildId.Response());
	}

	public static NShared.G2G_SetGuildId.Response Parse(IDictionary d, NShared.G2G_SetGuildId.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
