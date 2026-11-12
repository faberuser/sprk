using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MatchResultInfo
{
	public static MatchResultInfo Parse(string s)
	{
		return Parse(s, new MatchResultInfo());
	}

	public static MatchResultInfo Parse(string s, MatchResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchResultInfo Parse(IDictionary d)
	{
		return Parse(d, new MatchResultInfo());
	}

	public static MatchResultInfo Parse(IDictionary d, MatchResultInfo o)
	{
		o.MatchUid = BaseJsonMarshaler.ParseValueLong(d, "MatchUid");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.Win = BaseJsonMarshaler.ParseValueBool(d, "Win");
		o.GiveUp = BaseJsonMarshaler.ParseValueBool(d, "GiveUp");
		o.Canceled = BaseJsonMarshaler.ParseValueBool(d, "Canceled");
		o.GainedMatchScore = BaseJsonMarshaler.ParseValueInt(d, "GainedMatchScore");
		o.NewMatchScore = BaseJsonMarshaler.ParseValueInt(d, "NewMatchScore");
		o.NewTierIndex = BaseJsonMarshaler.ParseValueInt(d, "NewTierIndex");
		o.NewTierRank = BaseJsonMarshaler.ParseValueInt(d, "NewTierRank");
		o.NewTotalRank = BaseJsonMarshaler.ParseValueInt(d, "NewTotalRank");
		o.PrevTierIndex = BaseJsonMarshaler.ParseValueInt(d, "PrevTierIndex");
		o.PrevTierRank = BaseJsonMarshaler.ParseValueInt(d, "PrevTierRank");
		o.PrevTotalRank = BaseJsonMarshaler.ParseValueInt(d, "PrevTotalRank");
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.HeroExpResults = BaseJsonMarshaler.ParseArray(d, "HeroExpResults", JM_NShared_HeroExpResultInfo.Parse);
		o.SwordResult = BaseJsonMarshaler.ParseObject(d, "SwordResult", JM_NShared_StaminaResultInfo.Parse);
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		return o;
	}

	public static string EncodeJsonString(MatchResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchUid", o.MatchUid.ToString());
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("GiveUp", o.GiveUp.ToString());
		dictionary.Add("Canceled", o.Canceled.ToString());
		dictionary.Add("GainedMatchScore", o.GainedMatchScore.ToString());
		dictionary.Add("NewMatchScore", o.NewMatchScore.ToString());
		dictionary.Add("NewTierIndex", o.NewTierIndex.ToString());
		dictionary.Add("NewTierRank", o.NewTierRank.ToString());
		dictionary.Add("NewTotalRank", o.NewTotalRank.ToString());
		dictionary.Add("PrevTierIndex", o.PrevTierIndex.ToString());
		dictionary.Add("PrevTierRank", o.PrevTierRank.ToString());
		dictionary.Add("PrevTotalRank", o.PrevTotalRank.ToString());
		dictionary.Add("ItemResults", BaseJsonMarshaler.EncodeArray(o.ItemResults, JM_NShared_ItemResultInfo.EncodeJsonObject));
		dictionary.Add("HeroExpResults", BaseJsonMarshaler.EncodeArray(o.HeroExpResults, JM_NShared_HeroExpResultInfo.EncodeJsonObject));
		dictionary.Add("SwordResult", JM_NShared_StaminaResultInfo.EncodeJsonObject(o.SwordResult));
		dictionary.Add("CurrencyResults", BaseJsonMarshaler.EncodeArray(o.CurrencyResults, JM_NShared_CurrencyResultInfo3.EncodeJsonObject));
		return dictionary;
	}
}
