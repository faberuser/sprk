using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.OpenGodkingTrialDungeon;

public static class JM_NShared_OpenGodkingTrialDungeon_Request
{
	public static NShared.OpenGodkingTrialDungeon.Request Parse(string s)
	{
		return Parse(s, new NShared.OpenGodkingTrialDungeon.Request());
	}

	public static NShared.OpenGodkingTrialDungeon.Request Parse(string s, NShared.OpenGodkingTrialDungeon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.OpenGodkingTrialDungeon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.OpenGodkingTrialDungeon.Request());
	}

	public static NShared.OpenGodkingTrialDungeon.Request Parse(IDictionary d, NShared.OpenGodkingTrialDungeon.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.OpenGodkingTrialDungeon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.OpenGodkingTrialDungeon.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
