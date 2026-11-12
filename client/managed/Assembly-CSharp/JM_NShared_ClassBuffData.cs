using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ClassBuffData
{
	public static ClassBuffData Parse(string s)
	{
		return Parse(s, new ClassBuffData());
	}

	public static ClassBuffData Parse(string s, ClassBuffData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClassBuffData Parse(IDictionary d)
	{
		return Parse(d, new ClassBuffData());
	}

	public static ClassBuffData Parse(IDictionary d, ClassBuffData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.TagType = BaseJsonMarshaler.ParseEnum<CreatureTagType>(d, "TagType");
		o.ClassBuffLevel = BaseJsonMarshaler.ParseValueInt(d, "ClassBuffLevel");
		o.NeedPoint = BaseJsonMarshaler.ParseValueLong(d, "NeedPoint");
		o.NeedClassBuffIndex = BaseJsonMarshaler.ParseValueInt(d, "NeedClassBuffIndex");
		o.NeedClassBuffLevel = BaseJsonMarshaler.ParseValueInt(d, "NeedClassBuffLevel");
		o.NeedHeroCount = BaseJsonMarshaler.ParseValueInt(d, "NeedHeroCount");
		o.ClassBuffIconPath = BaseJsonMarshaler.ParseValueString(d, "ClassBuffIconPath");
		o.BuffOptionTitleName = BaseJsonMarshaler.ParseValueString(d, "BuffOptionTitleName");
		o.BuffOptionTitleDesc = BaseJsonMarshaler.ParseValueString(d, "BuffOptionTitleDesc");
		o.StatUsePercentage = BaseJsonMarshaler.ParseValueByte(d, "StatUsePercentage");
		o.Grade = BaseJsonMarshaler.ParseValueByte(d, "Grade");
		o.CreatureType = BaseJsonMarshaler.ParseValueInt(d, "CreatureType");
		o.ClassBuffIndex = BaseJsonMarshaler.ParseValueInt(d, "ClassBuffIndex");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		return o;
	}

	public static string EncodeJsonString(ClassBuffData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClassBuffData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("TagType", o.TagType.ToString());
		dictionary.Add("ClassBuffLevel", o.ClassBuffLevel.ToString());
		dictionary.Add("NeedPoint", o.NeedPoint.ToString());
		dictionary.Add("NeedClassBuffIndex", o.NeedClassBuffIndex.ToString());
		dictionary.Add("NeedClassBuffLevel", o.NeedClassBuffLevel.ToString());
		dictionary.Add("NeedHeroCount", o.NeedHeroCount.ToString());
		if (o.ClassBuffIconPath != null)
		{
			dictionary.Add("ClassBuffIconPath", o.ClassBuffIconPath);
		}
		if (o.BuffOptionTitleName != null)
		{
			dictionary.Add("BuffOptionTitleName", o.BuffOptionTitleName);
		}
		if (o.BuffOptionTitleDesc != null)
		{
			dictionary.Add("BuffOptionTitleDesc", o.BuffOptionTitleDesc);
		}
		dictionary.Add("StatUsePercentage", o.StatUsePercentage.ToString());
		dictionary.Add("Grade", o.Grade.ToString());
		dictionary.Add("CreatureType", o.CreatureType.ToString());
		dictionary.Add("ClassBuffIndex", o.ClassBuffIndex.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		return dictionary;
	}
}
