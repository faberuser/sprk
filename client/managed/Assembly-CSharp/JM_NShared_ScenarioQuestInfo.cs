using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ScenarioQuestInfo
{
	public static ScenarioQuestInfo Parse(string s)
	{
		return Parse(s, new ScenarioQuestInfo());
	}

	public static ScenarioQuestInfo Parse(string s, ScenarioQuestInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ScenarioQuestInfo Parse(IDictionary d)
	{
		return Parse(d, new ScenarioQuestInfo());
	}

	public static ScenarioQuestInfo Parse(IDictionary d, ScenarioQuestInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Progress = BaseJsonMarshaler.ParseValueInt(d, "Progress");
		return o;
	}

	public static string EncodeJsonString(ScenarioQuestInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ScenarioQuestInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Progress", o.Progress.ToString());
		return dictionary;
	}
}
