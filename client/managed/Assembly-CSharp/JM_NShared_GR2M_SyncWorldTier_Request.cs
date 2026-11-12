using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GR2M_SyncWorldTier;

public static class JM_NShared_GR2M_SyncWorldTier_Request
{
	public static NShared.GR2M_SyncWorldTier.Request Parse(string s)
	{
		return Parse(s, new NShared.GR2M_SyncWorldTier.Request());
	}

	public static NShared.GR2M_SyncWorldTier.Request Parse(string s, NShared.GR2M_SyncWorldTier.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GR2M_SyncWorldTier.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GR2M_SyncWorldTier.Request());
	}

	public static NShared.GR2M_SyncWorldTier.Request Parse(IDictionary d, NShared.GR2M_SyncWorldTier.Request o)
	{
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.AccountInfos = BaseJsonMarshaler.ParseArray(d, "AccountInfos", JM_NShared_GlobalAccountInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(NShared.GR2M_SyncWorldTier.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GR2M_SyncWorldTier.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("AccountInfos", BaseJsonMarshaler.EncodeArray(o.AccountInfos, JM_NShared_GlobalAccountInfo.EncodeJsonObject));
		return dictionary;
	}
}
