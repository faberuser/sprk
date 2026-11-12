using System.Collections;
using MiniJSON;
using NShared;
using NShared.G2M_SyncWaiter;

public static class JM_NShared_G2M_SyncWaiter_Response
{
	public static NShared.G2M_SyncWaiter.Response Parse(string s)
	{
		return Parse(s, new NShared.G2M_SyncWaiter.Response());
	}

	public static NShared.G2M_SyncWaiter.Response Parse(string s, NShared.G2M_SyncWaiter.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2M_SyncWaiter.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2M_SyncWaiter.Response());
	}

	public static NShared.G2M_SyncWaiter.Response Parse(IDictionary d, NShared.G2M_SyncWaiter.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.MatchCompletedInfos = BaseJsonMarshaler.ParseArray(d, "MatchCompletedInfos", JM_NShared_MatchInfo.Parse);
		o.ExpiredAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "ExpiredAccountIds", BaseJsonMarshaler.CastToLong);
		o.AllServerHosts = BaseJsonMarshaler.ParseArrayCast(d, "AllServerHosts", BaseJsonMarshaler.CastToString);
		return o;
	}
}
