using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckAchievements;

public static class JM_NShared_CheckAchievements_Request
{
	public static NShared.CheckAchievements.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckAchievements.Request());
	}

	public static NShared.CheckAchievements.Request Parse(string s, NShared.CheckAchievements.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckAchievements.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckAchievements.Request());
	}

	public static NShared.CheckAchievements.Request Parse(IDictionary d, NShared.CheckAchievements.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckAchievements.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckAchievements.Request o)
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
