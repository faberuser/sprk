using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_DefaultExtraStatData
{
	public static DefaultExtraStatData Parse(string s)
	{
		return Parse(s, new DefaultExtraStatData());
	}

	public static DefaultExtraStatData Parse(string s, DefaultExtraStatData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static DefaultExtraStatData Parse(IDictionary d)
	{
		return Parse(d, new DefaultExtraStatData());
	}

	public static DefaultExtraStatData Parse(IDictionary d, DefaultExtraStatData o)
	{
		o.OptionType = BaseJsonMarshaler.ParseEnum<OperationType>(d, "OptionType");
		o.OptionValue = BaseJsonMarshaler.ParseValueInt(d, "OptionValue");
		return o;
	}

	public static string EncodeJsonString(DefaultExtraStatData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(DefaultExtraStatData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OptionType", o.OptionType.ToString());
		dictionary.Add("OptionValue", o.OptionValue.ToString());
		return dictionary;
	}
}
