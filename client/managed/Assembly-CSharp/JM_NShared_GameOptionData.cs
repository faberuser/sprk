using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GameOptionData
{
	public static GameOptionData Parse(string s)
	{
		return Parse(s, new GameOptionData());
	}

	public static GameOptionData Parse(string s, GameOptionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GameOptionData Parse(IDictionary d)
	{
		return Parse(d, new GameOptionData());
	}

	public static GameOptionData Parse(IDictionary d, GameOptionData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ParentNum = BaseJsonMarshaler.ParseValueInt(d, "ParentNum");
		o.ChildRoot = BaseJsonMarshaler.ParseValueBool(d, "ChildRoot");
		o.SaveValue = BaseJsonMarshaler.ParseValueBool(d, "SaveValue");
		o.OptionName = BaseJsonMarshaler.ParseValueString(d, "OptionName");
		o.GameOptionTab = BaseJsonMarshaler.ParseEnum<GameOptionTab>(d, "GameOptionTab");
		o.GameOptionComponent = BaseJsonMarshaler.ParseEnum<GameOptionComponent>(d, "GameOptionComponent");
		o.OptionDataType = BaseJsonMarshaler.ParseValueString(d, "OptionDataType");
		o.ClassName = BaseJsonMarshaler.ParseValueString(d, "ClassName");
		o.FieldName = BaseJsonMarshaler.ParseValueString(d, "FieldName");
		o.PostProcessFunction = BaseJsonMarshaler.ParseValueString(d, "PostProcessFunction");
		o.UIdefaultValue = BaseJsonMarshaler.ParseValueString(d, "UIdefaultValue");
		o.ValueLocalKey1 = BaseJsonMarshaler.ParseValueString(d, "ValueLocalKey1");
		o.Value1 = BaseJsonMarshaler.ParseValueString(d, "Value1");
		o.DisableGroup1 = BaseJsonMarshaler.ParseArrayCast(d, "DisableGroup1", BaseJsonMarshaler.CastToInt);
		o.ValueLocalKey2 = BaseJsonMarshaler.ParseValueString(d, "ValueLocalKey2");
		o.Value2 = BaseJsonMarshaler.ParseValueString(d, "Value2");
		o.DisableGroup2 = BaseJsonMarshaler.ParseArrayCast(d, "DisableGroup2", BaseJsonMarshaler.CastToInt);
		o.ValueLocalKey3 = BaseJsonMarshaler.ParseValueString(d, "ValueLocalKey3");
		o.Value3 = BaseJsonMarshaler.ParseValueString(d, "Value3");
		o.DisableGroup3 = BaseJsonMarshaler.ParseArrayCast(d, "DisableGroup3", BaseJsonMarshaler.CastToInt);
		o.ValueLocalKey4 = BaseJsonMarshaler.ParseValueString(d, "ValueLocalKey4");
		o.Value4 = BaseJsonMarshaler.ParseValueString(d, "Value4");
		o.DisableGroup4 = BaseJsonMarshaler.ParseArrayCast(d, "DisableGroup4", BaseJsonMarshaler.CastToInt);
		o.ToolTipDesc = BaseJsonMarshaler.ParseValueString(d, "ToolTipDesc");
		return o;
	}

	public static string EncodeJsonString(GameOptionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GameOptionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ParentNum", o.ParentNum.ToString());
		dictionary.Add("ChildRoot", o.ChildRoot.ToString());
		dictionary.Add("SaveValue", o.SaveValue.ToString());
		if (o.OptionName != null)
		{
			dictionary.Add("OptionName", o.OptionName);
		}
		dictionary.Add("GameOptionTab", o.GameOptionTab.ToString());
		dictionary.Add("GameOptionComponent", o.GameOptionComponent.ToString());
		if (o.OptionDataType != null)
		{
			dictionary.Add("OptionDataType", o.OptionDataType);
		}
		if (o.ClassName != null)
		{
			dictionary.Add("ClassName", o.ClassName);
		}
		if (o.FieldName != null)
		{
			dictionary.Add("FieldName", o.FieldName);
		}
		if (o.PostProcessFunction != null)
		{
			dictionary.Add("PostProcessFunction", o.PostProcessFunction);
		}
		if (o.UIdefaultValue != null)
		{
			dictionary.Add("UIdefaultValue", o.UIdefaultValue);
		}
		if (o.ValueLocalKey1 != null)
		{
			dictionary.Add("ValueLocalKey1", o.ValueLocalKey1);
		}
		if (o.Value1 != null)
		{
			dictionary.Add("Value1", o.Value1);
		}
		dictionary.Add("DisableGroup1", BaseJsonMarshaler.EncodeArray(o.DisableGroup1));
		if (o.ValueLocalKey2 != null)
		{
			dictionary.Add("ValueLocalKey2", o.ValueLocalKey2);
		}
		if (o.Value2 != null)
		{
			dictionary.Add("Value2", o.Value2);
		}
		dictionary.Add("DisableGroup2", BaseJsonMarshaler.EncodeArray(o.DisableGroup2));
		if (o.ValueLocalKey3 != null)
		{
			dictionary.Add("ValueLocalKey3", o.ValueLocalKey3);
		}
		if (o.Value3 != null)
		{
			dictionary.Add("Value3", o.Value3);
		}
		dictionary.Add("DisableGroup3", BaseJsonMarshaler.EncodeArray(o.DisableGroup3));
		if (o.ValueLocalKey4 != null)
		{
			dictionary.Add("ValueLocalKey4", o.ValueLocalKey4);
		}
		if (o.Value4 != null)
		{
			dictionary.Add("Value4", o.Value4);
		}
		dictionary.Add("DisableGroup4", BaseJsonMarshaler.EncodeArray(o.DisableGroup4));
		if (o.ToolTipDesc != null)
		{
			dictionary.Add("ToolTipDesc", o.ToolTipDesc);
		}
		return dictionary;
	}
}
