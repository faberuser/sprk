using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ClassBuffDataBase
{
	public static ClassBuffDataBase Parse(string s)
	{
		return Parse(s, new ClassBuffDataBase());
	}

	public static ClassBuffDataBase Parse(string s, ClassBuffDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClassBuffDataBase Parse(IDictionary d)
	{
		return Parse(d, new ClassBuffDataBase());
	}

	public static ClassBuffDataBase Parse(IDictionary d, ClassBuffDataBase o)
	{
		o.CreatureType = BaseJsonMarshaler.ParseValueInt(d, "CreatureType");
		o.ClassBuffIndex = BaseJsonMarshaler.ParseValueInt(d, "ClassBuffIndex");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		return o;
	}

	public static string EncodeJsonString(ClassBuffDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClassBuffDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureType", o.CreatureType.ToString());
		dictionary.Add("ClassBuffIndex", o.ClassBuffIndex.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		return dictionary;
	}
}
