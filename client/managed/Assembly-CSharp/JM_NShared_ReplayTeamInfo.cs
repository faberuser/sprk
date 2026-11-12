using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ReplayTeamInfo
{
	public static ReplayTeamInfo Parse(string s)
	{
		return Parse(s, new ReplayTeamInfo());
	}

	public static ReplayTeamInfo Parse(string s, ReplayTeamInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReplayTeamInfo Parse(IDictionary d)
	{
		return Parse(d, new ReplayTeamInfo());
	}

	public static ReplayTeamInfo Parse(IDictionary d, ReplayTeamInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.NickName = BaseJsonMarshaler.ParseValueString(d, "NickName");
		o.AvaterIndex = BaseJsonMarshaler.ParseValueInt(d, "AvaterIndex");
		o.MatchScore = BaseJsonMarshaler.ParseValueInt(d, "MatchScore");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.TierRank = BaseJsonMarshaler.ParseValueInt(d, "TierRank");
		o.TeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "TeamType");
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_ReplayTeamHeroInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(ReplayTeamInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReplayTeamInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("Level", o.Level.ToString());
		if (o.NickName != null)
		{
			dictionary.Add("NickName", o.NickName);
		}
		dictionary.Add("AvaterIndex", o.AvaterIndex.ToString());
		dictionary.Add("MatchScore", o.MatchScore.ToString());
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("TierRank", o.TierRank.ToString());
		dictionary.Add("TeamType", o.TeamType.ToString());
		dictionary.Add("HeroInfos", BaseJsonMarshaler.EncodeArray(o.HeroInfos, JM_NShared_ReplayTeamHeroInfo.EncodeJsonObject));
		return dictionary;
	}
}
