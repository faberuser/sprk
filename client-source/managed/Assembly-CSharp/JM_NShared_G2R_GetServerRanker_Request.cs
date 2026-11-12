using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_GetServerRanker;

public static class JM_NShared_G2R_GetServerRanker_Request
{
	public static NShared.G2R_GetServerRanker.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetServerRanker.Request());
	}

	public static NShared.G2R_GetServerRanker.Request Parse(string s, NShared.G2R_GetServerRanker.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetServerRanker.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetServerRanker.Request());
	}

	public static NShared.G2R_GetServerRanker.Request Parse(IDictionary d, NShared.G2R_GetServerRanker.Request o)
	{
		o.__Header__ = BaseJsonMarshaler.ParseObject(d, "__Header__", JM_NShared_G2R_GetServerRanker_Header.Parse);
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.TierType = BaseJsonMarshaler.ParseEnum<MatchTierType>(d, "TierType");
		o.StartRank = BaseJsonMarshaler.ParseValueInt(d, "StartRank");
		o.EndRank = BaseJsonMarshaler.ParseValueInt(d, "EndRank");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_GetServerRanker.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_GetServerRanker.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("__Header__", JM_NShared_G2R_GetServerRanker_Header.EncodeJsonObject(o.__Header__));
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("TierType", o.TierType.ToString());
		dictionary.Add("StartRank", o.StartRank.ToString());
		dictionary.Add("EndRank", o.EndRank.ToString());
		return dictionary;
	}
}
