using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_UserHeroStatsInfo
{
	public static UserHeroStatsInfo Parse(string s)
	{
		return Parse(s, new UserHeroStatsInfo());
	}

	public static UserHeroStatsInfo Parse(string s, UserHeroStatsInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UserHeroStatsInfo Parse(IDictionary d)
	{
		return Parse(d, new UserHeroStatsInfo());
	}

	public static UserHeroStatsInfo Parse(IDictionary d, UserHeroStatsInfo o)
	{
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.RegTime = BaseJsonMarshaler.ParseValueInt(d, "RegTime");
		o.Heroes = BaseJsonMarshaler.ParseArray(d, "Heroes", JM_NShared_HeroData.Parse);
		return o;
	}

	public static string EncodeJsonString(UserHeroStatsInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UserHeroStatsInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("RegTime", o.RegTime.ToString());
		dictionary.Add("Heroes", BaseJsonMarshaler.EncodeArray(o.Heroes, JM_NShared_HeroData.EncodeJsonObject));
		return dictionary;
	}
}
