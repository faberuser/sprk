using System.Collections;
using MiniJSON;
using NShared;
using NShared.GR2M_UpdateWeeklyReward;

public static class JM_NShared_GR2M_UpdateWeeklyReward_Response
{
	public static NShared.GR2M_UpdateWeeklyReward.Response Parse(string s)
	{
		return Parse(s, new NShared.GR2M_UpdateWeeklyReward.Response());
	}

	public static NShared.GR2M_UpdateWeeklyReward.Response Parse(string s, NShared.GR2M_UpdateWeeklyReward.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GR2M_UpdateWeeklyReward.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.GR2M_UpdateWeeklyReward.Response());
	}

	public static NShared.GR2M_UpdateWeeklyReward.Response Parse(IDictionary d, NShared.GR2M_UpdateWeeklyReward.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		return o;
	}
}
