using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_InspectUserData
{
	public static InspectUserData Parse(string s)
	{
		return Parse(s, new InspectUserData());
	}

	public static InspectUserData Parse(string s, InspectUserData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InspectUserData Parse(IDictionary d)
	{
		return Parse(d, new InspectUserData());
	}

	public static InspectUserData Parse(IDictionary d, InspectUserData o)
	{
		o.Category = BaseJsonMarshaler.ParseEnum<InspectUserCategory>(d, "Category");
		o.Type = BaseJsonMarshaler.ParseEnum<InspectUserType>(d, "Type");
		o.StringKey = BaseJsonMarshaler.ParseValueString(d, "StringKey");
		return o;
	}

	public static string EncodeJsonString(InspectUserData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InspectUserData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Category", o.Category.ToString());
		dictionary.Add("Type", o.Type.ToString());
		if (o.StringKey != null)
		{
			dictionary.Add("StringKey", o.StringKey);
		}
		return dictionary;
	}
}
