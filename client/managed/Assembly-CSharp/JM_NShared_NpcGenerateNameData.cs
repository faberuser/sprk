using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_NpcGenerateNameData
{
	public static NpcGenerateNameData Parse(string s)
	{
		return Parse(s, new NpcGenerateNameData());
	}

	public static NpcGenerateNameData Parse(string s, NpcGenerateNameData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NpcGenerateNameData Parse(IDictionary d)
	{
		return Parse(d, new NpcGenerateNameData());
	}

	public static NpcGenerateNameData Parse(IDictionary d, NpcGenerateNameData o)
	{
		o.Name1 = BaseJsonMarshaler.ParseValueString(d, "Name1");
		o.Name2 = BaseJsonMarshaler.ParseValueString(d, "Name2");
		return o;
	}

	public static string EncodeJsonString(NpcGenerateNameData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NpcGenerateNameData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Name1 != null)
		{
			dictionary.Add("Name1", o.Name1);
		}
		if (o.Name2 != null)
		{
			dictionary.Add("Name2", o.Name2);
		}
		return dictionary;
	}
}
