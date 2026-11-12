using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ExtraSkillInfo
{
	public static ExtraSkillInfo Parse(string s)
	{
		return Parse(s, new ExtraSkillInfo());
	}

	public static ExtraSkillInfo Parse(string s, ExtraSkillInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ExtraSkillInfo Parse(IDictionary d)
	{
		return Parse(d, new ExtraSkillInfo());
	}

	public static ExtraSkillInfo Parse(IDictionary d, ExtraSkillInfo o)
	{
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.SkillLevel = BaseJsonMarshaler.ParseValueInt(d, "SkillLevel");
		return o;
	}

	public static string EncodeJsonString(ExtraSkillInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ExtraSkillInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("SkillLevel", o.SkillLevel.ToString());
		return dictionary;
	}
}
