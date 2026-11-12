using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetConquestDungeon;

public static class JM_NShared_ResetConquestDungeon_Request
{
	public static NShared.ResetConquestDungeon.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetConquestDungeon.Request());
	}

	public static NShared.ResetConquestDungeon.Request Parse(string s, NShared.ResetConquestDungeon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetConquestDungeon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetConquestDungeon.Request());
	}

	public static NShared.ResetConquestDungeon.Request Parse(IDictionary d, NShared.ResetConquestDungeon.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetConquestDungeon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetConquestDungeon.Request o)
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
