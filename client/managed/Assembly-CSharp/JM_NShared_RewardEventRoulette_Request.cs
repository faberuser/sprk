using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RewardEventRoulette;

public static class JM_NShared_RewardEventRoulette_Request
{
	public static NShared.RewardEventRoulette.Request Parse(string s)
	{
		return Parse(s, new NShared.RewardEventRoulette.Request());
	}

	public static NShared.RewardEventRoulette.Request Parse(string s, NShared.RewardEventRoulette.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RewardEventRoulette.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RewardEventRoulette.Request());
	}

	public static NShared.RewardEventRoulette.Request Parse(IDictionary d, NShared.RewardEventRoulette.Request o)
	{
		o.RouletteIndex = BaseJsonMarshaler.ParseValueInt(d, "RouletteIndex");
		o.RouletteCount = BaseJsonMarshaler.ParseValueInt(d, "RouletteCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RewardEventRoulette.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RewardEventRoulette.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RouletteIndex", o.RouletteIndex.ToString());
		dictionary.Add("RouletteCount", o.RouletteCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
