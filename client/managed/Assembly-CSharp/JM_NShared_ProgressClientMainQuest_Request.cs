using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ProgressClientMainQuest;

public static class JM_NShared_ProgressClientMainQuest_Request
{
	public static NShared.ProgressClientMainQuest.Request Parse(string s)
	{
		return Parse(s, new NShared.ProgressClientMainQuest.Request());
	}

	public static NShared.ProgressClientMainQuest.Request Parse(string s, NShared.ProgressClientMainQuest.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ProgressClientMainQuest.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ProgressClientMainQuest.Request());
	}

	public static NShared.ProgressClientMainQuest.Request Parse(IDictionary d, NShared.ProgressClientMainQuest.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ProgressClientMainQuest.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ProgressClientMainQuest.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
