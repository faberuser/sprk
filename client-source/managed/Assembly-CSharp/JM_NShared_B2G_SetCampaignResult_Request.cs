using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_SetCampaignResult;

public static class JM_NShared_B2G_SetCampaignResult_Request
{
	public static NShared.B2G_SetCampaignResult.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_SetCampaignResult.Request());
	}

	public static NShared.B2G_SetCampaignResult.Request Parse(string s, NShared.B2G_SetCampaignResult.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_SetCampaignResult.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_SetCampaignResult.Request());
	}

	public static NShared.B2G_SetCampaignResult.Request Parse(IDictionary d, NShared.B2G_SetCampaignResult.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.FinishedAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "FinishedAccountIds", BaseJsonMarshaler.CastToLong);
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.ElapsedPlayTime = BaseJsonMarshaler.ParseValueInt(d, "ElapsedPlayTime");
		o.Win = BaseJsonMarshaler.ParseValueBool(d, "Win");
		o.IsMaster = BaseJsonMarshaler.ParseValueBool(d, "IsMaster");
		o.AvgHeroLevel = BaseJsonMarshaler.ParseValueInt(d, "AvgHeroLevel");
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_SetCampaignResult.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_SetCampaignResult.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("FinishedAccountIds", BaseJsonMarshaler.EncodeArray(o.FinishedAccountIds));
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("ElapsedPlayTime", o.ElapsedPlayTime.ToString());
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("IsMaster", o.IsMaster.ToString());
		dictionary.Add("AvgHeroLevel", o.AvgHeroLevel.ToString());
		dictionary.Add("AliveHeroIndices", BaseJsonMarshaler.EncodeArray(o.AliveHeroIndices));
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		return dictionary;
	}
}
