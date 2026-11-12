using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_TutorialInfo
{
	public static TutorialInfo Parse(string s)
	{
		return Parse(s, new TutorialInfo());
	}

	public static TutorialInfo Parse(string s, TutorialInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static TutorialInfo Parse(IDictionary d)
	{
		return Parse(d, new TutorialInfo());
	}

	public static TutorialInfo Parse(IDictionary d, TutorialInfo o)
	{
		o.TutorialIndex = BaseJsonMarshaler.ParseValueInt(d, "TutorialIndex");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		return o;
	}

	public static string EncodeJsonString(TutorialInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(TutorialInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TutorialIndex", o.TutorialIndex.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		return dictionary;
	}
}
