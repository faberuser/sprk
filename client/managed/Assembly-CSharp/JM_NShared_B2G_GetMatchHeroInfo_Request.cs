using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_GetMatchHeroInfo;

public static class JM_NShared_B2G_GetMatchHeroInfo_Request
{
	public static NShared.B2G_GetMatchHeroInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_GetMatchHeroInfo.Request());
	}

	public static NShared.B2G_GetMatchHeroInfo.Request Parse(string s, NShared.B2G_GetMatchHeroInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_GetMatchHeroInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_GetMatchHeroInfo.Request());
	}

	public static NShared.B2G_GetMatchHeroInfo.Request Parse(IDictionary d, NShared.B2G_GetMatchHeroInfo.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_GetMatchHeroInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_GetMatchHeroInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		return dictionary;
	}
}
