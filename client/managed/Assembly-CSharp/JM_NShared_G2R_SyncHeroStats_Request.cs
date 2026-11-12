using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_SyncHeroStats;

public static class JM_NShared_G2R_SyncHeroStats_Request
{
	public static NShared.G2R_SyncHeroStats.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_SyncHeroStats.Request());
	}

	public static NShared.G2R_SyncHeroStats.Request Parse(string s, NShared.G2R_SyncHeroStats.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_SyncHeroStats.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_SyncHeroStats.Request());
	}

	public static NShared.G2R_SyncHeroStats.Request Parse(IDictionary d, NShared.G2R_SyncHeroStats.Request o)
	{
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.HeroStatsDataInfos = BaseJsonMarshaler.ParseArray(d, "HeroStatsDataInfos", JM_NShared_HeroStatsDataInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_SyncHeroStats.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_SyncHeroStats.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Host != null)
		{
			dictionary.Add("Host", o.Host);
		}
		dictionary.Add("HeroStatsDataInfos", BaseJsonMarshaler.EncodeArray(o.HeroStatsDataInfos, JM_NShared_HeroStatsDataInfo.EncodeJsonObject));
		return dictionary;
	}
}
