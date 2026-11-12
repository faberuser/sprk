using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_GetHeroInfo;

public static class JM_NShared_B2G_GetHeroInfo_Request
{
	public static NShared.B2G_GetHeroInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_GetHeroInfo.Request());
	}

	public static NShared.B2G_GetHeroInfo.Request Parse(string s, NShared.B2G_GetHeroInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_GetHeroInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_GetHeroInfo.Request());
	}

	public static NShared.B2G_GetHeroInfo.Request Parse(IDictionary d, NShared.B2G_GetHeroInfo.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.AiHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AiHeroIndices", BaseJsonMarshaler.CastToInt);
		o.GroupHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "GroupHeroIndices", BaseJsonMarshaler.CastToInt);
		o.IsMaster = BaseJsonMarshaler.ParseValueBool(d, "IsMaster");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_GetHeroInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_GetHeroInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("AiHeroIndices", BaseJsonMarshaler.EncodeArray(o.AiHeroIndices));
		dictionary.Add("GroupHeroIndices", BaseJsonMarshaler.EncodeArray(o.GroupHeroIndices));
		dictionary.Add("IsMaster", o.IsMaster.ToString());
		return dictionary;
	}
}
