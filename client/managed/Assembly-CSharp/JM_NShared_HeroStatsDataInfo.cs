using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroStatsDataInfo
{
	public static HeroStatsDataInfo Parse(string s)
	{
		return Parse(s, new HeroStatsDataInfo());
	}

	public static HeroStatsDataInfo Parse(string s, HeroStatsDataInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroStatsDataInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroStatsDataInfo());
	}

	public static HeroStatsDataInfo Parse(IDictionary d, HeroStatsDataInfo o)
	{
		o.UserHeroStatsInfos = BaseJsonMarshaler.ParseArray(d, "UserHeroStatsInfos", JM_NShared_UserHeroStatsInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(HeroStatsDataInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroStatsDataInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("UserHeroStatsInfos", BaseJsonMarshaler.EncodeArray(o.UserHeroStatsInfos, JM_NShared_UserHeroStatsInfo.EncodeJsonObject));
		return dictionary;
	}
}
