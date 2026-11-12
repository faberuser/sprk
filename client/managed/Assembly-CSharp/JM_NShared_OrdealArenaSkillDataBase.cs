using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_OrdealArenaSkillDataBase
{
	public static OrdealArenaSkillDataBase Parse(string s)
	{
		return Parse(s, new OrdealArenaSkillDataBase());
	}

	public static OrdealArenaSkillDataBase Parse(string s, OrdealArenaSkillDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OrdealArenaSkillDataBase Parse(IDictionary d)
	{
		return Parse(d, new OrdealArenaSkillDataBase());
	}

	public static OrdealArenaSkillDataBase Parse(IDictionary d, OrdealArenaSkillDataBase o)
	{
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		return o;
	}

	public static string EncodeJsonString(OrdealArenaSkillDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OrdealArenaSkillDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("Index", o.Index.ToString());
		return dictionary;
	}
}
