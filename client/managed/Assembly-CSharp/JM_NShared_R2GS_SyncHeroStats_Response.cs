using System.Collections;
using MiniJSON;
using NShared;
using NShared.R2GS_SyncHeroStats;

public static class JM_NShared_R2GS_SyncHeroStats_Response
{
	public static NShared.R2GS_SyncHeroStats.Response Parse(string s)
	{
		return Parse(s, new NShared.R2GS_SyncHeroStats.Response());
	}

	public static NShared.R2GS_SyncHeroStats.Response Parse(string s, NShared.R2GS_SyncHeroStats.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.R2GS_SyncHeroStats.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.R2GS_SyncHeroStats.Response());
	}

	public static NShared.R2GS_SyncHeroStats.Response Parse(IDictionary d, NShared.R2GS_SyncHeroStats.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		return o;
	}
}
