using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SubQuestData
{
	public static SubQuestData Parse(string s)
	{
		return Parse(s, new SubQuestData());
	}

	public static SubQuestData Parse(string s, SubQuestData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SubQuestData Parse(IDictionary d)
	{
		return Parse(d, new SubQuestData());
	}

	public static SubQuestData Parse(IDictionary d, SubQuestData o)
	{
		o.QuestIndex = BaseJsonMarshaler.ParseValueInt(d, "QuestIndex");
		o.Group = BaseJsonMarshaler.ParseValueInt(d, "Group");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.OldIndex = BaseJsonMarshaler.ParseValueInt(d, "OldIndex");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Desc = BaseJsonMarshaler.ParseValueString(d, "Desc");
		o.Goal = BaseJsonMarshaler.ParseValueString(d, "Goal");
		o.StateType = BaseJsonMarshaler.ParseEnum<QuestActionType>(d, "StateType");
		o.StateValue = BaseJsonMarshaler.ParseArrayCast(d, "StateValue", BaseJsonMarshaler.CastToString);
		o.ClientOnly = BaseJsonMarshaler.ParseValueBool(d, "ClientOnly");
		o.ReqProgress = BaseJsonMarshaler.ParseValueInt(d, "ReqProgress");
		o.AutoComplete = BaseJsonMarshaler.ParseValueBool(d, "AutoComplete");
		o.MoveTaskType = BaseJsonMarshaler.ParseEnum<ClientActivityType>(d, "MoveTaskType");
		o.MoveTaskValue = BaseJsonMarshaler.ParseArrayCast(d, "MoveTaskValue", BaseJsonMarshaler.CastToString);
		o.Variable1 = BaseJsonMarshaler.ParseArrayCast(d, "Variable1", BaseJsonMarshaler.CastToString);
		o.Variable2 = BaseJsonMarshaler.ParseArrayCast(d, "Variable2", BaseJsonMarshaler.CastToString);
		o.Variable3 = BaseJsonMarshaler.ParseArrayCast(d, "Variable3", BaseJsonMarshaler.CastToString);
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		o.TutorialIndex = BaseJsonMarshaler.ParseValueInt(d, "TutorialIndex");
		o.OpenConditionLocalKey = BaseJsonMarshaler.ParseValueString(d, "OpenConditionLocalKey");
		o.VisualRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "VisualRewardIndex");
		return o;
	}

	public static string EncodeJsonString(SubQuestData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SubQuestData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("QuestIndex", o.QuestIndex.ToString());
		dictionary.Add("Group", o.Group.ToString());
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("OldIndex", o.OldIndex.ToString());
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.Desc != null)
		{
			dictionary.Add("Desc", o.Desc);
		}
		if (o.Goal != null)
		{
			dictionary.Add("Goal", o.Goal);
		}
		dictionary.Add("StateType", o.StateType.ToString());
		dictionary.Add("StateValue", BaseJsonMarshaler.EncodeArray(o.StateValue));
		dictionary.Add("ClientOnly", o.ClientOnly.ToString());
		dictionary.Add("ReqProgress", o.ReqProgress.ToString());
		dictionary.Add("AutoComplete", o.AutoComplete.ToString());
		dictionary.Add("MoveTaskType", o.MoveTaskType.ToString());
		dictionary.Add("MoveTaskValue", BaseJsonMarshaler.EncodeArray(o.MoveTaskValue));
		dictionary.Add("Variable1", BaseJsonMarshaler.EncodeArray(o.Variable1));
		dictionary.Add("Variable2", BaseJsonMarshaler.EncodeArray(o.Variable2));
		dictionary.Add("Variable3", BaseJsonMarshaler.EncodeArray(o.Variable3));
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		dictionary.Add("TutorialIndex", o.TutorialIndex.ToString());
		if (o.OpenConditionLocalKey != null)
		{
			dictionary.Add("OpenConditionLocalKey", o.OpenConditionLocalKey);
		}
		dictionary.Add("VisualRewardIndex", o.VisualRewardIndex.ToString());
		return dictionary;
	}
}
