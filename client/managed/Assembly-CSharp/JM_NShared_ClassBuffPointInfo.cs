using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ClassBuffPointInfo
{
	public static ClassBuffPointInfo Parse(string s)
	{
		return Parse(s, new ClassBuffPointInfo());
	}

	public static ClassBuffPointInfo Parse(string s, ClassBuffPointInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClassBuffPointInfo Parse(IDictionary d)
	{
		return Parse(d, new ClassBuffPointInfo());
	}

	public static ClassBuffPointInfo Parse(IDictionary d, ClassBuffPointInfo o)
	{
		o.TagType = BaseJsonMarshaler.ParseValueInt(d, "TagType");
		o.BuffPoint = BaseJsonMarshaler.ParseValueLong(d, "BuffPoint");
		return o;
	}

	public static string EncodeJsonString(ClassBuffPointInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClassBuffPointInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TagType", o.TagType.ToString());
		dictionary.Add("BuffPoint", o.BuffPoint.ToString());
		return dictionary;
	}
}
