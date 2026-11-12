using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRaid;

public static class JM_NShared_NRaid_ChallengeRaidInfo
{
	public static ChallengeRaidInfo Parse(string s)
	{
		return Parse(s, new ChallengeRaidInfo());
	}

	public static ChallengeRaidInfo Parse(string s, ChallengeRaidInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChallengeRaidInfo Parse(IDictionary d)
	{
		return Parse(d, new ChallengeRaidInfo());
	}

	public static ChallengeRaidInfo Parse(IDictionary d, ChallengeRaidInfo o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.ChallengeRaidIndex = BaseJsonMarshaler.ParseValueInt(d, "ChallengeRaidIndex");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.raidRewardType = BaseJsonMarshaler.ParseEnum<RaidRewardType>(d, "raidRewardType");
		return o;
	}

	public static string EncodeJsonString(ChallengeRaidInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChallengeRaidInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("ChallengeRaidIndex", o.ChallengeRaidIndex.ToString());
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("raidRewardType", o.raidRewardType.ToString());
		return dictionary;
	}
}
