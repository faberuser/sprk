using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetChallengeRaidRankerList;

public static class JM_NShared_GetChallengeRaidRankerList_Request
{
	public static NShared.GetChallengeRaidRankerList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetChallengeRaidRankerList.Request());
	}

	public static NShared.GetChallengeRaidRankerList.Request Parse(string s, NShared.GetChallengeRaidRankerList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetChallengeRaidRankerList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetChallengeRaidRankerList.Request());
	}

	public static NShared.GetChallengeRaidRankerList.Request Parse(IDictionary d, NShared.GetChallengeRaidRankerList.Request o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.PageNo = BaseJsonMarshaler.ParseValueInt(d, "PageNo");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetChallengeRaidRankerList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetChallengeRaidRankerList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("PageNo", o.PageNo.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
