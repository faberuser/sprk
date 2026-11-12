using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ContentsOpenConditionData
{
	public static ContentsOpenConditionData Parse(string s)
	{
		return Parse(s, new ContentsOpenConditionData());
	}

	public static ContentsOpenConditionData Parse(string s, ContentsOpenConditionData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ContentsOpenConditionData Parse(IDictionary d)
	{
		return Parse(d, new ContentsOpenConditionData());
	}

	public static ContentsOpenConditionData Parse(IDictionary d, ContentsOpenConditionData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ContentsType = BaseJsonMarshaler.ParseEnum<ContentsType>(d, "ContentsType");
		o.ConditionValue = BaseJsonMarshaler.ParseArrayCast(d, "ConditionValue", BaseJsonMarshaler.CastToInt);
		o.EntryMenuCondition = BaseJsonMarshaler.ParseValueString(d, "EntryMenuCondition");
		o.OpenConditionMainMission = BaseJsonMarshaler.ParseValueInt(d, "OpenConditionMainMission");
		o.OpenText = BaseJsonMarshaler.ParseValueBool(d, "OpenText");
		o.OpenTextResorce = BaseJsonMarshaler.ParseValueString(d, "OpenTextResorce");
		o.OpenLocalKey = BaseJsonMarshaler.ParseValueString(d, "OpenLocalKey");
		o.OpenLocalKeyVariable = BaseJsonMarshaler.ParseArrayCast(d, "OpenLocalKeyVariable", BaseJsonMarshaler.CastToString);
		o.OpenConditionSubQuest = BaseJsonMarshaler.ParseValueInt(d, "OpenConditionSubQuest");
		return o;
	}

	public static string EncodeJsonString(ContentsOpenConditionData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ContentsOpenConditionData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ContentsType", o.ContentsType.ToString());
		dictionary.Add("ConditionValue", BaseJsonMarshaler.EncodeArray(o.ConditionValue));
		if (o.EntryMenuCondition != null)
		{
			dictionary.Add("EntryMenuCondition", o.EntryMenuCondition);
		}
		dictionary.Add("OpenConditionMainMission", o.OpenConditionMainMission.ToString());
		dictionary.Add("OpenText", o.OpenText.ToString());
		if (o.OpenTextResorce != null)
		{
			dictionary.Add("OpenTextResorce", o.OpenTextResorce);
		}
		if (o.OpenLocalKey != null)
		{
			dictionary.Add("OpenLocalKey", o.OpenLocalKey);
		}
		dictionary.Add("OpenLocalKeyVariable", BaseJsonMarshaler.EncodeArray(o.OpenLocalKeyVariable));
		dictionary.Add("OpenConditionSubQuest", o.OpenConditionSubQuest.ToString());
		return dictionary;
	}
}
