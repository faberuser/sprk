using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetMatchRanker;

public static class JM_NShared_GetMatchRanker_Request
{
	public static NShared.GetMatchRanker.Request Parse(string s)
	{
		return Parse(s, new NShared.GetMatchRanker.Request());
	}

	public static NShared.GetMatchRanker.Request Parse(string s, NShared.GetMatchRanker.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetMatchRanker.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetMatchRanker.Request());
	}

	public static NShared.GetMatchRanker.Request Parse(IDictionary d, NShared.GetMatchRanker.Request o)
	{
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.TierType = BaseJsonMarshaler.ParseEnum<MatchTierType>(d, "TierType");
		o.StartRank = BaseJsonMarshaler.ParseValueInt(d, "StartRank");
		o.EndRank = BaseJsonMarshaler.ParseValueInt(d, "EndRank");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetMatchRanker.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetMatchRanker.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("TierType", o.TierType.ToString());
		dictionary.Add("StartRank", o.StartRank.ToString());
		dictionary.Add("EndRank", o.EndRank.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
