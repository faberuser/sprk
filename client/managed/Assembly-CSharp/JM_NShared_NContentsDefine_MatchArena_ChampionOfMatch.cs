using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NContentsDefine;

public static class JM_NShared_NContentsDefine_MatchArena_ChampionOfMatch
{
	public static MatchArena.ChampionOfMatch Parse(string s)
	{
		return Parse(s, new MatchArena.ChampionOfMatch());
	}

	public static MatchArena.ChampionOfMatch Parse(string s, MatchArena.ChampionOfMatch o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchArena.ChampionOfMatch Parse(IDictionary d)
	{
		return Parse(d, new MatchArena.ChampionOfMatch());
	}

	public static MatchArena.ChampionOfMatch Parse(IDictionary d, MatchArena.ChampionOfMatch o)
	{
		o.PvpType = BaseJsonMarshaler.ParseEnum<MatchPvpType>(d, "PvpType");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.MinRank = BaseJsonMarshaler.ParseValueInt(d, "MinRank");
		return o;
	}

	public static string EncodeJsonString(MatchArena.ChampionOfMatch o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchArena.ChampionOfMatch o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PvpType", o.PvpType.ToString());
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		dictionary.Add("MinRank", o.MinRank.ToString());
		return dictionary;
	}
}
