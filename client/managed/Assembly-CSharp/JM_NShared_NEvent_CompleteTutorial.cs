using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_CompleteTutorial
{
	public static CompleteTutorial Parse(string s)
	{
		return Parse(s, new CompleteTutorial());
	}

	public static CompleteTutorial Parse(string s, CompleteTutorial o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CompleteTutorial Parse(IDictionary d)
	{
		return Parse(d, new CompleteTutorial());
	}

	public static CompleteTutorial Parse(IDictionary d, CompleteTutorial o)
	{
		o.TutorialIndex = BaseJsonMarshaler.ParseValueInt(d, "TutorialIndex");
		return o;
	}

	public static string EncodeJsonString(CompleteTutorial o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CompleteTutorial o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TutorialIndex", o.TutorialIndex.ToString());
		return dictionary;
	}
}
