using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GiveRewardHeroInnRoulette;

public static class JM_NShared_GiveRewardHeroInnRoulette_Request
{
	public static NShared.GiveRewardHeroInnRoulette.Request Parse(string s)
	{
		return Parse(s, new NShared.GiveRewardHeroInnRoulette.Request());
	}

	public static NShared.GiveRewardHeroInnRoulette.Request Parse(string s, NShared.GiveRewardHeroInnRoulette.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GiveRewardHeroInnRoulette.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GiveRewardHeroInnRoulette.Request());
	}

	public static NShared.GiveRewardHeroInnRoulette.Request Parse(IDictionary d, NShared.GiveRewardHeroInnRoulette.Request o)
	{
		o.RouletteIndex = BaseJsonMarshaler.ParseValueInt(d, "RouletteIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GiveRewardHeroInnRoulette.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GiveRewardHeroInnRoulette.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RouletteIndex", o.RouletteIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
