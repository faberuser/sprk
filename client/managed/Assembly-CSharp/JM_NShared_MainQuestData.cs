using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MainQuestData
{
	public static MainQuestData Parse(string s)
	{
		return Parse(s, new MainQuestData());
	}

	public static MainQuestData Parse(string s, MainQuestData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MainQuestData Parse(IDictionary d)
	{
		return Parse(d, new MainQuestData());
	}

	public static MainQuestData Parse(IDictionary d, MainQuestData o)
	{
		o.QuestIndex = BaseJsonMarshaler.ParseValueInt(d, "QuestIndex");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.Desc = BaseJsonMarshaler.ParseValueString(d, "Desc");
		o.BackgroundPath = BaseJsonMarshaler.ParseValueString(d, "BackgroundPath");
		o.Variable1 = BaseJsonMarshaler.ParseArrayCast(d, "Variable1", BaseJsonMarshaler.CastToString);
		o.Variable2 = BaseJsonMarshaler.ParseArrayCast(d, "Variable2", BaseJsonMarshaler.CastToString);
		o.StateType = BaseJsonMarshaler.ParseEnum<QuestActionType>(d, "StateType");
		o.StateValue = BaseJsonMarshaler.ParseArrayCast(d, "StateValue", BaseJsonMarshaler.CastToString);
		o.ClientOnly = BaseJsonMarshaler.ParseValueBool(d, "ClientOnly");
		o.ReqProgress = BaseJsonMarshaler.ParseValueInt(d, "ReqProgress");
		o.AutoComplete = BaseJsonMarshaler.ParseValueBool(d, "AutoComplete");
		o.MoveTaskType = BaseJsonMarshaler.ParseValueString(d, "MoveTaskType");
		o.MoveTaskValue = BaseJsonMarshaler.ParseArrayCast(d, "MoveTaskValue", BaseJsonMarshaler.CastToString);
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		return o;
	}

	public static string EncodeJsonString(MainQuestData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MainQuestData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("QuestIndex", o.QuestIndex.ToString());
		dictionary.Add("Step", o.Step.ToString());
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		if (o.Desc != null)
		{
			dictionary.Add("Desc", o.Desc);
		}
		if (o.BackgroundPath != null)
		{
			dictionary.Add("BackgroundPath", o.BackgroundPath);
		}
		dictionary.Add("Variable1", BaseJsonMarshaler.EncodeArray(o.Variable1));
		dictionary.Add("Variable2", BaseJsonMarshaler.EncodeArray(o.Variable2));
		dictionary.Add("StateType", o.StateType.ToString());
		dictionary.Add("StateValue", BaseJsonMarshaler.EncodeArray(o.StateValue));
		dictionary.Add("ClientOnly", o.ClientOnly.ToString());
		dictionary.Add("ReqProgress", o.ReqProgress.ToString());
		dictionary.Add("AutoComplete", o.AutoComplete.ToString());
		if (o.MoveTaskType != null)
		{
			dictionary.Add("MoveTaskType", o.MoveTaskType);
		}
		dictionary.Add("MoveTaskValue", BaseJsonMarshaler.EncodeArray(o.MoveTaskValue));
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		return dictionary;
	}
}
