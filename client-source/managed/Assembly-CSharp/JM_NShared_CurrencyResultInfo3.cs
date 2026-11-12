using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CurrencyResultInfo3
{
	public static CurrencyResultInfo3 Parse(string s)
	{
		return Parse(s, new CurrencyResultInfo3());
	}

	public static CurrencyResultInfo3 Parse(string s, CurrencyResultInfo3 o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CurrencyResultInfo3 Parse(IDictionary d)
	{
		return Parse(d, new CurrencyResultInfo3());
	}

	public static CurrencyResultInfo3 Parse(IDictionary d, CurrencyResultInfo3 o)
	{
		o.CurrencyType = BaseJsonMarshaler.ParseEnum<CurrencyType>(d, "CurrencyType");
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		o.Field1 = BaseJsonMarshaler.ParseValueString(d, "Field1");
		o.Value1 = BaseJsonMarshaler.ParseValueLong(d, "Value1");
		o.Field2 = BaseJsonMarshaler.ParseValueString(d, "Field2");
		o.Value2 = BaseJsonMarshaler.ParseValueLong(d, "Value2");
		o.Field3 = BaseJsonMarshaler.ParseValueString(d, "Field3");
		o.Value3 = BaseJsonMarshaler.ParseValueLong(d, "Value3");
		o.Field4 = BaseJsonMarshaler.ParseValueString(d, "Field4");
		o.Value4 = BaseJsonMarshaler.ParseValueLong(d, "Value4");
		return o;
	}

	public static string EncodeJsonString(CurrencyResultInfo3 o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CurrencyResultInfo3 o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CurrencyType", o.CurrencyType.ToString());
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		if (o.Field1 != null)
		{
			dictionary.Add("Field1", o.Field1);
		}
		dictionary.Add("Value1", o.Value1.ToString());
		if (o.Field2 != null)
		{
			dictionary.Add("Field2", o.Field2);
		}
		dictionary.Add("Value2", o.Value2.ToString());
		if (o.Field3 != null)
		{
			dictionary.Add("Field3", o.Field3);
		}
		dictionary.Add("Value3", o.Value3.ToString());
		if (o.Field4 != null)
		{
			dictionary.Add("Field4", o.Field4);
		}
		dictionary.Add("Value4", o.Value4.ToString());
		return dictionary;
	}
}
