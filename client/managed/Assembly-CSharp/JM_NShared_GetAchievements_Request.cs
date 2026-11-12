using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetAchievements;

public static class JM_NShared_GetAchievements_Request
{
	public static NShared.GetAchievements.Request Parse(string s)
	{
		return Parse(s, new NShared.GetAchievements.Request());
	}

	public static NShared.GetAchievements.Request Parse(string s, NShared.GetAchievements.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetAchievements.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetAchievements.Request());
	}

	public static NShared.GetAchievements.Request Parse(IDictionary d, NShared.GetAchievements.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetAchievements.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetAchievements.Request o)
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
