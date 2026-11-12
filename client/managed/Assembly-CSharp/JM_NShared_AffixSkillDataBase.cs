using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AffixSkillDataBase
{
	public static AffixSkillDataBase Parse(string s)
	{
		return Parse(s, new AffixSkillDataBase());
	}

	public static AffixSkillDataBase Parse(string s, AffixSkillDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AffixSkillDataBase Parse(IDictionary d)
	{
		return Parse(d, new AffixSkillDataBase());
	}

	public static AffixSkillDataBase Parse(IDictionary d, AffixSkillDataBase o)
	{
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SkillLevel = BaseJsonMarshaler.ParseValueInt(d, "SkillLevel");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.TeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "TeamType");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(AffixSkillDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AffixSkillDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("SkillLevel", o.SkillLevel.ToString());
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("TeamType", o.TeamType.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		return dictionary;
	}
}
