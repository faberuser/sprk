using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetWorldRanker;

public static class JM_NShared_GetWorldRanker_Request
{
	public static NShared.GetWorldRanker.Request Parse(string s)
	{
		return Parse(s, new NShared.GetWorldRanker.Request());
	}

	public static NShared.GetWorldRanker.Request Parse(string s, NShared.GetWorldRanker.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetWorldRanker.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetWorldRanker.Request());
	}

	public static NShared.GetWorldRanker.Request Parse(IDictionary d, NShared.GetWorldRanker.Request o)
	{
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.TierType = BaseJsonMarshaler.ParseEnum<MatchTierType>(d, "TierType");
		o.StartRank = BaseJsonMarshaler.ParseValueInt(d, "StartRank");
		o.EndRank = BaseJsonMarshaler.ParseValueInt(d, "EndRank");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetWorldRanker.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetWorldRanker.Request o)
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
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
