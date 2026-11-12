using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AffixStatDataBase
{
	public static AffixStatDataBase Parse(string s)
	{
		return Parse(s, new AffixStatDataBase());
	}

	public static AffixStatDataBase Parse(string s, AffixStatDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AffixStatDataBase Parse(IDictionary d)
	{
		return Parse(d, new AffixStatDataBase());
	}

	public static AffixStatDataBase Parse(IDictionary d, AffixStatDataBase o)
	{
		o.OptionType = BaseJsonMarshaler.ParseEnum<OperationType>(d, "OptionType");
		o.OptionValue = BaseJsonMarshaler.ParseValueInt(d, "OptionValue");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.TeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "TeamType");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(AffixStatDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AffixStatDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OptionType", o.OptionType.ToString());
		dictionary.Add("OptionValue", o.OptionValue.ToString());
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("TeamType", o.TeamType.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		return dictionary;
	}
}
