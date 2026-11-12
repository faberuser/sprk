using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetChallengeRaidRankInfo;

public static class JM_NShared_GetChallengeRaidRankInfo_Request
{
	public static NShared.GetChallengeRaidRankInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetChallengeRaidRankInfo.Request());
	}

	public static NShared.GetChallengeRaidRankInfo.Request Parse(string s, NShared.GetChallengeRaidRankInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetChallengeRaidRankInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetChallengeRaidRankInfo.Request());
	}

	public static NShared.GetChallengeRaidRankInfo.Request Parse(IDictionary d, NShared.GetChallengeRaidRankInfo.Request o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetChallengeRaidRankInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetChallengeRaidRankInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
