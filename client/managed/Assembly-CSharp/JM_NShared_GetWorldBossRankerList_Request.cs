using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetWorldBossRankerList;
using NShared.NWorldBoss;

public static class JM_NShared_GetWorldBossRankerList_Request
{
	public static NShared.GetWorldBossRankerList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetWorldBossRankerList.Request());
	}

	public static NShared.GetWorldBossRankerList.Request Parse(string s, NShared.GetWorldBossRankerList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetWorldBossRankerList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetWorldBossRankerList.Request());
	}

	public static NShared.GetWorldBossRankerList.Request Parse(IDictionary d, NShared.GetWorldBossRankerList.Request o)
	{
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.PageNo = BaseJsonMarshaler.ParseValueInt(d, "PageNo");
		o.RankingType = BaseJsonMarshaler.ParseEnum<WorldBossRankingType>(d, "RankingType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetWorldBossRankerList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetWorldBossRankerList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WorldBossIndex", o.WorldBossIndex.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("PageNo", o.PageNo.ToString());
		dictionary.Add("RankingType", o.RankingType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
