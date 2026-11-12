using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_StaminaConsumePointData
{
	public static StaminaConsumePointData Parse(string s)
	{
		return Parse(s, new StaminaConsumePointData());
	}

	public static StaminaConsumePointData Parse(string s, StaminaConsumePointData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static StaminaConsumePointData Parse(IDictionary d)
	{
		return Parse(d, new StaminaConsumePointData());
	}

	public static StaminaConsumePointData Parse(IDictionary d, StaminaConsumePointData o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.DailyMaxValue = BaseJsonMarshaler.ParseValueInt(d, "DailyMaxValue");
		o.MaxValue = BaseJsonMarshaler.ParseValueInt(d, "MaxValue");
		o.StaminaType = BaseJsonMarshaler.ParseArray<StaminaType>(d, "StaminaType");
		o.Multiple = BaseJsonMarshaler.ParseArrayCast(d, "Multiple", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(StaminaConsumePointData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(StaminaConsumePointData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		dictionary.Add("DailyMaxValue", o.DailyMaxValue.ToString());
		dictionary.Add("MaxValue", o.MaxValue.ToString());
		dictionary.Add("StaminaType", BaseJsonMarshaler.EncodeArray(o.StaminaType));
		dictionary.Add("Multiple", BaseJsonMarshaler.EncodeArray(o.Multiple));
		return dictionary;
	}
}
