using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PolledRewardInfo
{
	public static PolledRewardInfo Parse(string s)
	{
		return Parse(s, new PolledRewardInfo());
	}

	public static PolledRewardInfo Parse(string s, PolledRewardInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PolledRewardInfo Parse(IDictionary d)
	{
		return Parse(d, new PolledRewardInfo());
	}

	public static PolledRewardInfo Parse(IDictionary d, PolledRewardInfo o)
	{
		o.Gold = BaseJsonMarshaler.ParseValueLong(d, "Gold");
		o.Gem = BaseJsonMarshaler.ParseValueLong(d, "Gem");
		o.PvpCoin = BaseJsonMarshaler.ParseValueLong(d, "PvpCoin");
		o.GuildPoint = BaseJsonMarshaler.ParseValueLong(d, "GuildPoint");
		o.Items = BaseJsonMarshaler.ParseArray(d, "Items", JM_NShared_RewardItemInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PolledRewardInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PolledRewardInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Gold", o.Gold.ToString());
		dictionary.Add("Gem", o.Gem.ToString());
		dictionary.Add("PvpCoin", o.PvpCoin.ToString());
		dictionary.Add("GuildPoint", o.GuildPoint.ToString());
		dictionary.Add("Items", BaseJsonMarshaler.EncodeArray(o.Items, JM_NShared_RewardItemInfo.EncodeJsonObject));
		return dictionary;
	}
}
