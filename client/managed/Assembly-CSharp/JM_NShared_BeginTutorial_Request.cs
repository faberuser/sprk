using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BeginTutorial;

public static class JM_NShared_BeginTutorial_Request
{
	public static NShared.BeginTutorial.Request Parse(string s)
	{
		return Parse(s, new NShared.BeginTutorial.Request());
	}

	public static NShared.BeginTutorial.Request Parse(string s, NShared.BeginTutorial.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BeginTutorial.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BeginTutorial.Request());
	}

	public static NShared.BeginTutorial.Request Parse(IDictionary d, NShared.BeginTutorial.Request o)
	{
		o.TutorialIndex = BaseJsonMarshaler.ParseValueInt(d, "TutorialIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BeginTutorial.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BeginTutorial.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TutorialIndex", o.TutorialIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
