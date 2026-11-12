using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetBonusData
{
	public static PetBonusData Parse(string s)
	{
		return Parse(s, new PetBonusData());
	}

	public static PetBonusData Parse(string s, PetBonusData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetBonusData Parse(IDictionary d)
	{
		return Parse(d, new PetBonusData());
	}

	public static PetBonusData Parse(IDictionary d, PetBonusData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.BonusType = BaseJsonMarshaler.ParseEnum<EventBonusType>(d, "BonusType");
		o.BonusValue0 = BaseJsonMarshaler.ParseValueInt(d, "BonusValue0");
		o.BonusValue1 = BaseJsonMarshaler.ParseValueInt(d, "BonusValue1");
		o.BonusValue2 = BaseJsonMarshaler.ParseValueInt(d, "BonusValue2");
		o.BonusValue3 = BaseJsonMarshaler.ParseValueInt(d, "BonusValue3");
		o.BonusValue4 = BaseJsonMarshaler.ParseValueInt(d, "BonusValue4");
		o.BonusValue5 = BaseJsonMarshaler.ParseValueInt(d, "BonusValue5");
		o.ConditionType = BaseJsonMarshaler.ParseValueString(d, "ConditionType");
		o.ConditionValue = BaseJsonMarshaler.ParseArrayCast(d, "ConditionValue", BaseJsonMarshaler.CastToString);
		o.BonusNameKey = BaseJsonMarshaler.ParseValueString(d, "BonusNameKey");
		o.BonusDescKey = BaseJsonMarshaler.ParseValueString(d, "BonusDescKey");
		return o;
	}

	public static string EncodeJsonString(PetBonusData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetBonusData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("BonusType", o.BonusType.ToString());
		dictionary.Add("BonusValue0", o.BonusValue0.ToString());
		dictionary.Add("BonusValue1", o.BonusValue1.ToString());
		dictionary.Add("BonusValue2", o.BonusValue2.ToString());
		dictionary.Add("BonusValue3", o.BonusValue3.ToString());
		dictionary.Add("BonusValue4", o.BonusValue4.ToString());
		dictionary.Add("BonusValue5", o.BonusValue5.ToString());
		if (o.ConditionType != null)
		{
			dictionary.Add("ConditionType", o.ConditionType);
		}
		dictionary.Add("ConditionValue", BaseJsonMarshaler.EncodeArray(o.ConditionValue));
		if (o.BonusNameKey != null)
		{
			dictionary.Add("BonusNameKey", o.BonusNameKey);
		}
		if (o.BonusDescKey != null)
		{
			dictionary.Add("BonusDescKey", o.BonusDescKey);
		}
		return dictionary;
	}
}
