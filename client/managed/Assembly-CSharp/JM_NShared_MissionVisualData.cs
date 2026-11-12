using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MissionVisualData
{
	public static MissionVisualData Parse(string s)
	{
		return Parse(s, new MissionVisualData());
	}

	public static MissionVisualData Parse(string s, MissionVisualData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MissionVisualData Parse(IDictionary d)
	{
		return Parse(d, new MissionVisualData());
	}

	public static MissionVisualData Parse(IDictionary d, MissionVisualData o)
	{
		o.MainCategoryIndex = BaseJsonMarshaler.ParseValueInt(d, "MainCategoryIndex");
		o.SubCategoryIndex = BaseJsonMarshaler.ParseValueInt(d, "SubCategoryIndex");
		o.QuestType = BaseJsonMarshaler.ParseValueString(d, "QuestType");
		o.QuestIndex = BaseJsonMarshaler.ParseValueInt(d, "QuestIndex");
		o.MissionGradeValue = BaseJsonMarshaler.ParseValueInt(d, "MissionGradeValue");
		o.MainStep = BaseJsonMarshaler.ParseValueInt(d, "MainStep");
		o.SubStep = BaseJsonMarshaler.ParseValueInt(d, "SubStep");
		o.NodeIconPath = BaseJsonMarshaler.ParseValueString(d, "NodeIconPath");
		o.NodeIconLocalKey = BaseJsonMarshaler.ParseValueString(d, "NodeIconLocalKey");
		o.MilestonePopupNpcCode = BaseJsonMarshaler.ParseValueString(d, "MilestonePopupNpcCode");
		o.DetailDisplayLocalize = BaseJsonMarshaler.ParseArrayCast(d, "DetailDisplayLocalize", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(MissionVisualData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MissionVisualData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MainCategoryIndex", o.MainCategoryIndex.ToString());
		dictionary.Add("SubCategoryIndex", o.SubCategoryIndex.ToString());
		if (o.QuestType != null)
		{
			dictionary.Add("QuestType", o.QuestType);
		}
		dictionary.Add("QuestIndex", o.QuestIndex.ToString());
		dictionary.Add("MissionGradeValue", o.MissionGradeValue.ToString());
		dictionary.Add("MainStep", o.MainStep.ToString());
		dictionary.Add("SubStep", o.SubStep.ToString());
		if (o.NodeIconPath != null)
		{
			dictionary.Add("NodeIconPath", o.NodeIconPath);
		}
		if (o.NodeIconLocalKey != null)
		{
			dictionary.Add("NodeIconLocalKey", o.NodeIconLocalKey);
		}
		if (o.MilestonePopupNpcCode != null)
		{
			dictionary.Add("MilestonePopupNpcCode", o.MilestonePopupNpcCode);
		}
		dictionary.Add("DetailDisplayLocalize", BaseJsonMarshaler.EncodeArray(o.DetailDisplayLocalize));
		return dictionary;
	}
}
