using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldBoss;

public static class JM_NShared_NWorldBoss_EventWorldBossScoreInfo
{
	public static EventWorldBossScoreInfo Parse(string s)
	{
		return Parse(s, new EventWorldBossScoreInfo());
	}

	public static EventWorldBossScoreInfo Parse(string s, EventWorldBossScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventWorldBossScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new EventWorldBossScoreInfo());
	}

	public static EventWorldBossScoreInfo Parse(IDictionary d, EventWorldBossScoreInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		return o;
	}

	public static string EncodeJsonString(EventWorldBossScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventWorldBossScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("WorldBossIndex", o.WorldBossIndex.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("Score", o.Score.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		return dictionary;
	}
}
