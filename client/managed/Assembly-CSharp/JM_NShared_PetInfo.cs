using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetInfo
{
	public static PetInfo Parse(string s)
	{
		return Parse(s, new PetInfo());
	}

	public static PetInfo Parse(string s, PetInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetInfo Parse(IDictionary d)
	{
		return Parse(d, new PetInfo());
	}

	public static PetInfo Parse(IDictionary d, PetInfo o)
	{
		o.PetIndex = BaseJsonMarshaler.ParseValueInt(d, "PetIndex");
		o.Star = BaseJsonMarshaler.ParseValueSByte(d, "Star");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.HappinessPoint = BaseJsonMarshaler.ParseValueInt(d, "HappinessPoint");
		o.LastPlayTime = BaseJsonMarshaler.ParseValueString(d, "LastPlayTime");
		o.LastPatTime = BaseJsonMarshaler.ParseValueString(d, "LastPatTime");
		o.LastFeedTime = BaseJsonMarshaler.ParseValueString(d, "LastFeedTime");
		o.MaxRewardedTime = BaseJsonMarshaler.ParseValueString(d, "MaxRewardedTime");
		o.FullPoint = BaseJsonMarshaler.ParseValueInt(d, "FullPoint");
		o.Status = BaseJsonMarshaler.ParseEnum<PetStatusType>(d, "Status");
		o.EndPenaltyTime = BaseJsonMarshaler.ParseValueString(d, "EndPenaltyTime");
		return o;
	}

	public static string EncodeJsonString(PetInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PetIndex", o.PetIndex.ToString());
		dictionary.Add("Star", o.Star.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		dictionary.Add("HappinessPoint", o.HappinessPoint.ToString());
		if (o.LastPlayTime != null)
		{
			dictionary.Add("LastPlayTime", o.LastPlayTime);
		}
		if (o.LastPatTime != null)
		{
			dictionary.Add("LastPatTime", o.LastPatTime);
		}
		if (o.LastFeedTime != null)
		{
			dictionary.Add("LastFeedTime", o.LastFeedTime);
		}
		if (o.MaxRewardedTime != null)
		{
			dictionary.Add("MaxRewardedTime", o.MaxRewardedTime);
		}
		dictionary.Add("FullPoint", o.FullPoint.ToString());
		dictionary.Add("Status", o.Status.ToString());
		if (o.EndPenaltyTime != null)
		{
			dictionary.Add("EndPenaltyTime", o.EndPenaltyTime);
		}
		return dictionary;
	}
}
