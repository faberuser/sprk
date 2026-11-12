using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroAddResultInfo
{
	public static HeroAddResultInfo Parse(string s)
	{
		return Parse(s, new HeroAddResultInfo());
	}

	public static HeroAddResultInfo Parse(string s, HeroAddResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroAddResultInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroAddResultInfo());
	}

	public static HeroAddResultInfo Parse(IDictionary d, HeroAddResultInfo o)
	{
		o.HeroInfo = BaseJsonMarshaler.ParseObject(d, "HeroInfo", JM_NShared_HeroInfo.Parse);
		o.TeamExpResult = BaseJsonMarshaler.ParseObject(d, "TeamExpResult", JM_NShared_ExpResultInfo.Parse);
		o.StaminaResult = BaseJsonMarshaler.ParseObject(d, "StaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.HeroFriendlyInfo = BaseJsonMarshaler.ParseObject(d, "HeroFriendlyInfo", JM_NShared_PlayerHeroFriendlyInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(HeroAddResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroAddResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroInfo", JM_NShared_HeroInfo.EncodeJsonObject(o.HeroInfo));
		dictionary.Add("TeamExpResult", JM_NShared_ExpResultInfo.EncodeJsonObject(o.TeamExpResult));
		dictionary.Add("StaminaResult", JM_NShared_StaminaResultInfo.EncodeJsonObject(o.StaminaResult));
		dictionary.Add("HeroFriendlyInfo", JM_NShared_PlayerHeroFriendlyInfo.EncodeJsonObject(o.HeroFriendlyInfo));
		return dictionary;
	}
}
