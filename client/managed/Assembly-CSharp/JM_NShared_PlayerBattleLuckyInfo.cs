using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerBattleLuckyInfo
{
	public static PlayerBattleLuckyInfo Parse(string s)
	{
		return Parse(s, new PlayerBattleLuckyInfo());
	}

	public static PlayerBattleLuckyInfo Parse(string s, PlayerBattleLuckyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerBattleLuckyInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerBattleLuckyInfo());
	}

	public static PlayerBattleLuckyInfo Parse(IDictionary d, PlayerBattleLuckyInfo o)
	{
		o.MatchScore = BaseJsonMarshaler.ParseValueInt(d, "MatchScore");
		o.SeasonWin = BaseJsonMarshaler.ParseValueInt(d, "SeasonWin");
		o.SeasonLose = BaseJsonMarshaler.ParseValueInt(d, "SeasonLose");
		o.SuccessiveWin = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveWin");
		o.SuccessiveLose = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveLose");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.LastMatchTime = BaseJsonMarshaler.ParseValueString(d, "LastMatchTime");
		o.WorldTierIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldTierIndex");
		return o;
	}

	public static string EncodeJsonString(PlayerBattleLuckyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerBattleLuckyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchScore", o.MatchScore.ToString());
		dictionary.Add("SeasonWin", o.SeasonWin.ToString());
		dictionary.Add("SeasonLose", o.SeasonLose.ToString());
		dictionary.Add("SuccessiveWin", o.SuccessiveWin.ToString());
		dictionary.Add("SuccessiveLose", o.SuccessiveLose.ToString());
		dictionary.Add("TierIndex", o.TierIndex.ToString());
		if (o.LastMatchTime != null)
		{
			dictionary.Add("LastMatchTime", o.LastMatchTime);
		}
		dictionary.Add("WorldTierIndex", o.WorldTierIndex.ToString());
		return dictionary;
	}
}
