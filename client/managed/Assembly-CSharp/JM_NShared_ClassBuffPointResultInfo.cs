using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ClassBuffPointResultInfo
{
	public static ClassBuffPointResultInfo Parse(string s)
	{
		return Parse(s, new ClassBuffPointResultInfo());
	}

	public static ClassBuffPointResultInfo Parse(string s, ClassBuffPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClassBuffPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new ClassBuffPointResultInfo());
	}

	public static ClassBuffPointResultInfo Parse(IDictionary d, ClassBuffPointResultInfo o)
	{
		o.NewValue = BaseJsonMarshaler.ParseValueInt(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(ClassBuffPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClassBuffPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}
