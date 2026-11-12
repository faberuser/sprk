using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.LevelUpGuild;

public static class JM_NShared_LevelUpGuild_Request
{
	public static NShared.LevelUpGuild.Request Parse(string s)
	{
		return Parse(s, new NShared.LevelUpGuild.Request());
	}

	public static NShared.LevelUpGuild.Request Parse(string s, NShared.LevelUpGuild.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.LevelUpGuild.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.LevelUpGuild.Request());
	}

	public static NShared.LevelUpGuild.Request Parse(IDictionary d, NShared.LevelUpGuild.Request o)
	{
		o.Level = BaseJsonMarshaler.ParseValueByte(d, "Level");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.LevelUpGuild.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.LevelUpGuild.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Level", o.Level.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
