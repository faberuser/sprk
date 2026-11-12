using System.Collections;
using MiniJSON;
using NShared;
using NShared.GR2M_SyncWorldTier;

public static class JM_NShared_GR2M_SyncWorldTier_Response
{
	public static NShared.GR2M_SyncWorldTier.Response Parse(string s)
	{
		return Parse(s, new NShared.GR2M_SyncWorldTier.Response());
	}

	public static NShared.GR2M_SyncWorldTier.Response Parse(string s, NShared.GR2M_SyncWorldTier.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GR2M_SyncWorldTier.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.GR2M_SyncWorldTier.Response());
	}

	public static NShared.GR2M_SyncWorldTier.Response Parse(IDictionary d, NShared.GR2M_SyncWorldTier.Response o)
	{
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.AffectedAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "AffectedAccountIds", BaseJsonMarshaler.CastToString);
		return o;
	}
}
