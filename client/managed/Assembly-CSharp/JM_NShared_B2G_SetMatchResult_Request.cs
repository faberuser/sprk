using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetMatchResult;

public static class JM_NShared_B2G_SetMatchResult_Request
{
	public static NShared.B2G_SetMatchResult.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetMatchResult.Request());
	}

	public static NShared.B2G_SetMatchResult.Request Parse(string s, NShared.B2G_SetMatchResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetMatchResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetMatchResult.Request());
	}

	public static NShared.B2G_SetMatchResult.Request Parse(IDictionary d, NShared.B2G_SetMatchResult.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.MatchUid = BaseJsonMarshaler.ParseValueLong(d, "MatchUid");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.AvgFightingPower = BaseJsonMarshaler.ParseValueInt(d, "AvgFightingPower");
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.EnterHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "EnterHeroIndices", BaseJsonMarshaler.CastToInt);
		o.BanHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "BanHeroIndices", BaseJsonMarshaler.CastToInt);
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.IsLogon = BaseJsonMarshaler.ParseValueBool(d, "IsLogon");
		o.IsGiveUp = BaseJsonMarshaler.ParseValueBool(d, "IsGiveUp");
		o.PenaltyCount = BaseJsonMarshaler.ParseValueInt(d, "PenaltyCount");
		o.MatchBanPickState = BaseJsonMarshaler.ParseValueInt(d, "MatchBanPickState");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetMatchResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetMatchResult.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("MatchUid", o.MatchUid.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("AvgFightingPower", o.AvgFightingPower.ToString());
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("AliveHeroIndices", BaseJsonMarshaler.EncodeArray(o.AliveHeroIndices));
		dictionary.Add("EnterHeroIndices", BaseJsonMarshaler.EncodeArray(o.EnterHeroIndices));
		dictionary.Add("BanHeroIndices", BaseJsonMarshaler.EncodeArray(o.BanHeroIndices));
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("IsLogon", o.IsLogon.ToString());
		dictionary.Add("IsGiveUp", o.IsGiveUp.ToString());
		dictionary.Add("PenaltyCount", o.PenaltyCount.ToString());
		dictionary.Add("MatchBanPickState", o.MatchBanPickState.ToString());
		return dictionary;
	}
}
