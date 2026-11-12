using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_UpdateMatchScore;

public static class JM_NShared_G2R_UpdateMatchScore_Request
{
	public static NShared.G2R_UpdateMatchScore.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_UpdateMatchScore.Request());
	}

	public static NShared.G2R_UpdateMatchScore.Request Parse(string s, NShared.G2R_UpdateMatchScore.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_UpdateMatchScore.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_UpdateMatchScore.Request());
	}

	public static NShared.G2R_UpdateMatchScore.Request Parse(IDictionary d, NShared.G2R_UpdateMatchScore.Request o)
	{
		o.__Header__ = BaseJsonMarshaler.ParseObject(d, "__Header__", JM_NShared_G2R_UpdateMatchScore_Header.Parse);
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.AccountId = BaseJsonMarshaler.ParseValueString(d, "AccountId");
		o.SeasonWin = BaseJsonMarshaler.ParseValueInt(d, "SeasonWin");
		o.SeasonLose = BaseJsonMarshaler.ParseValueInt(d, "SeasonLose");
		o.SuccessiveWin = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveWin");
		o.SuccessiveLose = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveLose");
		o.MatchScore = BaseJsonMarshaler.ParseValueInt(d, "MatchScore");
		o.WorldTierIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldTierIndex");
		o.LastMatchTime = BaseJsonMarshaler.ParseValueString(d, "LastMatchTime");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_UpdateMatchScore.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_UpdateMatchScore.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("__Header__", JM_NShared_G2R_UpdateMatchScore_Header.EncodeJsonObject(o.__Header__));
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		if (o.AccountId != null)
		{
			dictionary.Add("AccountId", o.AccountId);
		}
		dictionary.Add("SeasonWin", o.SeasonWin.ToString());
		dictionary.Add("SeasonLose", o.SeasonLose.ToString());
		dictionary.Add("SuccessiveWin", o.SuccessiveWin.ToString());
		dictionary.Add("SuccessiveLose", o.SuccessiveLose.ToString());
		dictionary.Add("MatchScore", o.MatchScore.ToString());
		dictionary.Add("WorldTierIndex", o.WorldTierIndex.ToString());
		if (o.LastMatchTime != null)
		{
			dictionary.Add("LastMatchTime", o.LastMatchTime);
		}
		return dictionary;
	}
}
