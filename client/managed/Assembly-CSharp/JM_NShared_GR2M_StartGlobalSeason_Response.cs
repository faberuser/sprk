using System.Collections;
using MiniJSON;
using NShared;
using NShared.GR2M_StartGlobalSeason;

public static class JM_NShared_GR2M_StartGlobalSeason_Response
{
	public static NShared.GR2M_StartGlobalSeason.Response Parse(string s)
	{
		return Parse(s, new NShared.GR2M_StartGlobalSeason.Response());
	}

	public static NShared.GR2M_StartGlobalSeason.Response Parse(string s, NShared.GR2M_StartGlobalSeason.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GR2M_StartGlobalSeason.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.GR2M_StartGlobalSeason.Response());
	}

	public static NShared.GR2M_StartGlobalSeason.Response Parse(IDictionary d, NShared.GR2M_StartGlobalSeason.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		return o;
	}
}
