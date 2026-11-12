using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetMultPlayiChallengeRaidRankerList;

public static class JM_NShared_GetMultPlayiChallengeRaidRankerList_Request
{
	public static NShared.GetMultPlayiChallengeRaidRankerList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetMultPlayiChallengeRaidRankerList.Request());
	}

	public static NShared.GetMultPlayiChallengeRaidRankerList.Request Parse(string s, NShared.GetMultPlayiChallengeRaidRankerList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetMultPlayiChallengeRaidRankerList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetMultPlayiChallengeRaidRankerList.Request());
	}

	public static NShared.GetMultPlayiChallengeRaidRankerList.Request Parse(IDictionary d, NShared.GetMultPlayiChallengeRaidRankerList.Request o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetMultPlayiChallengeRaidRankerList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetMultPlayiChallengeRaidRankerList.Request o)
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
