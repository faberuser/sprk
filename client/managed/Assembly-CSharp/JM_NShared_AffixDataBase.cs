using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AffixDataBase
{
	public static AffixDataBase Parse(string s)
	{
		return Parse(s, new AffixDataBase());
	}

	public static AffixDataBase Parse(string s, AffixDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AffixDataBase Parse(IDictionary d)
	{
		return Parse(d, new AffixDataBase());
	}

	public static AffixDataBase Parse(IDictionary d, AffixDataBase o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.TeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "TeamType");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(AffixDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AffixDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("TeamType", o.TeamType.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		return dictionary;
	}
}
