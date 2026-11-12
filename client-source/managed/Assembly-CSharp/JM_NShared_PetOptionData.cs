using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetOptionData
{
	public static PetOptionData Parse(string s)
	{
		return Parse(s, new PetOptionData());
	}

	public static PetOptionData Parse(string s, PetOptionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetOptionData Parse(IDictionary d)
	{
		return Parse(d, new PetOptionData());
	}

	public static PetOptionData Parse(IDictionary d, PetOptionData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.OptionType = BaseJsonMarshaler.ParseEnum<OperationType>(d, "OptionType");
		o.OptionValue0 = BaseJsonMarshaler.ParseValueInt(d, "OptionValue0");
		o.OptionValue1 = BaseJsonMarshaler.ParseValueInt(d, "OptionValue1");
		o.OptionValue2 = BaseJsonMarshaler.ParseValueInt(d, "OptionValue2");
		o.OptionValue3 = BaseJsonMarshaler.ParseValueInt(d, "OptionValue3");
		o.OptionValue4 = BaseJsonMarshaler.ParseValueInt(d, "OptionValue4");
		o.OptionValue5 = BaseJsonMarshaler.ParseValueInt(d, "OptionValue5");
		o.ConditionType = BaseJsonMarshaler.ParseEnum<PetOptionConditionType>(d, "ConditionType");
		o.ConditionValue = BaseJsonMarshaler.ParseArrayCast(d, "ConditionValue", BaseJsonMarshaler.CastToString);
		o.ContentsOptionDescKey = BaseJsonMarshaler.ParseValueString(d, "ContentsOptionDescKey");
		return o;
	}

	public static string EncodeJsonString(PetOptionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetOptionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("OptionType", o.OptionType.ToString());
		dictionary.Add("OptionValue0", o.OptionValue0.ToString());
		dictionary.Add("OptionValue1", o.OptionValue1.ToString());
		dictionary.Add("OptionValue2", o.OptionValue2.ToString());
		dictionary.Add("OptionValue3", o.OptionValue3.ToString());
		dictionary.Add("OptionValue4", o.OptionValue4.ToString());
		dictionary.Add("OptionValue5", o.OptionValue5.ToString());
		dictionary.Add("ConditionType", o.ConditionType.ToString());
		dictionary.Add("ConditionValue", BaseJsonMarshaler.EncodeArray(o.ConditionValue));
		if (o.ContentsOptionDescKey != null)
		{
			dictionary.Add("ContentsOptionDescKey", o.ContentsOptionDescKey);
		}
		return dictionary;
	}
}
