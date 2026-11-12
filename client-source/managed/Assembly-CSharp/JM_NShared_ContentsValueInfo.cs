using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContentsValueInfo
{
	public static ContentsValueInfo Parse(string s)
	{
		return Parse(s, new ContentsValueInfo());
	}

	public static ContentsValueInfo Parse(string s, ContentsValueInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContentsValueInfo Parse(IDictionary d)
	{
		return Parse(d, new ContentsValueInfo());
	}

	public static ContentsValueInfo Parse(IDictionary d, ContentsValueInfo o)
	{
		o.Key = BaseJsonMarshaler.ParseValueString(d, "Key");
		o.Value = BaseJsonMarshaler.ParseValueString(d, "Value");
		return o;
	}

	public static string EncodeJsonString(ContentsValueInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContentsValueInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Key != null)
		{
			dictionary.Add("Key", o.Key);
		}
		if (o.Value != null)
		{
			dictionary.Add("Value", o.Value);
		}
		return dictionary;
	}
}
