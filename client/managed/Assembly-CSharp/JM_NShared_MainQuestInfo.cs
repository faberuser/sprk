using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MainQuestInfo
{
	public static MainQuestInfo Parse(string s)
	{
		return Parse(s, new MainQuestInfo());
	}

	public static MainQuestInfo Parse(string s, MainQuestInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MainQuestInfo Parse(IDictionary d)
	{
		return Parse(d, new MainQuestInfo());
	}

	public static MainQuestInfo Parse(IDictionary d, MainQuestInfo o)
	{
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.Progress = BaseJsonMarshaler.ParseValueInt(d, "Progress");
		return o;
	}

	public static string EncodeJsonString(MainQuestInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MainQuestInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("Progress", o.Progress.ToString());
		return dictionary;
	}
}
