using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetMatchRank;

public static class JM_NShared_GetMatchRank_Request
{
	public static NShared.GetMatchRank.Request Parse(string s)
	{
		return Parse(s, new NShared.GetMatchRank.Request());
	}

	public static NShared.GetMatchRank.Request Parse(string s, NShared.GetMatchRank.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetMatchRank.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetMatchRank.Request());
	}

	public static NShared.GetMatchRank.Request Parse(IDictionary d, NShared.GetMatchRank.Request o)
	{
		o.MatchType = BaseJsonMarshaler.ParseValueLong(d, "MatchType");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.TierType = BaseJsonMarshaler.ParseEnum<MatchTierType>(d, "TierType");
		o.RankerAccountId = BaseJsonMarshaler.ParseValueLong(d, "RankerAccountId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetMatchRank.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetMatchRank.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchType", o.MatchType.ToString());
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("TierType", o.TierType.ToString());
		dictionary.Add("RankerAccountId", o.RankerAccountId.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
