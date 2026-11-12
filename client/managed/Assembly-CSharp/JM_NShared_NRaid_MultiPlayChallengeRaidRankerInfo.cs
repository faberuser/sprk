using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRaid;

public static class JM_NShared_NRaid_MultiPlayChallengeRaidRankerInfo
{
	public static MultiPlayChallengeRaidRankerInfo Parse(string s)
	{
		return Parse(s, new MultiPlayChallengeRaidRankerInfo());
	}

	public static MultiPlayChallengeRaidRankerInfo Parse(string s, MultiPlayChallengeRaidRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MultiPlayChallengeRaidRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new MultiPlayChallengeRaidRankerInfo());
	}

	public static MultiPlayChallengeRaidRankerInfo Parse(IDictionary d, MultiPlayChallengeRaidRankerInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.RankerInfos = BaseJsonMarshaler.ParseArray(d, "RankerInfos", JM_NShared_NRaid_RankerInfo.Parse);
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.BattleTime = BaseJsonMarshaler.ParseValueInt(d, "BattleTime");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(MultiPlayChallengeRaidRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MultiPlayChallengeRaidRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("RankerInfos", BaseJsonMarshaler.EncodeArray(o.RankerInfos, JM_NShared_NRaid_RankerInfo.EncodeJsonObject));
		dictionary.Add("Score", o.Score.ToString());
		dictionary.Add("BattleTime", o.BattleTime.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		return dictionary;
	}
}
