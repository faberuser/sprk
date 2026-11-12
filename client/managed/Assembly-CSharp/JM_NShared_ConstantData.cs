using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ConstantData
{
	public static ConstantData Parse(string s)
	{
		return Parse(s, new ConstantData());
	}

	public static ConstantData Parse(string s, ConstantData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ConstantData Parse(IDictionary d)
	{
		return Parse(d, new ConstantData());
	}

	public static ConstantData Parse(IDictionary d, ConstantData o)
	{
		o.Key = BaseJsonMarshaler.ParseValueString(d, "Key");
		o.Value = BaseJsonMarshaler.ParseValueString(d, "Value");
		return o;
	}

	public static string EncodeJsonString(ConstantData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ConstantData o)
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
