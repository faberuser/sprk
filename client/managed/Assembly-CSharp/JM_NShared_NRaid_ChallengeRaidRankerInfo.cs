using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRaid;

public static class JM_NShared_NRaid_ChallengeRaidRankerInfo
{
	public static ChallengeRaidRankerInfo Parse(string s)
	{
		return Parse(s, new ChallengeRaidRankerInfo());
	}

	public static ChallengeRaidRankerInfo Parse(string s, ChallengeRaidRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChallengeRaidRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new ChallengeRaidRankerInfo());
	}

	public static ChallengeRaidRankerInfo Parse(IDictionary d, ChallengeRaidRankerInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.CurrentScore = BaseJsonMarshaler.ParseValueLong(d, "CurrentScore");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.CurrentBattleTime = BaseJsonMarshaler.ParseValueInt(d, "CurrentBattleTime");
		o.BattleTime = BaseJsonMarshaler.ParseValueInt(d, "BattleTime");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		return o;
	}

	public static string EncodeJsonString(ChallengeRaidRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChallengeRaidRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("CurrentScore", o.CurrentScore.ToString());
		dictionary.Add("Score", o.Score.ToString());
		dictionary.Add("CurrentBattleTime", o.CurrentBattleTime.ToString());
		dictionary.Add("BattleTime", o.BattleTime.ToString());
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		return dictionary;
	}
}
