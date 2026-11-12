using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ItemGradeData
{
	public static ItemGradeData Parse(string s)
	{
		return Parse(s, new ItemGradeData());
	}

	public static ItemGradeData Parse(string s, ItemGradeData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ItemGradeData Parse(IDictionary d)
	{
		return Parse(d, new ItemGradeData());
	}

	public static ItemGradeData Parse(IDictionary d, ItemGradeData o)
	{
		o.Grade = BaseJsonMarshaler.ParseValueInt(d, "Grade");
		o.Score = BaseJsonMarshaler.ParseValueByte(d, "Score");
		o.GetEffect = BaseJsonMarshaler.ParseValueString(d, "GetEffect");
		o.NameColor = BaseJsonMarshaler.ParseValueString(d, "NameColor");
		return o;
	}

	public static string EncodeJsonString(ItemGradeData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ItemGradeData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Grade", o.Grade.ToString());
		dictionary.Add("Score", o.Score.ToString());
		if (o.GetEffect != null)
		{
			dictionary.Add("GetEffect", o.GetEffect);
		}
		if (o.NameColor != null)
		{
			dictionary.Add("NameColor", o.NameColor);
		}
		return dictionary;
	}
}
