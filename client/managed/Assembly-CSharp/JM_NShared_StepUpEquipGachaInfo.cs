using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_StepUpEquipGachaInfo
{
	public static StepUpEquipGachaInfo Parse(string s)
	{
		return Parse(s, new StepUpEquipGachaInfo());
	}

	public static StepUpEquipGachaInfo Parse(string s, StepUpEquipGachaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static StepUpEquipGachaInfo Parse(IDictionary d)
	{
		return Parse(d, new StepUpEquipGachaInfo());
	}

	public static StepUpEquipGachaInfo Parse(IDictionary d, StepUpEquipGachaInfo o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.CompletedStep = BaseJsonMarshaler.ParseValueInt(d, "CompletedStep");
		o.CompletedCount = BaseJsonMarshaler.ParseValueInt(d, "CompletedCount");
		o.ProgressTime = BaseJsonMarshaler.ParseValueString(d, "ProgressTime");
		return o;
	}

	public static string EncodeJsonString(StepUpEquipGachaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(StepUpEquipGachaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		dictionary.Add("CompletedStep", o.CompletedStep.ToString());
		dictionary.Add("CompletedCount", o.CompletedCount.ToString());
		if (o.ProgressTime != null)
		{
			dictionary.Add("ProgressTime", o.ProgressTime);
		}
		return dictionary;
	}
}
