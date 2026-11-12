using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_OptionSellData
{
	public static OptionSellData Parse(string s)
	{
		return Parse(s, new OptionSellData());
	}

	public static OptionSellData Parse(string s, OptionSellData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OptionSellData Parse(IDictionary d)
	{
		return Parse(d, new OptionSellData());
	}

	public static OptionSellData Parse(IDictionary d, OptionSellData o)
	{
		o.Category = BaseJsonMarshaler.ParseEnum<OptionSellCategory>(d, "Category");
		o.Value = BaseJsonMarshaler.ParseValueInt(d, "Value");
		o.BackGroundIndex = BaseJsonMarshaler.ParseValueInt(d, "BackGroundIndex");
		o.GradeStringKey = BaseJsonMarshaler.ParseValueString(d, "GradeStringKey");
		o.ReplaceKey = BaseJsonMarshaler.ParseValueString(d, "ReplaceKey");
		o.SelectType = BaseJsonMarshaler.ParseEnum<OptionSellSelectType>(d, "SelectType");
		o.GradeDependency = BaseJsonMarshaler.ParseArrayCast(d, "GradeDependency", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(OptionSellData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OptionSellData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Category", o.Category.ToString());
		dictionary.Add("Value", o.Value.ToString());
		dictionary.Add("BackGroundIndex", o.BackGroundIndex.ToString());
		if (o.GradeStringKey != null)
		{
			dictionary.Add("GradeStringKey", o.GradeStringKey);
		}
		if (o.ReplaceKey != null)
		{
			dictionary.Add("ReplaceKey", o.ReplaceKey);
		}
		dictionary.Add("SelectType", o.SelectType.ToString());
		dictionary.Add("GradeDependency", BaseJsonMarshaler.EncodeArray(o.GradeDependency));
		return dictionary;
	}
}
