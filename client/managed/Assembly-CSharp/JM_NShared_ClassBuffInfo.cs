using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ClassBuffInfo
{
	public static ClassBuffInfo Parse(string s)
	{
		return Parse(s, new ClassBuffInfo());
	}

	public static ClassBuffInfo Parse(string s, ClassBuffInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClassBuffInfo Parse(IDictionary d)
	{
		return Parse(d, new ClassBuffInfo());
	}

	public static ClassBuffInfo Parse(IDictionary d, ClassBuffInfo o)
	{
		o.TagType = BaseJsonMarshaler.ParseValueInt(d, "TagType");
		o.ClassBuffIndex = BaseJsonMarshaler.ParseValueInt(d, "ClassBuffIndex");
		o.ClassBuffLevel = BaseJsonMarshaler.ParseValueInt(d, "ClassBuffLevel");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(ClassBuffInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClassBuffInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TagType", o.TagType.ToString());
		dictionary.Add("ClassBuffIndex", o.ClassBuffIndex.ToString());
		dictionary.Add("ClassBuffLevel", o.ClassBuffLevel.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
