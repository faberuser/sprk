using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerBattleBanPickInfo
{
	public static PlayerBattleBanPickInfo Parse(string s)
	{
		return Parse(s, new PlayerBattleBanPickInfo());
	}

	public static PlayerBattleBanPickInfo Parse(string s, PlayerBattleBanPickInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerBattleBanPickInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerBattleBanPickInfo());
	}

	public static PlayerBattleBanPickInfo Parse(IDictionary d, PlayerBattleBanPickInfo o)
	{
		o.MatchScore = BaseJsonMarshaler.ParseValueInt(d, "MatchScore");
		o.SeasonWin = BaseJsonMarshaler.ParseValueInt(d, "SeasonWin");
		o.SeasonLose = BaseJsonMarshaler.ParseValueInt(d, "SeasonLose");
		o.SuccessiveWin = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveWin");
		o.SuccessiveLose = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveLose");
		o.TierIndex = BaseJsonMarshaler.ParseValueInt(d, "TierIndex");
		o.WorldTierIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldTierIndex");
		o.LastMatchTime = BaseJsonMarshaler.ParseValueString(d, "LastMatchTime");
		return o;
	}

	public static string EncodeJsonString(PlayerBattleBanPickInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerBattleBanPickInfo o)
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
		dictionary.Add("WorldTierIndex", o.WorldTierIndex.ToString());
		if (o.LastMatchTime != null)
		{
			dictionary.Add("LastMatchTime", o.LastMatchTime);
		}
		return dictionary;
	}
}
